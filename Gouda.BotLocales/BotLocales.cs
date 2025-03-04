using System.Reflection;
using System.Text.Json;

namespace Gouda.BotLocales;

public class BotLocales
{
    public static async Task<Dictionary<string, JsonElement>?> ReadTranslationFileAsync(string language, string @namespace = "translation")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Gouda.BotLocales.Translations.{language.Replace("-", "_")}.{@namespace}.json";

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return null;
        }

        var json = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(stream);
        if (json == null)
        {
            return null;
        }

        return json;
    }

    public static IEnumerable<string> AvailableLanguages()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith("Gouda.BotLocales.Translations."))
            .Select(n => n.Replace("Gouda.BotLocales.Translations.", string.Empty))
            .Select(n => n[..n.IndexOf('.')].Replace("_", "-"));
    }
}
