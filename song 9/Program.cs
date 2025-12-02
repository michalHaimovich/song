using WEBAPI.interfaces;
using SongHomeWork.service;
using MyMiddleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container.
builder.Services.addSongService();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();

// app.Run(async context => await context.Response.WriteAsync("our no-map terminal 2nd middleware!\n"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
     app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseMyLogMiddleware();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
