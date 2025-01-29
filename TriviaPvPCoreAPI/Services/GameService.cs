using TriviaPvP.Models;
using TriviaPvP.Services;
using TriviaPvPCoreAPI.Interfaces;
using TriviaPvPCoreAPI.Models;
using static TriviaPvPCoreAPI.Models.ApiResponses;

namespace TriviaPvPCoreAPI.Services
{
    public class GameService : IGameService
    {
        private static readonly Dictionary<string, Game> _games = new Dictionary<string, Game>();
        private readonly OpenAiService _openAiService;

        public GameService(OpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        public (string SessionId, StartResponse StartResponse) StartGame(List<string> playerNames)
        {
            var sessionId = Guid.NewGuid().ToString();

            Game game = new Game(_openAiService);
            game.AddPlayers(playerNames);

            _games[sessionId] = game;

            StartResponse response = game.StartRound();

            return (sessionId, response);
        }

        public TriviaQuestion GetQuestion(string topic, string sessionId)
        {
            if (!_games.ContainsKey(sessionId))
                return new TriviaQuestion { Question = "Game session not found" };

            Game game = _games[sessionId];

            TriviaQuestion question = game.GetTriviaQuestion(topic);

            return question;
        }

        public RoundResult SubmitAnswer(string sessionId, AnswerRequest answerRequest)
        {
            if (!_games.ContainsKey(sessionId))
                return new RoundResult { IsGameOver = false, Message = "Game not found." };

            Game game = _games[sessionId];

            RoundResult result = game.SubmitAnswer(answerRequest.PlayerName, answerRequest.SelectedAnswer);

            if (result.IsGameOver)
            {
                _games.Remove(sessionId);
            }

            return result;
        }
    }
}
