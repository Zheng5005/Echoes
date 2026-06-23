using Echoes.Server.ConnectionManager;
using Echoes.Server.Endpoints;
using Echoes.Server.Hubs;
using Echoes.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<UserConnectionManager>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// --- Middleware pipeline ---
app.UseCookiePolicy();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// --- Endpoint groups ---
app.MapAuthEndpoints();
app.MapMessageEndpoints();

// --- SignalR hub ---
app.MapHub<ChatHub>("/hub/chat");

app.Run();
