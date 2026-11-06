using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Chat.Services
{
    public class TokenService
    {
        public int EstimateTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Length / 4;
        }
    }
}
