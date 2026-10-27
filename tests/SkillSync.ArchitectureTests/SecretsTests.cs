using System.Text.Json;

namespace SkillSync.ArchitectureTests;

public class SecretsTests
{
    private static readonly string[] SecretWords = ["password", "secret", "signingkey", "apikey", "token", "privatekey"];

    [Fact]
    public void Appsettings_files_hold_no_secrets()
    {
        var files = Directory.GetFiles(ModuleCatalog.RepositoryRoot(), "appsettings*.json", SearchOption.AllDirectories)
            .Where(f => !f.Replace('\\', '/').Contains("/bin/") && !f.Replace('\\', '/').Contains("/obj/"));

        var problems = new List<string>();
        var options = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

        foreach (var file in files)
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(file), options);
            Walk(doc.RootElement, "", Path.GetFileName(file), problems);
        }

        Assert.True(problems.Count == 0, "Secrets found in config: " + string.Join("; ", problems));
    }

    private static void Walk(JsonElement element, string path, string file, List<string> problems)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    Walk(property.Value, path.Length == 0 ? property.Name : path + ":" + property.Name, file, problems);
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    Walk(item, path, file, problems);
                break;

            case JsonValueKind.String:
                var value = element.GetString();
                var key = path.ToLowerInvariant();
                var looksSecret = SecretWords.Any(word => key.Contains(word, StringComparison.Ordinal));
                var hasPassword = key.StartsWith("connectionstrings", StringComparison.Ordinal)
                                  && (value?.Contains("password=", StringComparison.OrdinalIgnoreCase) ?? false);

                if ((looksSecret && !string.IsNullOrWhiteSpace(value)) || hasPassword)
                    problems.Add($"{file}: {path}");
                break;
        }
    }
}