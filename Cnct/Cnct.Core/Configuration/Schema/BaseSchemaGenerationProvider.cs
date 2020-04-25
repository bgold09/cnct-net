using Newtonsoft.Json.Schema.Generation;

namespace Cnct.Core.Configuration.Schema
{
    public abstract class BaseSchemaGenerationProvider<T> : JSchemaGenerationProvider
    {
        public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
        {
            return context.ObjectType == typeof(T);
        }
    }
}
