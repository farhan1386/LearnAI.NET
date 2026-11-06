using LearnAI.Gateway.Enums;
using System;

namespace LearnAI.Gateway.Models
{
    public class ProviderMetric
    {
        public ProviderType Provider { get; set; }
        public string? ModelUsed { get; set; }
        public bool IsSuccess { get; set; }
        public TimeSpan Latency { get; set; }
        public int TokenCount { get; set; }
        public string? ErrorMessage { get; set; }
    }
}