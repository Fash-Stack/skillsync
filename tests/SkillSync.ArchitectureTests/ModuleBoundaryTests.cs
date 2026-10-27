namespace SkillSync.ArchitectureTests;

public class ModuleBoundaryTests
{
    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Only_the_module_class_is_public(string module)
    {
        var publicTypes = ModuleCatalog.Load(module).GetExportedTypes().Select(t => t.Name).ToList();

        Assert.Equal(new[] { $"{module}Module" }, publicTypes);
    }

    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Module_uses_only_shared_kernel_and_contracts(string module)
    {
        var illegal = ModuleCatalog.Load(module).GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("SkillSync.", StringComparison.Ordinal))
            .Where(n => n != "SkillSync.SharedKernel" && !n.EndsWith(".Contracts", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(illegal);
    }

    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Contracts_use_only_shared_kernel_and_no_web_or_database_code(string module)
    {
        var illegal = ModuleCatalog.LoadContracts(module).GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
                     || n.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                     || (n.StartsWith("SkillSync.", StringComparison.Ordinal) && n != "SkillSync.SharedKernel"))
            .ToList();

        Assert.Empty(illegal);
    }
}