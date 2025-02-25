using Microsoft.EntityFrameworkCore;
using TriviaPvP.Services;
using TriviaPvPCoreAPI.DTO;
using TriviaPvPCoreAPI.Interfaces;
using TriviaPvPCoreAPI.Models;
using TriviaPvPCoreAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TriviaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TriviaDb"), sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddSingleton<OpenAiService>(provider =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("The environment variable 'OPENAI_API_KEY' is not set.");
    }
    return new OpenAiService(apiKey);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
