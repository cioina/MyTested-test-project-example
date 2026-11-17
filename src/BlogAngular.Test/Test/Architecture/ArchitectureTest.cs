using BlogAngular.Application;
using BlogAngular.Domain;
using BlogAngular.Infrastructure;
using NetArchTest.Rules;
using Xunit;

namespace BlogAngular.Test.Architecture
{
    public class ArchitectureTest
    {
        [Fact]
        public void DomainShouldNotHaveAnyDependencies()
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
        public void InfrastructureShouldDependOnDomain()
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
}