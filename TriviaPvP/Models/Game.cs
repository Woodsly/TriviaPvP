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
        private TriviaQuestion _question;
        private List<Player> _players;
        public Game(TriviaQuestion question, List<Player> players)
        {
            _question = question;
            _players = players;
        }

        public void Start()
        {
            Console.WriteLine("Starting the trivia game!");

            foreach (Player player in _players)
            {
                Console.WriteLine($"{player.Name}, it's your turn!");
                Console.WriteLine($"Question: {_question.Question}");

                foreach (string choice in _question.Options)
                {
                    Console.WriteLine(choice);
                }

                string answer = ValidationService.ValidateGameInput("Your answer: ");

                if (answer.Equals(_question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Correct!");

                    player.AddScore(1);
                }
                else
                {
                    Console.WriteLine($"Wrong! The correct answer was: {_question.CorrectAnswer}");
                }
            }

            AnnounceWinner();
        }

        private void AnnounceWinner()
        {
            Player winner = _players.OrderByDescending(p => p.Score).First();

            Console.WriteLine($"The winner is {winner.Name} with {winner.Score} points!");
        }
    }
}
