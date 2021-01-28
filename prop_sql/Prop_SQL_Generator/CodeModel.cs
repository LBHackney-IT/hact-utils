using System.Collections.Generic;

namespace Prop_SQL_Generator
{
    class CodeModel
    {
    }

    class Statement
    {
        public Statement Left { get; set; }
         
        public ConnectorEnum? Connector { get; set; }
         
        public Statement Right { get; set; }
         
        public Statement Parent { get; set; }
         
        public Equation Equation { get; set; }

        public override string ToString()
        {
            return Equation?.ToString() ?? $"({Left} {Connector} {Right})";
        }

        public void LoadEquations(List<Equation> equations)
        {
            if (Equation != null)
            {
                equations.Add(Equation);
            }

            Left?.LoadEquations(equations);
            Right?.LoadEquations(equations);
        }
    }

    class Equation
    {
        public string PropertyProperty { get; set; }

        public ComparatorEnum? Comparator { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{PropertyProperty} {Comparator} {Value}";
        }
    }

    public enum ComparatorEnum
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThenOrEqual,
        LessThanOrEqual,
        NotEqual
    }

    public enum ConnectorEnum
    {
        And,
        Or
    }
}
