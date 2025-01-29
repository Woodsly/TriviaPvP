using TriviaPvP.Models;
using static TriviaPvPCoreAPI.Models.ApiResponses;

namespace TriviaPvPCoreAPI.Interfaces
{
    public interface IGameService
    {
        (string SessionId, StartResponse StartResponse) StartGame(List<string> playerNames);
        TriviaQuestion GetQuestion(string topic, string sessionId);
        RoundResult SubmitAnswer(string sessionId, AnswerRequest answerRequest);
    }
}
