using TriviaPvP.Models;
using TriviaPvP.Services;
using static TriviaPvPCoreAPI.Models.ApiResponses;

namespace TriviaPvPCoreAPI.Models
{
    public class Game
    {
        private OpenAiService _openAiService;
        private List<Player> _players;
        private TriviaQuestion _question;
        private int _roundNumber;

        public Game(OpenAiService openAiService)
        {
            _openAiService = openAiService;
            _players = new List<Player>();
            _roundNumber = 1;
        }

        public void AddPlayers(List<string> playerNames)
        {
            foreach (string name in playerNames)
            {
                _players.Add(new Player(name));
            }
        }

        public StartResponse StartGame()
        {
            return new StartResponse
            {
                Message = $"Starting round {_roundNumber}. Enter a topic to begin!",
                Options = new List<string>()
            };
        }

        public TriviaQuestion GetTriviaQuestion(string topic)
        {
            string prompt = $"Generate a multiple choice or true/false trivia question about {topic}. Provide the question and the correct answer.  " +
                         $"Mark multiple choice A-D, and true false A or B.  " +
                         $"Always try to stick to the formatting example: **Question:** What is 2+2?\r\n\r\nA: 1  \r\nB: 2  \r\nC: 3  \r\nD: 4  \r\n\r\n**Correct Answer:** D: 4.";

            _question = _openAiService.GenerateTriviaQuestion(prompt);
            return _question;
        }

        public RoundResult SubmitAnswer(string playerName, string selectedAnswer)
        {
            Player player = _players.FirstOrDefault(p => p.Name == playerName);
            if (player == null) throw new ArgumentException("Invalid player");

            string correctAnswer = _question.CorrectAnswer.Substring(0, 1);

            if (!player.Answered)
            {
                if (selectedAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    player.AddScore(1);
                }
            }

            player.Answered = true; // Mark player as having answered

            // Check if all players have answered
            if (_players.All(p => p.Answered))
            {
                // Increment the round after all players have answered
                _roundNumber++;

                foreach (var p in _players)
                {
                    p.Answered = false;
                }

                // Check if a player has won and the game should end
                Player winner = _players.FirstOrDefault(p => p.Score >= 3);
                if (winner != null)
                {
                    // Ensure there's no tie by checking if only one player has 3 or more points
                    List<Player> tiedPlayers = _players.Where(p => p.Score >= 3).ToList();
                    if (tiedPlayers.Count > 1)
                    {
                        return new RoundResult
                        {
                            Message = "It's a tie! No winner.",
                            Scores = _players.Select(p => new PlayerScore { PlayerName = p.Name, Score = p.Score }).ToList(),
                            IsGameOver = false // Game continues
                        };
                    }

                    return new RoundResult
                    {
                        Message = $"The winner is {winner.Name} with {winner.Score} points!",
                        Scores = _players.Select(p => new PlayerScore { PlayerName = p.Name, Score = p.Score }).ToList(),
                        IsGameOver = true // Indicating the game is over
                    };
                }
            }

            string message = $"Correct answer was: {_question.CorrectAnswer}";
            return new RoundResult
            {
                Message = message,
                Scores = _players.Select(p => new PlayerScore { PlayerName = p.Name, Score = p.Score }).ToList(),
                IsGameOver = false, // Game continues
                RoundNumber = _roundNumber // Include round number in the response
            };
        }
    }
}
