using ShortenLink.AspNetCore;
using ShortenLink.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddShortenLink(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseRateLimiter();

app.MapShortLinkManagementEndpoints();
app.MapRedirectEndpoints();
app.MapSecuritySessionEndpoints();
app.MapSecurityApiKeyEndpoints();
app.MapSecurityRoleEndpoints();
app.MapSecurityUserEndpoints();
app.MapSecurityAssignmentEndpoints();
app.MapHealthEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapMockDataEndpoints();
}

app.Run();

public partial class Program;
