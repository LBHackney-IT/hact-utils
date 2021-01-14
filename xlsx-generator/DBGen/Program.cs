using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DBGen
{
    class Program
    {
        public static Dictionary<string, string> BuiltInTypes = new Dictionary<string, string>()
        {
            { "string", "string" },
            { "datetime", "DateTime" },
            { "date", "DateTime" },
            { "time", "DateTime" },
            { "double", "double" },
            { "Duration", "DateTimeOffset" },
            { "integer", "int" },
        };

        public static List<string> IgnoreTypes = new List<string>()
        {
            "Reference",
            "BasicAttachment",
            "Attachment:BasicAttachment"
        };

        static void Main(string[] args)
        {
            ExcelResult data = LoadExcel("\\res\\RaiseRepair.xlsx");

            List<Entity> types = data.BuildTypes();

            string codeFile = new CodeFileBuilder(types).BuildTypeFile();

            File.WriteAllText("out.cs", codeFile);
        }

        private static ExcelResult LoadExcel(string path)
        {
            var result = new ExcelResult();
            Application app = new Application();
            var directory = Directory.GetCurrentDirectory();
            Workbook wb = app.Workbooks.Open(directory + path);
            _Worksheet sheet = wb.Sheets[2];
            Range range = sheet.UsedRange;

            try
            {

                int rowCount = range.Rows.Count;

                for (int i = 1; i <= rowCount; i++)
                {
                    result.AddAttribute(
                        range.Cells[i, 1].Value2.ToString(),
                        range.Cells[i, 2].Value2.ToString(),
                        range.Cells[i, 3].Value2.ToString(),
                        range.Cells[i, 8].Value2.ToString()
                        );
                }

            } finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Marshal.ReleaseComObject(range);
                Marshal.ReleaseComObject(sheet);

                wb.Close();
                Marshal.ReleaseComObject(wb);
                app.Quit();
                Marshal.ReleaseComObject(app);
            }
            

            return result;
        }
    }

    internal class ExcelResult
    {
        public List<AttributeRow> Data = new List<AttributeRow>();

        public ExcelResult()
        {
        }

        public void AddAttribute(string entity, string property, string type, string cardinality)
        {
            Data.Add(new AttributeRow
            {
                Entity = entity,
                Property = property,
                Type = type,
                IsCollection = !string.IsNullOrEmpty(cardinality) && !cardinality.EndsWith("1")
            });
        }

        internal List<Entity> BuildTypes()
        {
            Dictionary<string, Entity> result = GetBuiltInTypes();

            foreach (var item in Data.Skip(1))
            {
                if (Program.IgnoreTypes.Contains(item.Entity) || Program.IgnoreTypes.Contains(item.Type))
                {
                    continue;
                }

                Entity e = CreateEntity(result, item.Entity);
                AddProperty(e, item);
            }

            RemoveInheritedProperties(result);

            return result.Values.ToList();
        }

        private void RemoveInheritedProperties(Dictionary<string, Entity> result)
        {
            foreach (var entity in result.Values)
            {
                foreach (var prop in entity.Properties.ToList())
                {
                    if (entity.Parent?.Properties.Any(p => p.Name == prop.Name) ?? false) entity.Properties.Remove(prop);
                }
            }
        }

        private Dictionary<string, Entity> GetBuiltInTypes()
        {
            Dictionary<string, Entity> result = new Dictionary<string, Entity>();


            foreach (var type in Program.BuiltInTypes)
            {
                var e = CreateEntity(result, type.Key);
                e.GenerateDefinition = false;
                e.ReferenceAlias = type.Value;
            }

            return result;
        }

        private void AddProperty(Entity e, AttributeRow item)
        {
            string typeAlias;
            bool isBuiltInType = Program.BuiltInTypes.TryGetValue(item.Type, out typeAlias);

            e.Properties.Add(new Property
            {
                Name = item.Property,
                IsList = item.IsCollection,
                TypeString = isBuiltInType ? typeAlias : item.Type,
                IsNavigation = Data.Any(d => d.Entity.Split(':')[0] == item.Type)
            });
        }

        private Entity CreateEntity(Dictionary<string, Entity> result, string entityName)
        {
            string[] split = entityName.Split(':');
            string typeName = split[0];
            Entity parent = null;

            if (split.Length == 2)
            {
                parent = CreateEntity(result, split[1]);
            }

            if (!result.ContainsKey(typeName))
            {
                result.Add(typeName, new Entity
                {
                    Name = typeName,
                    Properties = new List<Property>(),
                    Parent = parent
                });
            }

            return result[typeName];
        }
    }

    public class AttributeRow
    {
        public string Entity { get; internal set; }
        public string Property { get; internal set; }
        public string Type { get; internal set; }
        public bool IsCollection { get; internal set; }
    }

    public class Entity
    {
        public string Name { get; set; }
        public Entity Parent { get; internal set; }

        public List<Property> Properties { get; set; }

        public bool GenerateDefinition { get; set; } = true;
        public string ReferenceAlias { get; internal set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string TypeString { get; set; }
        public bool IsList { get; set; }
        public bool IsNavigation { get; set; }
    }
}