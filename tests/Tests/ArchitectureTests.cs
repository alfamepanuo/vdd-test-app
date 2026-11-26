using System.Reflection;

namespace Tests;

public class ArchitectureTests
{
    private static string[] ProjectRefs(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n is "Domain" or "Application" or "Infrastructure" or "Web")
            .Order()
            .ToArray();

    [Fact]
    public void Domain_has_no_project_references() =>
        Assert.Empty(ProjectRefs(typeof(Domain.AggregateRoot).Assembly));

    [Fact]
    public void Application_references_only_domain() =>
        Assert.Equal(["Domain"], ProjectRefs(typeof(Application.IUnitOfWork).Assembly));

    [Fact]
    public void Infrastructure_does_not_reference_web() =>
        Assert.DoesNotContain("Web", ProjectRefs(typeof(Infrastructure.AppDbContext).Assembly));

    [Fact]
    public void Web_does_not_reference_domain_internals_wrongly_and_is_not_referenced_by_inner_layers()
    {
        Assert.DoesNotContain("Web", ProjectRefs(typeof(Application.IUnitOfWork).Assembly));
        Assert.DoesNotContain("Web", ProjectRefs(typeof(Domain.AggregateRoot).Assembly));
    }
}
