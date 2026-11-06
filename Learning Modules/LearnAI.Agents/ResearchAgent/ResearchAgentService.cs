namespace LearnAI.Agents.ResearchAgent
{
    public class ResearchAgentService
    {
        public string SystemPersona => "You are an expert Retrieval and Research Assistant. Your purpose is to scan, filter, and extract core data points from raw materials.";

        public async Task<string> ProcessResearchAsync(string taskDescription)
        {
            await Task.Delay(100);
            return $"[Research Profile Output] Extracted 3 reference vectors matching target context: '{taskDescription}'. Documents aggregated successfully.";
        }
    }
}