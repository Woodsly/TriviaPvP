namespace TriviaPvPCoreAPI.Models
{
    public class ApiResponses
    {
        public class StartGameResponse
        {
            public int RoundNumber { get; set; }
            public string Message { get; set; }
        }

        public class StartResponse
        {
            public string Message { get; set; }
            public List<string> Options { get; set; }
        }

        public class AnswerRequest
        {
            public string PlayerName { get; set; }
            public string SelectedAnswer { get; set; }
        }

        public class RoundResult
        {
            public string Message { get; set; }
            public List<PlayerScore> Scores { get; set; }
            public bool IsGameOver { get; set; }
            public int RoundNumber { get; set; }
        }

        public class PlayerScore
        {
            public string PlayerName { get; set; }
            public int Score { get; set; }
        }

    }
}
