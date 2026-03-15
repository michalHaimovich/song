using SongApi.interfaces;
using Microsoft.OpenApi.Models;
using SongApi.Services;
using MyMiddleware;
using Token.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SongApi.Hubs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();


// Add services to the container.
builder.Services.addSongService();
builder.Services.addUserService();
builder.Services.AddControllers();
builder.Services.AddActiveUser();
builder.Services.AddHttpContextAccessor();
builder.Services.addSongRepository();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ILogQueueService, LogQueueService>();
builder.Services.AddHostedService<LogWorker>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = TokenService.GetTokenValidationParameters();

    // --- התוספת הקריטית עבור SignalR ---
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // אם הבקשה מגיעה ל-Hub שלנו ויש לה טוקן ב-URL
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/activityHub"))
            {
                // תגיד ל-.NET להשתמש בטוקן הזה עבור ההתחברות
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.SetMinimumLevel(LogLevel.Debug);


var app = builder.Build();

// app.Run(async context => await context.Response.WriteAsync("our no-map terminal 2nd middleware!\n"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //
     app.UseDeveloperExceptionPage();
     app.UseSwagger();
    //
     app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}



app.UseDefaultFiles();

app.UseStaticFiles();

app.UseHttpsRedirection();
//
app.UseAuthentication();
//
app.UseAuthorization();

app.UseMyLogMiddleware();

app.MapControllers();

app.MapHub<ActivityHub>("/activityHub");

app.Run();
