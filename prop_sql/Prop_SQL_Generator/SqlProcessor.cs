using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prop_SQL_Generator
{
    class SqlProcessor
    {

        public Connections ProcessSQl(string propSql)
        {
            return EatConnections(ref propSql);
        }

        private Connections EatConnections(ref string propSql)
        {
            var result = new Connections();
            if (EatToken("(", ref propSql))
            {
                result.Parentheses = true;
            }

            while (true)
            {
                Connector connector = EatConnector(ref propSql);
                result.Connectors.Add(connector);

                if (connector.Value is null) break;
            }

            EatToken(")", ref propSql);

            return result;
        }

        private Connector EatConnector(ref string propSql)
        {
            var result = new Connector();
            Token nextToken = PeekToken(propSql);

            if (nextToken == Token.Property)
            {
                result.LeftStatement = EatComparator(ref propSql);
            }
            else if (nextToken == Token.OpenParenthesis)
            {
                result.LeftStatement = EatConnections(ref propSql);
            }

            propSql = EatConnectorSymbol(propSql, result);

            return result;
        }

        private string EatConnectorSymbol(string propSql, Connector result)
        {
            string _;
            Token connectorToken = PeekToken(propSql);

            if (connectorToken == Token.And)
            {
                result.Value = ConnectorEnum.And;
                EatToken(Token.And, ref propSql, out _);
            }

            if (connectorToken == Token.Or)
            {
                result.Value = ConnectorEnum.Or;
                EatToken(Token.Or, ref propSql, out _);
            }

            return propSql;
        }

        private bool EatToken(Token token, ref string propSql, out string value)
        {
            string simple;
            if (tokenMap.TryGetValue(token, out simple))
            {
                value = string.Empty;
                return EatToken(simple, ref propSql);
            }

            if (token == Token.Value)
            {
                value = GetValue(propSql);
                return EatToken(value, ref propSql);
            }

            if (token == Token.Property)
            {
                value = GetProperty(propSql);
                return EatToken(value, ref propSql);
            }

            value = string.Empty;
            return false;
        }

        private Comparator EatComparator(ref string propSql)
        {
            var result = new Comparator();
            result.Left = EatTableProperty(ref propSql);
            result.Value = EatComparatorEnum(ref propSql);
            result.Right = EatConstant(ref propSql);
            return result;
        }

        private TableProperty EatTableProperty(ref string propSql)
        {
            TableProperty result = new TableProperty();
            string value;
            EatToken(Token.Property, ref propSql, out value);
            result.Value = value;
            return result;
        }

        private ComparatorEnum EatComparatorEnum(ref string propSql)
        {
            Token comparer = PeekToken(propSql);
            string _;
            EatToken(comparer, ref propSql, out _);
            switch (comparer)
            {
                case Token.Equals: return ComparatorEnum.Equals;
                case Token.LessThan: return ComparatorEnum.LessThan;
                case Token.GreaterThan: return ComparatorEnum.GreaterThan;
                case Token.GreaterThanOrEqual: return ComparatorEnum.GreaterThenOrEqual;
                case Token.LessThanOrEqual: return ComparatorEnum.LessThanOrEqual;
                case Token.NotEqual: return ComparatorEnum.NotEqual;
            }

            throw new NotSupportedException("unsupported comparer token");
        }

        private Constant EatConstant(ref string propSql)
        {
            Constant result = new Constant();
            string value;
            EatToken(Token.Value, ref propSql, out value);
            result.Value = value;
            return result;
        }

        private bool EatToken(string s, ref string propSql)
        {
            if (propSql.StartsWith(s))
            {
                propSql = propSql.Substring(s.Length).TrimStart();
                return true;
            }

            return false;
        }

        private Token PeekToken(string propSql)
        {

            if (propSql.StartsWith('('))
            {
                return Token.OpenParenthesis;
            } 
            else if (propSql.StartsWith(')'))
            {
                return Token.CloseParenthesis;
            }
            else if (propSql.StartsWith("AND"))
            {
                return Token.And;
            }
            else if (propSql.StartsWith("OR"))
            {
                return Token.Or;
            }
            else if (propSql.StartsWith(">="))
            {
                return Token.GreaterThanOrEqual;
            }
            else if (propSql.StartsWith("<="))
            {
                return Token.LessThanOrEqual;
            }
            else if (propSql.StartsWith("<>"))
            {
                return Token.NotEqual;
            }
            else if (propSql.StartsWith('='))
            {
                return Token.Equals;
            }
            else if (propSql.StartsWith('<'))
            {
                return Token.LessThan;
            }
            else if (propSql.StartsWith('>'))
            {
                return Token.GreaterThan;
            }
            else if (propSql.StartsWith('\''))
            {
                return Token.Value;
            }
            else if (propSql.StartsWith("property_v"))
            {
                return Token.Property;
            }
            else
            {
                throw new NotSupportedException($"Fell over with :{propSql}: remaining");
            }
        }

        private string GetProperty(string propSql)
        {
            StringBuilder builder = new StringBuilder();

            char[] chars = propSql.ToCharArray();
            int index = 0;
            char[] blacklist = { '=', '<', '>' };
            while (!blacklist.Contains(chars[index]))
            {
                builder.Append(chars[index]);

                index++;
            }

            return builder.ToString();
        }

        private string GetValue(string propSql)
        {
            StringBuilder builder = new StringBuilder();

            char[] chars = propSql.ToCharArray();
            int index = 0;
            int quoteCount = 0;
            while (quoteCount < 2)
            {
                if (chars[index] == '\'') quoteCount++;

                builder.Append(chars[index]);

                index++;
            }

            return builder.ToString();
        }

        public Dictionary<Token, string> tokenMap = new Dictionary<Token, string>()
        {
            { Token.And, "AND" },
            { Token.CloseParenthesis, ")" },
            { Token.Equals, "=" },
            { Token.GreaterThan, ">" },
            { Token.GreaterThanOrEqual, ">=" },
            { Token.LessThan, "<" },
            { Token.LessThanOrEqual, "<=" },
            { Token.NotEqual, "<>" },
            { Token.OpenParenthesis, "(" },
            { Token.Or, "OR" }
        };
    }

    enum Token
    {
        Property,
        Value,
        And,
        Or,
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        NotEqual,
        OpenParenthesis,
        CloseParenthesis
    }
}
