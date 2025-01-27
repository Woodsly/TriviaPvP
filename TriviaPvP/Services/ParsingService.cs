using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriviaPvP.Models;

namespace TriviaPvP.Services
{
    public static class ParsingService
    {
        public static TriviaQuestion ParseOpenAiToTriviaQuestion(string aiResponse)
        {
            TriviaQuestion triviaQuestion = new TriviaQuestion();

            // Split the response into lines
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (!lines.Any())
            {
                //error
                return triviaQuestion;
            }

            triviaQuestion.Question = lines.FirstOrDefault(line => line.StartsWith("**Question:**"))
                                          ?.Replace("**Question:**", "")
                                          .Trim();

            // Extract options
            foreach (var line in lines.Where(line => line.StartsWith("A:") || line.StartsWith("B:") || line.StartsWith("C:") || line.StartsWith("D:")))
            {
                triviaQuestion.Options.Add(line.Trim());
            }

            // Extract the correct answer
            var correctAnswerLine = lines.FirstOrDefault(line => line.StartsWith("**Correct Answer:**"));
            if (correctAnswerLine != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(correctAnswerLine, @"\*\*Correct Answer:\*\* ([A-D]):");
                if (match.Success)
                {
                    triviaQuestion.CorrectAnswer = match.Groups[1].Value; // Extract only the letter (e.g., "C")
                }
            }

            return triviaQuestion;
        }
    }
}
