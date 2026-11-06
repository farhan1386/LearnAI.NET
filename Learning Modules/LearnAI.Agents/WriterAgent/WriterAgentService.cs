namespace LearnAI.Agents.WriterAgent
{
    public class WriterAgentService
    {
        public string SystemPersona => "You are a professional Technical Writer. Your objective is to synthesize research intelligence into highly structured markdown documentation.";

        public async Task<string> ProcessDraftAsync(string taskDescription)
        {
            await Task.Delay(100);
            return $"# Executive Summary\nGenerated comprehensive markdown report tracking task payload constraints: '{taskDescription}'. Layout finalized.";
        }
    }
}