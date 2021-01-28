using System;
using System.Linq;
using System.Text;

namespace Prop_SQL_Generator
{
    class SqlProcessor
    {
        public Statement ProcessSQl(string propSql)
        {
            Statement result = new Statement();

            string originalSql = (string)propSql.Clone();

            Statement current = result;

            Token token;
            string value;
            while (EatToken(ref propSql, out token, out value))
            {
                switch (token)
                {
                    case Token.OpenParenthesis:
                        if (current.Connector is null)
                        {
                            current.Left = new Statement();
                            current.Left.Parent = current;
                            current = current.Left;
                        }
                        else
                        {
                            current.Right = new Statement();
                            current.Right.Parent = current;
                            current = current.Right;
                        }
                        break;
                    case Token.CloseParenthesis:

                        if (current.Equation != null && current.Parent != null)
                        {
                            current = current.Parent;
                        }
                        
                        if (current.Parent != null)
                        {
                            current = current.Parent;
                        }
                        break;
                    case Token.And:
                        ShiftExistingEquation(current);
                        if (current.Connector is null)
                        {
                            current.Connector = ConnectorEnum.And;
                        }
                        else
                        {
                            var newStatement = new Statement();
                            newStatement.Parent = current;
                            newStatement.Left = current.Right;
                            newStatement.Connector = ConnectorEnum.And;
                            current.Right = newStatement;
                            current = newStatement;
                        }
                        break;
                    case Token.Or:
                        ShiftExistingEquation(current);
                        if (current.Connector is null)
                        {
                            current.Connector = ConnectorEnum.Or;
                        }
                        else
                        {
                            var newStatement = new Statement();
                            newStatement.Parent = current;
                            newStatement.Left = current.Right;
                            newStatement.Connector = ConnectorEnum.Or;
                            current.Right = newStatement;
                            current = newStatement;
                        }
                        break;
                    case Token.Property:
                        if (current.Connector != null)
                        {
                            current.Right = new Statement();
                            current.Right.Parent = current;
                            current = current.Right;
                        }
                        current.Equation = new Equation();
                        current.Equation.PropertyProperty = value;
                        break;
                    case Token.Value:
                        current.Equation.Value = value;
                        break;
                    case Token.Equals:
                        current.Equation.Comparator = ComparatorEnum.Equals;
                        break;
                    case Token.LessThan:
                        current.Equation.Comparator = ComparatorEnum.LessThan;
                        break;
                    case Token.GreaterThan:
                        current.Equation.Comparator = ComparatorEnum.GreaterThan;
                        break;
                    case Token.LessThanOrEqual:
                        current.Equation.Comparator = ComparatorEnum.LessThanOrEqual;
                        break;
                    case Token.GreaterThanOrEqual:
                        current.Equation.Comparator = ComparatorEnum.GreaterThenOrEqual;
                        break;
                    case Token.NotEqual:
                        current.Equation.Comparator = ComparatorEnum.NotEqual;
                        break;
                }
            }

            return result;
        }

        private void ShiftExistingEquation(Statement current)
        {
            if (current.Equation is null) return;

            current.Left = new Statement();
            current.Left.Parent = current;
            current.Left.Equation = current.Equation;
            current.Equation = null;
        }

        private bool EatToken(ref string propSql, out Token token, out string value)
        {
            string s;

            if (propSql.StartsWith('('))
            {
                token = Token.OpenParenthesis;
                value = null;
                s = "(";
            } 
            else if (propSql.StartsWith(')'))
            {
                token = Token.CloseParenthesis;
                value = null;
                s = ")";
            }
            else if (propSql.StartsWith("AND"))
            {
                token = Token.And;
                value = null;
                s = "AND";
            }
            else if (propSql.StartsWith("OR"))
            {
                token = Token.Or;
                value = null;
                s = "OR";
            }
            else if (propSql.StartsWith(">="))
            {
                token = Token.GreaterThanOrEqual;
                value = null;
                s = ">=";
            }
            else if (propSql.StartsWith("<="))
            {
                token = Token.LessThanOrEqual;
                value = null;
                s = "<=";
            }
            else if (propSql.StartsWith("<>"))
            {
                token = Token.NotEqual;
                value = null;
                s = "<>";
            }
            else if (propSql.StartsWith('='))
            {
                token = Token.Equals;
                value = null;
                s = "=";
            }
            else if (propSql.StartsWith('<'))
            {
                token = Token.LessThan;
                value = null;
                s = "<";
            }
            else if (propSql.StartsWith('>'))
            {
                token = Token.GreaterThan;
                value = null;
                s = ">";
            }
            else if (propSql.StartsWith('\''))
            {
                token = Token.Value;
                s = GetValue(propSql);
                value = s;
            }
            else if (propSql.StartsWith("property_v"))
            {
                token = Token.Property;
                s = GetProperty(propSql);
                value = s;
            }
            else if (propSql.Length == 0)
            {
                token = Token.Property;
                value = null;
                propSql = string.Empty;
                return false;
            }
            else
            {
                throw new NotSupportedException($"Fell over with :{propSql}: remaining");
            }

            propSql = propSql.Substring(s.Length).TrimStart();
            return true;
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
