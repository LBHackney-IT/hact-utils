namespace Prop_SQL_Generator
{
    static class StatementExtensions
    {
        public static bool StatementEquals(this Statement source, Statement other)
        {
            if (source is null && other is null) return true;
            if (source is null || other is null) return false;

            if (source.Equation is null && other.Equation is null)
            {
                return source.Left.StatementEquals(other.Left) && source.Right.StatementEquals(other.Right) && source.Connector == other.Connector;
            }

            if (source.Equation?.PropertyProperty == other.Equation?.PropertyProperty && source.Equation?.Comparator == other.Equation?.Comparator)
            {
                return true;
            }

            return false;
        }
    }
}
