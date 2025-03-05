using Npm.Renovator.Common.Extensions;

namespace Npm.Renovator.Domain.Models
{
    public sealed record PackageJsonDependencies : IEquatable<PackageJsonDependencies>
    {
        public Dictionary<string, string> DevDependencies { get; set; } = [];
        public Dictionary<string, string> Dependencies { get; set; } = [];
        
        public bool Equals(PackageJsonDependencies? other)
        {
            return other is not null &&
                   DevDependencies.IsStringSequenceEqual(other.DevDependencies) &&
                   Dependencies.IsStringSequenceEqual(other.Dependencies);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }
    }
}
