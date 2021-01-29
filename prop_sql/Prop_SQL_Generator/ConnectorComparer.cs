using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Prop_SQL_Generator
{
    internal class ConnectorComparer : IEqualityComparer<Connector>
    {
        public bool Equals([AllowNull] Connector x, [AllowNull] Connector y) => x.Equals(y);
        public int GetHashCode([DisallowNull] Connector obj) => obj.GetHashCode();
    }
}