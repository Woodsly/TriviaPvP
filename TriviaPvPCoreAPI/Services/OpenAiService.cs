﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;
using TriviaPvP.Models;

namespace TriviaPvP.Services
{
    public class OpenAiService
    {
        //https://github.com/openai/openai-dotnet

        private string _apiKey;
        public OpenAiService(string apiKey)
        {
            _apiKey = apiKey;
        }


        public TriviaQuestion GenerateTriviaQuestion(string prompt)
        {
            TriviaQuestion question = new TriviaQuestion();

            List<ChatMessage> messages = [new UserChatMessage(prompt)];

            ChatClient client = new(model: "gpt-4o-mini", apiKey: _apiKey);

            ChatCompletion completion = client.CompleteChat(messages, GetOptions());

            string aiResponse = completion.Content[0].Text;

            if (aiResponse != null)
            {
                question = JsonConvert.DeserializeObject<TriviaQuestion>(aiResponse);
            }

            return question;
        }

        public ChatCompletionOptions GetOptions()
        {
            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "trivia",
                    jsonSchema: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "Question": { "type": "string" },
                    "Options": { 
                        "type": "array",
                        "items": { "type": "string" }
                    },
                    "CorrectAnswer": { "type": "string" }
                },
                "required": ["Question", "Options", "CorrectAnswer"],
                "additionalProperties": false
            }
            """u8.ToArray()),
                    jsonSchemaIsStrict: true
                )
            };

            return options;
        }
    }
}
