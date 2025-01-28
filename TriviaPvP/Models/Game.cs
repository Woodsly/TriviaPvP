using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriviaPvP.Services;

namespace TriviaPvP.Models
{
    public class Game
    {
        private OpenAiService _openAiService;
        private List<Player> _players;
        public Game(OpenAiService openAiService, List<Player> players)
        {
            _openAiService = openAiService;
            _players = players;
        }

        public void Start(int roundNumber)
        {
            Console.WriteLine($"Starting round {roundNumber}...");
            Console.WriteLine("Decide this round's trivia topic!");
            Console.WriteLine("");

            TriviaQuestion question = GetTriviaQuestion();

            foreach (Player player in _players)
            {
                Console.WriteLine($"{player.Name}, it's your turn!");
                Console.WriteLine($"Question: {question.Question}");

                foreach (string choice in question.Options)
                {
                    Console.WriteLine(choice);
                }

                string answer = ValidationService.ValidateGameInput("Your answer: ");

                string first = question.CorrectAnswer.Substring(0, 1);

                if (answer.Equals(first, StringComparison.OrdinalIgnoreCase))
                {
                    player.AddScore(1);
                }
            }
            Console.WriteLine("");
            Console.WriteLine($"The correct answer was: {question.CorrectAnswer}");
            Console.WriteLine("");

            AnnounceWinner(roundNumber);
        }

        private void AnnounceWinner(int roundNumber)
        {
            List<Player> winner = _players.OrderByDescending(p => p.Score).Where(p => p.Score >= 3).ToList();

            if (winner.Count == 1)
            {
                Console.WriteLine($"The winner is {winner[0].Name} with {winner[0].Score} points!");
            }
            else
            {
                //if we don't have a winner incrememnt round and keep going
                Start(roundNumber + 1);
            }
        }

        private TriviaQuestion GetTriviaQuestion()
        {
            string topic = ValidationService.ValidateInput("Enter a topic for trivia (e.g., science, history): ");

            var prompt = $"Generate a multiple choice or true/false trivia question about {topic}. Provide the question and the correct answer.  " +
                $"Mark multiple choice A-D, and true false A or B.  " +
                $"Always try to stick to the formatting example: **Question:** What is 2+2?\r\n\r\nA: 1  \r\nB: 2  \r\nC: 3  \r\nD: 4  \r\n\r\n**Correct Answer:** D: 4.";

            TriviaQuestion aiGeneratedQuestion = _openAiService.GenerateTriviaQuestion(prompt);

            return aiGeneratedQuestion;
        }
    }
}
