using TriviaPvP.Models;
using TriviaPvP.Services;

// API Key
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

// Initialize OpenAI service
var openAiService = new OpenAiService(openAiApiKey);

List<Player> players = new List<Player>
        {
            new Player("Player 1"),
            new Player("Player 2")
        };

Game game = new Game(openAiService, players);

game.Start(1);