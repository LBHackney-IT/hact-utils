using System;
using System.Collections.Generic;
using System.Text;

namespace DBGen
{
    internal class CodeFileBuilder
    {
        private List<Entity> types;
        private string Header =
@"
using System;
using System.Collections.Generic;

namespace DBGen
{

";

        private string Footer =
@"
}

";
        private const string Props = "{ get; set; }";

        public CodeFileBuilder(List<Entity> types)
        {
            this.types = types;
        }

        internal string BuildTypeFile()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Header);

            foreach (var entity in types)
            {
                builder.Append(CreateClassString(entity));
            }

            builder.Append(Footer);

            return builder.ToString();
        }

        private string CreateClassString(Entity entity)
        {
            if (!entity.GenerateDefinition) return string.Empty;

            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"public class {entity.Name}");
            builder.AppendLine("{");
            builder.AppendLine("[Key] public int Id { get; set; }");

            if (entity.Parent != null)
            {
                builder.AppendLine($"public virtual {entity.Parent.Name} {entity.Parent.Name} {Props}");
            }

            foreach (var property in entity.Properties)
            {
                if (property.IsList)
                {
                    builder.AppendLine($"public virtual List<{property.TypeString}> {property.Name} {Props}");
                }
                else
                {
                    string virtualMod = property.IsNavigation ? "virtual " : string.Empty;
                    builder.AppendLine($"public {virtualMod}{property.TypeString} {property.Name} {Props}");
                }
            }
            builder.AppendLine("}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}