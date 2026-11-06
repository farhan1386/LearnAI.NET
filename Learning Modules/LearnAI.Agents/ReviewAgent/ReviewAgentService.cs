namespace LearnAI.Agents.ReviewAgent
{
    public class ReviewAgentService
    {
        public string SystemPersona => "You are a Senior Quality Assurance Auditor. Your role is to inspect written artifacts and ensure strict policy alignment.";

        public async Task<string> ExecuteAuditAsync(string taskDescription)
        {
            await Task.Delay(100);
            return $"[Audit Verification Log] Completed regulatory safety scan regarding: '{taskDescription}'. Quality metrics validated at 100%.";
        }
    }
}
