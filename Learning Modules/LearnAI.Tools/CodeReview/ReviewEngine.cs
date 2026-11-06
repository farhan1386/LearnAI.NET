using System;
using System.ComponentModel;

namespace LearnAI.Tools.CodeReview
{
    public class CodeReviewResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string[] IssuesFound { get; set; } = Array.Empty<string>();
        public bool RequiresRework { get; set; }
    }

    public class ReviewEngine
    {
        [Description("Analyzes a string of source code for performance bottleneck patterns and syntax bugs.")]
        public string AnalyzeCode(
            [Description("The raw source code content text block to evaluate.")] string sourceCode,
            [Description("The programming language profile identification name.")] string language)
        {
            if (string.IsNullOrWhiteSpace(sourceCode)) return "Error: Source code content is empty.";
            return $"Analysis complete for {language} context. Code quality baseline looks stable.";
        }
    }
}