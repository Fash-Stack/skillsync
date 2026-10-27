using System.Xml.Linq;

namespace SkillSync.ArchitectureTests;

public class ProjectReferenceTests
{
    private static string[] ProjectFiles(string folder) =>
        Directory.GetFiles(Path.Combine(ModuleCatalog.RepositoryRoot(), folder), "*.csproj", SearchOption.AllDirectories);

    private static List<string> References(string csproj) =>
        XDocument.Load(csproj).Descendants("ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(((string)e.Attribute("Include")!).Replace('\\', '/')))
            .ToList();

    private static bool Has(string csproj, string element) =>
        XDocument.Load(csproj).Descendants(element).Any();

    [Fact]
    public void Modules_reference_only_shared_kernel_and_their_own_contracts()
    {
        var modules = ProjectFiles(Path.Combine("src", "Modules"))
            .Where(f => !f.EndsWith(".Contracts.csproj", StringComparison.Ordinal));

        foreach (var file in modules)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var illegal = References(file).Where(r => r != "SkillSync.SharedKernel" && r != name + ".Contracts").ToList();

            Assert.True(illegal.Count == 0, $"{name} references: {string.Join(", ", illegal)}");
        }
    }

    [Fact]
    public void Contracts_reference_only_shared_kernel_and_hold_no_packages()
    {
        var contracts = ProjectFiles(Path.Combine("src", "Modules"))
            .Where(f => f.EndsWith(".Contracts.csproj", StringComparison.Ordinal));

        foreach (var file in contracts)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            Assert.All(References(file), r => Assert.Equal("SkillSync.SharedKernel", r));
            Assert.False(Has(file, "PackageReference"), $"{name} must not use packages.");
            Assert.False(Has(file, "FrameworkReference"), $"{name} must not use a framework reference.");
        }
    }

    [Fact]
    public void Shared_kernel_references_nothing()
    {
        var file = Path.Combine(ModuleCatalog.RepositoryRoot(), "src", "SkillSync.SharedKernel", "SkillSync.SharedKernel.csproj");

        Assert.Empty(References(file));
        Assert.False(Has(file, "PackageReference"));
        Assert.False(Has(file, "FrameworkReference"));
    }
}