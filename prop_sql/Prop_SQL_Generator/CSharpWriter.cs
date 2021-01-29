using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prop_SQL_Generator
{
    internal class CSharpWriter
    {
        private Dictionary<int, CSharpOutModel> methods;

        public CSharpWriter(Dictionary<int, CSharpOutModel> methods)
        {
            this.methods = methods;
        }

        internal void WriteTo(string fileName)
        {
            var methodDeclarations = methods.Select(m =>
            {
                ParameterLoader visitor = new ParameterLoader();
                m.Value.Code.Visit(visitor);

                return $"// {string.Join(',', m.Value.ContractReferences)}\nprivate static bool Method{m.Key}(IProperty p, string[] values) => {m.Value.Code.ToCSharp()};";
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