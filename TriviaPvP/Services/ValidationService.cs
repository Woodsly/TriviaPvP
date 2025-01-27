using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TriviaPvP.Services
{
    public static class ValidationService
    {
        public static string ValidateGameInput(string prompt)
        {

            bool valid = false;
            string answer = "";

            while (!valid)
            {
                Console.Write(prompt);

                answer = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(answer))
                {
                    Console.WriteLine("try again!");
                    continue;
                }
                else if (!Settings.AnswerOptions.Any(a => a.Equals(answer.ToUpper())))
                {
                    Console.WriteLine("Enter the letter.");
                    continue;
                }

                valid = true;
            }

            return answer;
        }

        public static string ValidateInput()
        {

            bool valid = false;
            string answer = "";

            while (!valid)
            {
                answer = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(answer))
                {
                    Console.WriteLine("try again!");
                    continue;
                }

                valid = true;
            }

            return answer;
        }
    }
}
