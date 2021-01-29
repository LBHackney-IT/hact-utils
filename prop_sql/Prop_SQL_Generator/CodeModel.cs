using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Prop_SQL_Generator
{
    class CodeModel
    {
    }

    class Statement
    {
        public List<string> Contractors { get; set; } = new List<string>();


        public bool Parentheses { get; set; } = false;
        public Statement[] Statements { get; set; }
        public ConnectorEnum?[] Connectors { get; set; }
         
         
        public Equation Equation { get; set; }

        string GetConnector(ConnectorEnum enumValue)
        {
            switch (enumValue)
            {
                case ConnectorEnum.And: return "&&";
                case ConnectorEnum.Or: return "||";
            }

            throw new NotSupportedException("Unsupported Connector");
        }

        public override string ToString()
        {
            return Equation?.ToString() ?? $"({Left} {Connector} {Right})";
        }

        public string ToCSharp()
        {
            string open = Parentheses ? "(" : "";
            string close = Parentheses ? ")" : "";
            string connectorSection = Connector != null ? $"{ConnectorString} {Right?.ToCSharp()}" : "";
            return Equation?.ToCSharp() ?? $"{open}{Left?.ToCSharp()} {connectorSection}{close}"; 
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

        public string TrimmedValue => Value.Trim('\'');

        public override string ToString()
        {
            return $"{PropertyProperty} {Comparator} {TrimmedValue}";
        }

        public string ComparatorString
        {
            get
            {
                switch (Comparator)
                {
                    case ComparatorEnum.Equals: return "==";
                    case ComparatorEnum.GreaterThan: return ">";
                    case ComparatorEnum.GreaterThenOrEqual: return ">=";
                    case ComparatorEnum.LessThan: return "<";
                    case ComparatorEnum.LessThanOrEqual: return "<=";
                    case ComparatorEnum.NotEqual: return "!=";
                }

                throw new NotSupportedException("Unsupported Comparator");
            }
        }

        public string CSharpProperty
        {
            get
            {
                var split = PropertyProperty.Split('.');
                var prop = split[1];
                var words = prop.Split('_');
                return $"p.{string.Join("", words.Select(w => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(w)))}";
            }
        }

        public int Index { get; internal set; }

        internal string ToCSharp()
        {
            if (Comparator == ComparatorEnum.Equals || Comparator == ComparatorEnum.NotEqual)
            {
                return $"{CSharpProperty} {ComparatorString} values[{Index}]";
            }

            return $"string.Compare({CSharpProperty}, values[{Index}]) {ComparatorString} 0";
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
