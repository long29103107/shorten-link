[CmdletBinding()]
param(
    [string]$ConnectionString = "Host=localhost;Port=5432;Database=shorten_link;Username=postgres;Password=postgres",
    [string]$ApiUrl = "http://127.0.0.1:5199",
    [string]$AliasPrefix = "pgsmoke"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ConnectionSetting {
    param(
        [string]$Text,
        [string]$Key,
        [string]$DefaultValue
    )

    $pattern = "(?:^|;)\s*$([Regex]::Escape($Key))\s*=\s*([^;]+)"
    $match = [Regex]::Match($Text, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($match.Success) {
        return $match.Groups[1].Value.Trim()
    }

    return $DefaultValue
}

function Invoke-Json {
    param(
        [System.Net.Http.HttpClient]$Client,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null
    )

    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::$Method, $Url)
    if ($null -ne $Body) {
        $json = $Body | ConvertTo-Json -Depth 10
        $request.Content = [System.Net.Http.StringContent]::new($json, [System.Text.Encoding]::UTF8, "application/json")
    }

    return $Client.SendAsync($request).GetAwaiter().GetResult()
}

$hostName = Get-ConnectionSetting -Text $ConnectionString -Key "Host" -DefaultValue "localhost"
$portText = Get-ConnectionSetting -Text $ConnectionString -Key "Port" -DefaultValue "5432"
$port = 5432
[void][int]::TryParse($portText, [ref]$port)

$tcpProbe = Test-NetConnection -ComputerName $hostName -Port $port -WarningAction SilentlyContinue
if (-not $tcpProbe.TcpTestSucceeded) {
    throw "PostgreSQL is not reachable at ${hostName}:$port. Start a PostgreSQL instance first, then rerun this script."
}

$apiUri = [Uri]::new($ApiUrl)
$projectPath = Join-Path $PSScriptRoot "..\src\ShortenLink.Api\ShortenLink.Api.csproj"
$workingDirectory = Split-Path -Parent $projectPath

$stdout = Join-Path ([System.IO.Path]::GetTempPath()) ("shorten-link-postgres-smoke-" + [Guid]::NewGuid().ToString("N") + ".out.log")
$stderr = Join-Path ([System.IO.Path]::GetTempPath()) ("shorten-link-postgres-smoke-" + [Guid]::NewGuid().ToString("N") + ".err.log")

$originalEnvironment = @{
    "ASPNETCORE_ENVIRONMENT" = $env:ASPNETCORE_ENVIRONMENT
    "ASPNETCORE_URLS" = $env:ASPNETCORE_URLS
    "ShortenLink__BaseUrl" = $env:ShortenLink__BaseUrl
    "ShortenLink__Database__UsePostgres" = $env:ShortenLink__Database__UsePostgres
    "ShortenLink__Database__PostgresConnectionString" = $env:ShortenLink__Database__PostgresConnectionString
    "ShortenLink__Redirect__EnableFrontendFallback" = $env:ShortenLink__Redirect__EnableFrontendFallback
}

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = $ApiUrl
$env:ShortenLink__BaseUrl = ($ApiUrl.TrimEnd("/") + "/")
$env:ShortenLink__Database__UsePostgres = "true"
$env:ShortenLink__Database__PostgresConnectionString = $ConnectionString
$env:ShortenLink__Redirect__EnableFrontendFallback = "false"

$process = $null
$client = $null

try {
    $process = Start-Process -FilePath "dotnet" `
        -ArgumentList @("run", "--project", $projectPath, "--no-launch-profile") `
        -WorkingDirectory $workingDirectory `
        -PassThru `
        -WindowStyle Hidden `
        -RedirectStandardOutput $stdout `
        -RedirectStandardError $stderr

    $handler = [System.Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromSeconds(5)

    $healthUrl = ($ApiUrl.TrimEnd("/") + "/api/health")
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        Start-Sleep -Milliseconds 500

        if ($process.HasExited) {
            break
        }

        try {
            $healthResponse = Invoke-Json -Client $client -Method "Get" -Url $healthUrl
            if ($healthResponse.IsSuccessStatusCode) {
                $ready = $true
                break
            }
        } catch {
        }
    }

    if (-not $ready) {
        $stdOutText = if (Test-Path $stdout) { Get-Content -LiteralPath $stdout -Raw } else { "" }
        $stdErrText = if (Test-Path $stderr) { Get-Content -LiteralPath $stderr -Raw } else { "" }
        throw "API did not become ready with PostgreSQL enabled.`nSTDOUT:`n$stdOutText`nSTDERR:`n$stdErrText"
    }

    $alias = "$AliasPrefix-$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())"
    $createResponse = Invoke-Json -Client $client -Method "Post" -Url ($ApiUrl.TrimEnd("/") + "/api/short-links") -Body @{
        originalUrl = "https://example.com/postgres-smoke"
        customAlias = $alias
    }

    if ([int]$createResponse.StatusCode -ne 201) {
        throw "Create short link failed with status $([int]$createResponse.StatusCode)."
    }

    $created = ($createResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json)
    $detailResponse = Invoke-Json -Client $client -Method "Get" -Url ($ApiUrl.TrimEnd("/") + "/api/short-links/$alias")
    $detail = ($detailResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult() | ConvertFrom-Json)
    $redirectResponse = Invoke-Json -Client $client -Method "Get" -Url ($ApiUrl.TrimEnd("/") + "/$alias")
    $deleteResponse = Invoke-Json -Client $client -Method "Delete" -Url ($ApiUrl.TrimEnd("/") + "/api/short-links/$alias")
    $postDeleteRedirect = Invoke-Json -Client $client -Method "Get" -Url ($ApiUrl.TrimEnd("/") + "/$alias")

    [pscustomobject]@{
        Status = "Completed"
        ApiUrl = $ApiUrl
        Alias = $alias
        CreateStatus = [int]$createResponse.StatusCode
        DetailStatus = [int]$detailResponse.StatusCode
        RedirectStatus = [int]$redirectResponse.StatusCode
        RedirectLocation = $redirectResponse.Headers.Location.AbsoluteUri
        DeleteStatus = [int]$deleteResponse.StatusCode
        PostDeleteRedirectStatus = [int]$postDeleteRedirect.StatusCode
        ShortUrl = $created.shortUrl
        OriginalUrl = $detail.originalUrl
        IsActive = $detail.isActive
    } | ConvertTo-Json -Depth 5
}
finally {
    if ($null -ne $client) {
        $client.Dispose()
    }

    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $process.WaitForExit()
    }

    foreach ($entry in $originalEnvironment.GetEnumerator()) {
        if ($null -eq $entry.Value) {
            Remove-Item "Env:$($entry.Key)" -ErrorAction SilentlyContinue
        }
        else {
            Set-Item "Env:$($entry.Key)" -Value $entry.Value
        }
    }
}
