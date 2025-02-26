using Microsoft.AspNetCore.Authentication;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Rest.Extensions;
using Remora.Rest;

namespace Gouda.ApiService;

public static class RemoraExtensions
{
    public static async Task<RestRequestCustomization> Userify(this IRestCustomizable customisable, HttpContext httpContext)
    {
        var token = await httpContext.GetTokenAsync("access_token");
        return customisable.WithCustomization(c =>
        {
            c.SkipAuthorization();
            c.With(d => d.Headers.Authorization = new("Bearer", token));
        });
    }

    public static Task<RestRequestCustomization> Userify(this IDiscordRestUserAPI customisable, HttpContext httpContext)
    {
        return ((IRestCustomizable)customisable).Userify(httpContext);
    }

    public static Task<RestRequestCustomization> Userify(this IDiscordRestGuildAPI customisable, HttpContext httpContext)
    {
        return ((IRestCustomizable)customisable).Userify(httpContext);
    }
}
