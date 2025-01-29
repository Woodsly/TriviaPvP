using Microsoft.EntityFrameworkCore;
using TriviaPvP.Models;
using TriviaPvP.Services;
using TriviaPvPCoreAPI.DTO;
using TriviaPvPCoreAPI.Interfaces;
using TriviaPvPCoreAPI.Models;
using static TriviaPvPCoreAPI.Models.ApiResponses;

namespace TriviaPvPCoreAPI.Services
{
    public class GameService : IGameService
    {
        private readonly TriviaContext _context;
        private readonly OpenAiService _openAiService;

        public GameService(TriviaContext context, OpenAiService openAiService)
        {
            _context = context;
            _openAiService = openAiService;
        }

        public (string SessionId, StartResponse StartResponse) StartGame(List<string> playerNames)
        {
            var sessionId = Guid.NewGuid();

            var gameSession = new GameSession
            {
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameSessions.Add(gameSession);

            // Save the game session first to ensure SessionId is valid
            _context.SaveChanges();

            var players = new List<DTO.Player>();

            foreach (var name in playerNames)
            {
                var player = new DTO.Player
                {
                    PlayerName = name,
                    Score = 0,
                    Answered = false
                };

                _context.Players.Add(player);
                players.Add(player);
            }

            // Save players first so they get PlayerIds
            _context.SaveChanges();

            foreach (var player in players)
            {
                _context.Set<Dictionary<string, object>>("PlayerGameSession").Add(new Dictionary<string, object>
                {
                    ["PlayerId"] = player.PlayerId, // Now PlayerId exists
                    ["SessionId"] = sessionId
                });
            }

            // Save the many-to-many relationship
            _context.SaveChanges();

            var response = new StartResponse
            {
                Message = $"Game session {sessionId} started. Enter a topic to begin!",
                Options = new List<string>()
            };

            return (sessionId.ToString(), response);
        }


        public TriviaQuestionDto GetQuestion(string topic, string sessionId)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
                return new TriviaQuestionDto { Question = "Invalid session ID." };

            var session = _context.GameSessions.Find(sessionGuid);
            if (session == null)
                return new TriviaQuestionDto { Question = "Game session not found" };

            string prompt = $"Generate a multiple choice or true/false trivia question about {topic}. Provide the question and the correct answer.  " +
                            $"Mark multiple choice A-D, and true false A or B.  " +
                            $"Always try to stick to the formatting example: **Question:** What is 2+2?\r\n\r\nA: 1  \r\nB: 2  \r\nC: 3  \r\nD: 4  \r\n\r\n**Correct Answer:** D: 4.";

            var question = _openAiService.GenerateTriviaQuestion(prompt);

            var questionEntity = new DTO.TriviaQuestion
            {
                Question = question.QuestionContents,
                CorrectAnswer = question.CorrectAnswer,
                SessionId = sessionGuid
            };

            _context.TriviaQuestions.Add(questionEntity);

            // Save the options for this trivia question
            foreach (var optionText in question.Options) // Assuming 'Options' is a list of options returned by OpenAI
            {
                var optionEntity = new DTO.TriviaQuestionOption
                {
                    OptionText = optionText,
                    Question = questionEntity // Establish the relationship with the trivia question
                };

                _context.TriviaQuestionOptions.Add(optionEntity);
            }

            List<DTO.Player> playerList = _context.Players.Where(p => p.Sessions.Any(s => s.SessionId == sessionGuid)).ToList();

            foreach (DTO.Player player in playerList)
            {
                player.Answered = false;
            }

            _context.SaveChanges();

            // Now load the question with the options to return in the DTO
            var triviaQuestionWithOptions = _context.TriviaQuestions
                .Where(q => q.QuestionId == questionEntity.QuestionId)
                .Include(q => q.TriviaQuestionOptions)
                .FirstOrDefault();

            if (triviaQuestionWithOptions == null)
                return new TriviaQuestionDto { Question = "Error loading question with options." };

            // Map the entity to DTO
            var triviaQuestionDto = new TriviaQuestionDto
            {
                QuestionId = triviaQuestionWithOptions.QuestionId,
                Question = triviaQuestionWithOptions.Question,
                CorrectAnswer = triviaQuestionWithOptions.CorrectAnswer,
                SessionId = triviaQuestionWithOptions.SessionId,
                Options = triviaQuestionWithOptions.TriviaQuestionOptions
                    .Select(o => new TriviaQuestionOption
                    {
                        OptionText = o.OptionText,
                        OptionId = o.OptionId,
                        QuestionId = o.QuestionId
                    }).ToList()
            };

            return triviaQuestionDto;
        }


        public RoundResult SubmitAnswer(string sessionId, AnswerRequest answerRequest)
        {
            if (!Guid.TryParse(sessionId, out var sessionGuid))
                return new RoundResult { IsGameOver = false, Message = "Invalid session ID." };

            var session = _context.GameSessions.Find(sessionGuid);
            if (session == null)
                return new RoundResult { IsGameOver = false, Message = "Game session not found." };

            var player = _context.Players.FirstOrDefault(p => p.PlayerName == answerRequest.PlayerName && p.Sessions.Any(s => s.SessionId == sessionGuid));
            if (player == null)
                return new RoundResult { IsGameOver = false, Message = "Player not found in this session." };

            var question = _context.TriviaQuestions
                .Where(q => q.SessionId == sessionGuid)
                .OrderByDescending(q => q.QuestionId)
                .FirstOrDefault();

            if (question == null)
                return new RoundResult { IsGameOver = false, Message = "No active question found." };

            string correctAnswer = question.CorrectAnswer.Substring(0, 1);
            if (!(bool)player.Answered && answerRequest.SelectedAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
            {
                player.Score += 1;
            }

            player.Answered = true;
            _context.SaveChanges();

            // Now filter players by the current session
            var playersInSession = _context.Players.Where(p => p.Sessions.Any(s => s.SessionId == sessionGuid)).ToList();

            var allPlayersAnswered = playersInSession.All(p => (bool)p.Answered);

            if (allPlayersAnswered)
            {
                var winner = playersInSession.FirstOrDefault(p => p.Score >= 3);
                if (winner != null)
                {
                    EndGame(sessionGuid);
                    return new RoundResult
                    {
                        Message = $"The winner is {winner.PlayerName} with {winner.Score} points!",
                        Scores = playersInSession.Select(p => new PlayerScore
                        {
                            PlayerName = p.PlayerName,
                            Score = (int)p.Score
                        }).ToList(),
                        IsGameOver = true
                    };
                }
            }

            return new RoundResult
            {
                Message = $"Correct answer was: {question.CorrectAnswer}",
                Scores = playersInSession.Select(p => new PlayerScore
                {
                    PlayerName = p.PlayerName,
                    Score = (int)p.Score
                }).ToList(),
                IsGameOver = false
            };
        }

        public void EndGame(Guid guid)
        {
            List<Player> players = _context.Players.Where(p => p.Sessions.Any(s => s.SessionId == guid)).ToList();
            _context.TriviaQuestionOptions.RemoveRange(_context.TriviaQuestionOptions.Where(o => o.Question.SessionId == guid));
            _context.TriviaQuestions.RemoveRange(_context.TriviaQuestions.Where(q => q.SessionId == guid));
            _context.Set<Dictionary<string, object>>("PlayerGameSession").RemoveRange(_context.Set<Dictionary<string, object>>("PlayerGameSession").Where(pgs => (Guid)pgs["SessionId"] == guid));
            _context.GameSessions.Remove(_context.GameSessions.Find(guid));
            players.ForEach(p => p.Answered = false);
            _context.SaveChanges();
        }

    }

}
