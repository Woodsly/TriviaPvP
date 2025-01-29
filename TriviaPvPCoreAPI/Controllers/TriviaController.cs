using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriviaPvP.Services;
using TriviaPvPCoreAPI.Interfaces;
using TriviaPvPCoreAPI.Models;
using static TriviaPvPCoreAPI.Models.ApiResponses;

namespace TriviaPvPCoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriviaController : ControllerBase
    {
        private readonly IGameService _gameService;
        public TriviaController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("start")]
        public IActionResult StartGame([FromBody] List<string> playerNames)
        {
            var (sessionId, startResponse) = _gameService.StartGame(playerNames);
            return Ok(new { SessionId = sessionId, StartResponse = startResponse });
        }

        [HttpGet("question")]
        public IActionResult GetQuestion([FromQuery] string topic, [FromQuery] string sessionId)
        {
            var question = _gameService.GetQuestion(topic, sessionId);
            if (question.Question.Contains("Game session not found"))
                return NotFound(question);

            return Ok(question);
        }

        [HttpPost("answer")]
        public IActionResult SubmitAnswer([FromBody] AnswerRequest answerRequest, [FromQuery] string sessionId)
        {
            var result = _gameService.SubmitAnswer(sessionId, answerRequest);
            if (result.Message == "Game not found.")
                return NotFound(result.Message);

            return Ok(result);
        }
    }
}