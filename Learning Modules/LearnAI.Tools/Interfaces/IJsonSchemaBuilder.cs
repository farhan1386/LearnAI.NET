using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LearnAI.Tools.Interfaces
{
    public interface IJsonSchemaBuilder
    {
        string BuildSchema(MethodInfo methodInfo);
    }
}
