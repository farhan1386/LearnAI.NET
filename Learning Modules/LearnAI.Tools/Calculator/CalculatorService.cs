using System.ComponentModel;

namespace LearnAI.Tools.Calculator
{
    public class CalculatorService
    {
        [Description("Adds two integer numbers together and returns the sum.")]
        public string AddNumbers(
            [Description("The first integer addend.")] int a,
            [Description("The second integer addend.")] int b)
        {
            return (a + b).ToString();
        }

        [Description("Multiplies two floating-point numbers together and returns the product.")]
        public string MultiplyNumbers(
            [Description("The multiplicand value.")] double a,
            [Description("The multiplier value.")] double b)
        {
            return (a * b).ToString();
        }
    }
}