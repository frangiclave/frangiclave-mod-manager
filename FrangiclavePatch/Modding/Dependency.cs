using System;

namespace Frangiclave.Modding
{
    public struct Dependency
    {
        public string ModId;
        public DependencyOperator VersionOperator;
        public Version Version;
    }

    public enum DependencyOperator
    {
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Equal
    }
}