using NetArchTest.Rules;

namespace SkillSync.ArchitectureTests;

public class LayerRulesTests
{
    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Domain_depends_on_no_other_layer_and_no_framework(string module)
    {
        var result = Types.InAssembly(ModuleCatalog.Load(module))
            .That().ResideInNamespace($"SkillSync.{module}.Domain")
            .ShouldNot().HaveDependencyOnAny(
                $"SkillSync.{module}.Application",
                $"SkillSync.{module}.Infrastructure",
                $"SkillSync.{module}.Endpoints",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Application_does_not_know_infrastructure_endpoints_or_frameworks(string module)
    {
        var result = Types.InAssembly(ModuleCatalog.Load(module))
            .That().ResideInNamespace($"SkillSync.{module}.Application")
            .ShouldNot().HaveDependencyOnAny(
                $"SkillSync.{module}.Infrastructure",
                $"SkillSync.{module}.Endpoints",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Endpoints_do_not_touch_infrastructure(string module)
    {
        var result = Types.InAssembly(ModuleCatalog.Load(module))
            .That().ResideInNamespace($"SkillSync.{module}.Endpoints")
            .ShouldNot().HaveDependencyOnAny(
                $"SkillSync.{module}.Infrastructure",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(ModuleCatalog.Modules), MemberType = typeof(ModuleCatalog))]
    public void Infrastructure_does_not_know_endpoints(string module)
    {
        var result = Types.InAssembly(ModuleCatalog.Load(module))
            .That().ResideInNamespace($"SkillSync.{module}.Infrastructure")
            .ShouldNot().HaveDependencyOn($"SkillSync.{module}.Endpoints")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        "Broken by: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());
}