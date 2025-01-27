using TriviaPvP.Models;
using TriviaPvP.Services;

// API Key
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

// Initialize OpenAI service
var openAiService = new OpenAiService(openAiApiKey);

Console.WriteLine("Enter a topic for trivia (e.g., science, history):");
var topic = ValidationService.ValidateInput();

var prompt = $"Generate a multiple choice or true/false trivia question about {topic}. Provide the question and the correct answer.  " +
    $"Mark multiple choice A-D, and true false A or B.  " +
    $"Always try to stick to the formatting example: **Question:** What is 2+2?\r\n\r\nA: 1  \r\nB: 2  \r\nC: 3  \r\nD: 4  \r\n\r\n**Correct Answer:** D: 4";

var aiGeneratedQuestion = openAiService.GenerateTriviaQuestion(prompt);

List<Player> players = new List<Player>
        {
            new Player("Player 1"),
            new Player("Player 2")
        };

Game game = new Game(aiGeneratedQuestion, players);

game.Start();