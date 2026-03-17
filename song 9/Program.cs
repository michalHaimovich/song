using SongApi.interfaces;
using Microsoft.OpenApi.Models;
using SongApi.Services;
using SongApi.Models;
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
builder.Services.addGenericRepository<Song>();
builder.Services.addGenericRepository<User>();
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
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    // 1. הגדרת כפתור ה-Authorize והחלון הקופץ
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "הכנס את הטוקן שקיבלת מהלוגין לכאן.\n\nלא צריך לכתוב את המילה Bearer, פשוט להדביק את הטוקן עצמו.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // 2. הגדרה שכל פעולה ב-Swagger תדרוש את הטוקן הזה
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); 
builder.Logging.SetMinimumLevel(LogLevel.Debug);


var app = builder.Build();

// app.Run(async context => await context.Response.WriteAsync("our no-map terminal 2nd middleware!\n"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   // app.MapOpenApi();
    //
     app.UseDeveloperExceptionPage();
     app.UseSwagger();
    //
     app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
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
