using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cnct.Core.Configuration;
using Cnct.Core.Configuration.Schema;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Cnct.Core.Configuration.Schema
{
    public class CnctTaskSchemaGenerationProvider : BaseSchemaGenerationProvider<ICnctActionSpec>
    {
        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            var types = typeof(ICnctActionSpec).Assembly.GetTypes().Where(t => Yes(t)).ToArray();



            throw new NotImplementedException();
        }

        private static bool Yes(Type t)
        {
            var it = typeof(ICnctActionSpec);

            return it != t && it.IsAssignableFrom(t) && !t.IsAbstract;
        }
    }
}
