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

            Dictionary<int, Statement> methods = new Dictionary<int, Statement>();
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

                item.Statement?.LoadEquations(newModel.Parameters);
                csvModels.Add(newModel);
            }

            WriteCsv(csvModels);
            WriteCSharp(methods);
        }

        private static void WriteCSharp(Dictionary<int, Statement> methods)
        {
            new CSharpWriter(methods).WriteTo("../../../out.cs");
        }

        private static void WriteCsv(List<CSVOutModel> csvModels)
        {
            File.WriteAllLines("../../../out.csv", csvModels.Select(csvModels => csvModels.ToString()));
        }

        private static int GetOrAdd(Dictionary<int, Statement> methods, ref int index, Statement statement, string contractRef)
        {
            if (statement is null) return 0;

            int key = methods.Where(kv => kv.Value.StatementEquals(statement)).Select(kv => kv.Key).SingleOrDefault();

            if (key == 0)
            {
                methods.Add(index, statement);
                methods[index].Contractors.Add(contractRef);
                return index++;
            }
            else
            {
                methods[key].Contractors.Add(contractRef);
                return key;
            }
        }
    }

    class CSVModel
    {
        private readonly SqlProcessor _sql;

        public string ContractRef { get; set; }
        public string PropSql { get; set; }

        public Statement Statement { get; set; }

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
        public List<Equation> Parameters { get; set; } = new List<Equation>();

        public override string ToString()
        {
            return $"{ContractRef},{MatchResolutionId},{string.Join(',', Parameters.Select(p => p.TrimmedValue))}";
        }
    }
}
