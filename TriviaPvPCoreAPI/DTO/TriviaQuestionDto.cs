namespace TriviaPvPCoreAPI.DTO
{
    public class TriviaQuestionDto
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
        public Guid SessionId { get; set; }
        public List<TriviaQuestionOption> Options { get; set; }
    }
}
