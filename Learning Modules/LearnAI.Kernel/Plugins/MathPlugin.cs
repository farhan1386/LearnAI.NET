using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace LearnAI.Kernel.Plugins
{
    public class MathPlugin
    {
        [KernelFunction]
        [Description("Adds two numbers together.")]
        public double Add(
            [Description("The first number.")] double value1,
            [Description("The second number.")] double value2)
        {
            return value1 + value2;
        }

        [KernelFunction]
        [Description("Subtracts the second number from the first number.")]
        public double Subtract(
            [Description("The starting base number.")] double value1,
            [Description("The value to subtract away.")] double value2)
        {
            return value1 - value2;
        }
    }
}
