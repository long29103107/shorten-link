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

app.UseShortenLinkRateLimiting();

app.MapShortenLinkEndpoints();
app.MapApiHostEndpoints();

app.Run();

public partial class Program;
