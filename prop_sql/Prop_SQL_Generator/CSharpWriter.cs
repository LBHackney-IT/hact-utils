using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prop_SQL_Generator
{
    internal class CSharpWriter
    {
        private Dictionary<int, Statement> methods;

        public CSharpWriter(Dictionary<int, Statement> methods)
        {
            this.methods = methods;
        }

        internal void WriteTo(string fileName)
        {
            var methodDeclarations = methods.Where(m => m.Value.Contractors.Contains("001-AJS-VFD2")).Select(m =>
            {
                var equations = new List<Equation>();
                m.Value.LoadEquations(equations);
                int i = 0;
                foreach (var equation in equations)
                {
                    equation.Index = i;
                    i++;
                }

                return $"// {string.Join(',', m.Value.Contractors)}\nprivate static bool Method{m.Key}(IProperty p, string[] values) => {m.Value.ToCSharp()};";
            });
            var cases = methods.Select(m => $"case {m.Key}: return Method{m.Key}(p, values);");

            var result = Template.Replace("{cases}", string.Join('\n', cases))
                                 .Replace("{methods}", string.Join('\n', methodDeclarations));

            File.WriteAllText(fileName, result);
        }

        private string Template =>
@"
using System;

namespace RepairsApi
{
    class PropertyContracts
    {
        public static bool CheckPropertyConstraint(int i, IProperty p, params string[] values)
        {
            if (i == 0) return true;

            switch (i)
            {
                {cases}
                default:
                    throw new NotImplementedException(""no matcher implemented for id "" + i);
            }
    }

    {methods}
    }
}

";
    }
}