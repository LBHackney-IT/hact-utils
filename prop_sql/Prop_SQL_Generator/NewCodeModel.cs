using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Prop_SQL_Generator
{
    public interface ISourceItem
    {
        void Visit(IVisitor visitor);
    }

    public interface IVisitor
    {
        void Accept(ISourceItem item);
    }

    public class Comparator : IConnectorItem, ISourceItem
    {
        public ComparatorEnum Value { get; set; }

        public TableProperty Left { get; set; }
        public Constant Right { get; set; }
        public Connector Parent { get; set; }

        public bool Equals(IConnectorItem other)
        {
            if (!(other is Comparator)) return false;
            var casted = other as Comparator;
            return (Value == casted.Value) && Left.Equals(casted.Left) && Right.Equals(casted.Right);
        }

        public string ComparatorString
        {
            get
            {
                switch (Value)
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

        public string ToCSharp()
        {
            if (Value == ComparatorEnum.Equals || Value == ComparatorEnum.NotEqual)
            {
                return $"{Left.ToCSharp()} {ComparatorString} {Right.ToCSharp()}";
            }

            return $"string.Compare({Left.ToCSharp()}, {Right.ToCSharp()}) {ComparatorString} 0";
        }

        public void Visit(IVisitor visitor)
        {
            visitor.Accept(this);
            Left.Visit(visitor);
            Right.Visit(visitor);
        }
    }

    public class Connector : ISourceItem
    {
        public ConnectorEnum? Value { get; set; }

        public IConnectorItem LeftStatement { get; set; }
        public Connections Parent { get; set; }
        public object ConnectorString { get; private set; }

        public bool Equals(Connector obj)
        {
            return Value == obj.Value && LeftStatement.Equals(obj.LeftStatement);
        }

        public void Visit(IVisitor visitor)
        {
            visitor.Accept(this);
            LeftStatement.Visit(visitor);
        }

        internal string ToCSharp()
        {
            return $"{LeftStatement.ToCSharp()} {GetConnectorString(Value)}";
        }

        string GetConnectorString(ConnectorEnum? enumValue)
        {
            if (!enumValue.HasValue) return string.Empty;

            switch (enumValue)
            {
                case ConnectorEnum.And: return "&&";
                case ConnectorEnum.Or: return "||";
            }

            throw new NotSupportedException("Unsupported Connector");
        }
    }

    public class Connections : IConnectorItem, ISourceItem
    {
        public bool Parentheses { get; set; }
        public List<Connector> Connectors { get; set; } = new List<Connector>();
        public Connector Parent { get; set; }

        public bool Equals(IConnectorItem other)
        {
            if (!(other is Connections)) return false;
            var casted = other as Connections;
            return (Parentheses == casted.Parentheses) && Enumerable.SequenceEqual(this.Connectors, casted.Connectors, new ConnectorComparer());
        }

        public void Visit(IVisitor visitor)
        {
            visitor.Accept(this);
            foreach (var item in Connectors)
            {
                item.Visit(visitor);
            }
        }

        public string ToCSharp()
        {
            string open = Parentheses ? "(" : "";
            string close = Parentheses ? ")" : "";
            return $"{open}{string.Join(" ", Connectors.Select(c => c.ToCSharp()))}{close}";
        }
    }

    public interface IConnectorItem : ISourceItem
    {
        bool Equals(IConnectorItem other);
        string ToCSharp();
    }

    public class TableProperty : ISourceItem
    {
        public string Value { get; set; }
        public Comparator Parent { get; set; }

        public bool Equals(TableProperty obj) => Value == obj.Value;
        public void Visit(IVisitor visitor) => visitor.Accept(this);



        public string CSharpProperty
        {
            get
            {
                var split = Value.Split('.');
                var prop = split[1];
                var words = prop.Split('_');
                return $"p.{string.Join("", words.Select(w => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(w)))}";
            }
        }

        public string ToCSharp()
        {
            return CSharpProperty;
        }
    }

    public class Constant : ISourceItem
    {
        public string Value { get; set; }
        public Comparator Parent { get; set; }
        public int Index { get; internal set; }

        public bool Equals(Constant obj) => true;

        public string TrimmedValue => Value.Trim('\'');
        public void Visit(IVisitor visitor) => visitor.Accept(this);

        internal string ToCSharp()
        {
            return $"values[{Index}]";
        }
    }
}
