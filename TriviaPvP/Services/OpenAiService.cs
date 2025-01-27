using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;
using TriviaPvP.Models;

namespace TriviaPvP.Services
{
    public class OpenAiService
    {
        private string _apiKey;
        public OpenAiService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public TriviaQuestion GenerateTriviaQuestion(string prompt)
        {
            TriviaQuestion question = new TriviaQuestion();
            try
            {
                ChatClient client = new(model: "gpt-4o-mini", apiKey: _apiKey);

                ChatCompletion completion = client.CompleteChat($"{prompt}");

                string aiResponse = completion.Content[0].Text;

                question = ParsingService.ParseOpenAiToTriviaQuestion(aiResponse);

                return question;
            }
            catch (Exception ex)
            {
                var i = ex;
                return question;
            }
        }
    }
}
