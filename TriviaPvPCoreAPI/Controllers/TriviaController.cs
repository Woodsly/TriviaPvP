using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriviaPvP.Services;
using TriviaPvPCoreAPI.Models;
using static TriviaPvPCoreAPI.Models.ApiResponses;

namespace TriviaPvPCoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriviaController : ControllerBase
    {
        private static readonly Dictionary<string, Game> _games = new Dictionary<string, Game>();
        private readonly OpenAiService _openAiService;

        public TriviaController(OpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        [HttpPost("start")]
        public IActionResult StartGame([FromBody] List<string> playerNames)
        {
            // Generate a unique session ID (could be playerNames or a GUID)
            var sessionId = Guid.NewGuid().ToString();

            Game game = new Game(_openAiService);
            game.AddPlayers(playerNames);
            _games[sessionId] = game;

            StartResponse response = game.StartGame();
            return Ok(new { SessionId = sessionId, response });
        }

        [HttpGet("question")]
        public IActionResult GetQuestion([FromQuery] string topic, [FromQuery] string sessionId)
        {
            if (!_games.ContainsKey(sessionId))
                return NotFound("Game not found.");

            var game = _games[sessionId];
            var question = game.GetTriviaQuestion(topic);
            return Ok(question);
        }

        [HttpPost("answer")]
        public IActionResult SubmitAnswer([FromBody] AnswerRequest answerRequest, [FromQuery] string sessionId)
        {
            if (!_games.ContainsKey(sessionId))
                return NotFound("Game not found.");

            var game = _games[sessionId];
            //var question = game.GetTriviaQuestion(""); // You can store the topic if needed
            var result = game.SubmitAnswer(answerRequest.PlayerName, answerRequest.SelectedAnswer);

            if (result.IsGameOver)
            {
                _games.Remove(sessionId);
            }

            return Ok(result);
        }
    }
}