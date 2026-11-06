using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace LearnAI.Kernel.Plugins
{
    public class TimePlugin
    {
        [KernelFunction]
        [Description("Retrieves the current date and time in the system's local time zone.")]
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("F");
        }

        [KernelFunction]
        [Description("Calculates a future date based on a given number of days from today.")]
        public string GetFutureDate(
            [Description("The number of days to add to the current date.")] int daysToAdd)
        {
            return DateTime.Now.AddDays(daysToAdd).ToString("yyyy-MM-dd");
        }
    }
}
