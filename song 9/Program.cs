using WEBAPI.interfaces;
using SongHomeWork.service;
using MyMiddleware;
using MyIuser.interfaces;
using MyUserSe.Service;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();


// Add services to the container.
builder.Services.addSongService();
builder.Services.addUserService();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.SetMinimumLevel(LogLevel.Debug);


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
