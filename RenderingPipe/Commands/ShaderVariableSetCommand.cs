using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderingPipe.Commands
{
    public class ShaderVariableSetCommand : IRenderCommand
    {
        public override string ToString()
        {
            return String.Format("ShaderVariable: {0} = {1}", Key, Value);
        }

        public RenderCommandType RenderCommandType
        {
            get { return RenderCommandType.ShaderVriable_Set; }
        }

        public String Key { get; set; }
        public Object Value { get; set; }

        public static ShaderVariableSetCommand Create(String key, Object value)
        {
            return new ShaderVariableSetCommand
            {
                Key = key,
                Value = value,
            };

        }
    }
}
