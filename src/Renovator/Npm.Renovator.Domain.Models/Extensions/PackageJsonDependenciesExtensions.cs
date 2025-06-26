using Npm.Renovator.Common.Extensions;

namespace Npm.Renovator.Domain.Models.Extensions;

public static class PackageJsonDependenciesExtensions
{
    public static PackageJsonDependencies CloneObject(this PackageJsonDependencies packageJsonDependencies)
    {
        return new PackageJsonDependencies
        {
            DevDependencies = packageJsonDependencies.DevDependencies.Clone(),
            Dependencies = packageJsonDependencies.Dependencies.Clone()
        };
    }
}