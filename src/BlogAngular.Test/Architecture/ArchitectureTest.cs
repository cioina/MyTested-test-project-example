namespace BlogAngular.Test.Architecture;
#if DEBUG

using Application;
using Domain;
using Infrastructure;
using NetArchTest.Rules;
using Xunit;

public class ArchitectureTest
{
    [Fact]
    public void Domain_should_not_have_any_dependencies()
    {
        var application = typeof(ApplicationConfiguration).Assembly.GetName().Name;
        var infrastructure = typeof(InfrastructureConfiguration).Assembly.GetName().Name;

        var domainAssembly = typeof(DomainConfiguration).Assembly;

        var result = Types
          .InAssembly(domainAssembly)
          .ShouldNot()
          .HaveDependencyOnAny(
            application,
            infrastructure
          )
          .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_should_depend_on_domain()
    {
        var domain = typeof(DomainConfiguration).Assembly.GetName().Name;

        var infrastructureAssembly = typeof(InfrastructureConfiguration).Assembly;

        var result = Types
          .InAssembly(infrastructureAssembly)
          .That()
          .HaveNameEndingWith("Repository")
          .Should()
          .HaveDependencyOn(domain)
          .GetResult();

        Assert.True(result.IsSuccessful);
    }

}
#endif