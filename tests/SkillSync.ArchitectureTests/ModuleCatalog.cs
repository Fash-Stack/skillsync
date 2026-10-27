using System.Reflection;

namespace SkillSync.ArchitectureTests;

public static class ModuleCatalog
{
    public static readonly string[] Names =
        ["Identity", "Catalogue", "Learning", "Payments", "Affiliates", "Community", "AiAssistant", "Admin"];

    public static TheoryData<string> Modules
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var name in Names) data.Add(name);
            return data;
        }
    }

    public static Assembly Load(string module) => Assembly.Load($"SkillSync.{module}");
    public static Assembly LoadContracts(string module) => Assembly.Load($"SkillSync.{module}.Contracts");

    public static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "global.json")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException("global.json not found above the test folder.");
    }
}
