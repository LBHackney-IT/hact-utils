using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prop_SQL_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("data.csv");

            var csvData = lines.Select(l => new CSVModel().With(l)).ToList();

            foreach (var item in csvData)
            {
                item.LoadStatement();
            }

            Dictionary<int, CSharpOutModel> methods = new Dictionary<int, CSharpOutModel>();
            List<CSVOutModel> csvModels = new List<CSVOutModel>();
            int index = 1;
            foreach (var item in csvData)
            {
                int key = GetOrAdd(methods, ref index, item.Statement, item.ContractRef);

                CSVOutModel newModel = new CSVOutModel
                {
                    ContractRef = item.ContractRef,
                    MatchResolutionId = key,
                };

                ParameterLoader visitor = new ParameterLoader();
                item.Statement?.Visit(visitor);
                newModel.Parameters = visitor.GetParameters();

                csvModels.Add(newModel);
            }

            WriteCsv(csvModels);
            WriteCSharp(methods);
        }

        private static void WriteCSharp(Dictionary<int, CSharpOutModel> methods)
        {
            new CSharpWriter(methods).WriteTo("../../../out.cs");
        }

        private static void WriteCsv(List<CSVOutModel> csvModels)
        {
            File.WriteAllLines("../../../out.csv", csvModels.Select(csvModels => csvModels.ToString()));
        }

        private static int GetOrAdd(Dictionary<int, CSharpOutModel> methods, ref int index, Connections statement, string contractRef)
        {
            if (statement is null) return 0;

            int key = methods.Where(kv => kv.Value.Code.Equals(statement)).Select(kv => kv.Key).SingleOrDefault();

            if (key == 0)
            {
                methods.Add(index, new CSharpOutModel { Code = statement});
                methods[index].ContractReferences.Add(contractRef);
                return index++;
            }
            else
            {
                methods[key].ContractReferences.Add(contractRef);
                return key;
            }
        }
    }

    internal class ParameterLoader : IVisitor
    {
        private List<string> parameters = new List<string>();
        private int index = 0;

        public void Accept(ISourceItem item)
        {
            RunFor<Constant>(item, constant =>
            {
                parameters.Add(constant.Value);
                constant.Index = index++;
            });
        }

        private void RunFor<T>(ISourceItem item, Action<T> action)
            where T : class
        {
            if (item is T)
            {
                T casted = item as T;
                action(casted);
            }
        }

        internal List<string> GetParameters()
        {
            return parameters;
        }
    }

    class CSVModel
    {
        private readonly SqlProcessor _sql;

        public string ContractRef { get; set; }
        public string PropSql { get; set; }

        public Connections Statement { get; set; }

        public CSVModel()
        {
            _sql = new SqlProcessor();
        }

        public CSVModel With(string csvLine)
        {
            var split = csvLine.Split(',').Select(l => l.Trim());

            ContractRef = split.First();
            PropSql = split.Last();

            return this;
        }

        public void LoadStatement()
        {
            if (string.IsNullOrWhiteSpace(PropSql) || PropSql == "\"\"") return;
            Statement = _sql.ProcessSQl(PropSql);
        }
    }

    class CSVOutModel
    {
        public string ContractRef { get; set; }
        public int MatchResolutionId { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{ContractRef},{MatchResolutionId},{string.Join(';', Parameters.Select(p => p.Trim('\'')))}";
        }
    }

    class CSharpOutModel
    {
        public Connections Code { get; set; }
        public List<string> ContractReferences { get; set; } = new List<string>();
    }
}
