using AspNet.Security.OAuth.Discord;
using Gouda.ApiService.Services;
using Gouda.Database;
using Gouda.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.API;
using Remora.Discord.Caching.Services;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Rest.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddDiscord(o =>
    {
        o.ClientId = builder.Configuration["Gouda:ClientId"]!;
        o.ClientSecret = builder.Configuration["Gouda:ClientSecret"]!;
        o.CallbackPath = "/api/auth/callback";
        o.AccessDeniedPath = "/";
        o.ReturnUrlParameter = string.Empty;
        o.SaveTokens = true;

        o.Scope.Add("identify");
        o.Scope.Add("guilds");
    });

builder.Services.AddDiscordService(_ => builder.Configuration["Gouda:DiscordToken"]!);
builder.Services.AddScoped<DiscordUserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.TryAddSingleton<CacheService>();
builder.Services.AddOptions<CacheSettings>();
builder.Services.AddSingleton<ImmutableCacheSettings>();
builder.Services
    .Decorate<IDiscordRestChannelAPI, CachingDiscordRestChannelAPI>()
    .Decorate<IDiscordRestEmojiAPI, CachingDiscordRestEmojiAPI>()
    .Decorate<IDiscordRestGuildAPI, CachingDiscordRestGuildAPI>()
    .Decorate<IDiscordRestInteractionAPI, CachingDiscordRestInteractionAPI>()
    .Decorate<IDiscordRestInviteAPI, CachingDiscordRestInviteAPI>()
    .Decorate<IDiscordRestOAuth2API, CachingDiscordRestOAuth2API>()
    .Decorate<IDiscordRestTemplateAPI, CachingDiscordRestTemplateAPI>()
    // .Decorate<IDiscordRestUserAPI, CachingDiscordRestUserAPI>()
    .Decorate<IDiscordRestVoiceAPI, CachingDiscordRestVoiceAPI>()
    .Decorate<IDiscordRestWebhookAPI, CachingDiscordRestWebhookAPI>();

builder.AddNpgsqlDbContext<GoudaDbContext>(connectionName: "gouda-db");

var app = builder.Build();

app.UseForwardedHeaders();
app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.MapDefaultEndpoints();

app.Run();
