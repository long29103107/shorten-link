using ShortenLink.AspNetCore;

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

app.MapShortenLinkEndpoints();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    app = "ShortenLink.Api"
}))
.WithName("Health");

app.Run();
