using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Cnct.Core.Configuration.Schema
{
    public class LinkTaskSpecificationSchemaGenerationProvider : BaseSchemaGenerationProvider<LinkTaskSpecification>
    {
        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            var schema = new JSchema();

            var linksSchema = new JSchema();
            linksSchema.OneOf.Add(new JSchema() { Type = JSchemaType.String });
            linksSchema.OneOf.Add(new JSchema() { Type = JSchemaType.Null });
            //linksSchema.OneOf.Add(new JSchema() { Type = JSchemaType. });

            schema.Properties.Add("links", linksSchema);

            return schema;
        }
    }
}
