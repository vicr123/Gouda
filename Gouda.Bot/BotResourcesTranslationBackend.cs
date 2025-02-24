using System.Reflection;
using System.Text.Json;
using I18Next.Net.Backends;
using I18Next.Net.TranslationTrees;

namespace Gouda.Bot;

public class BotResourcesTranslationBackend : ITranslationBackend
{
    private readonly ITranslationTreeBuilderFactory _treeBuilderFactory = new GenericTranslationTreeBuilderFactory<HierarchicalTranslationTreeBuilder>();

    public async Task<ITranslationTree> LoadNamespaceAsync(string language, string @namespace)
    {
        var json = await BotLocales.BotLocales.ReadTranslationFileAsync(language, @namespace);
        if (json == null)
        {
            return _treeBuilderFactory.Create().Build();
        }

        var treeBuilder = _treeBuilderFactory.Create();
        PopulateTreeBuilder(string.Empty, json, treeBuilder);
        return treeBuilder.Build();
    }

    private static void PopulateTreeBuilder(
        string path,
        Dictionary<string, JsonElement> node,
        ITranslationTreeBuilder builder)
    {
        if (path != string.Empty)
        {
            path += ".";
        }

        foreach (var keyValuePair in node)
        {
            var str = path + keyValuePair.Key;
            if (keyValuePair.Value.GetString() is { } translation)
            {
                builder.AddTranslation(str, translation);
            }
            else if (keyValuePair.Value.Deserialize<Dictionary<string, JsonElement>>() is { } @object)
            {
                PopulateTreeBuilder(str, @object, builder);
            }
        }
    }
}
