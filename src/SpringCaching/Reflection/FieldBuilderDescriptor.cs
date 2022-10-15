using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Reflection
{
#if DEBUG
    public class FieldBuilderDescriptor
#else
    internal class FieldBuilderDescriptor
#endif
    {
        public FieldBuilderDescriptor(ParameterInfo parameter, FieldBuilder fieldBuilder)
        {
            Parameter = parameter;
            FieldBuilder = fieldBuilder;
        }
        public ParameterInfo Parameter { get; }
        public FieldBuilder FieldBuilder { get; }
    }
}
