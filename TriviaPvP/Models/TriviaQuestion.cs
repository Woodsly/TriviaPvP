using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaPvP.Models
{
    public class TriviaQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }

        public TriviaQuestion()
        {
            Options = new List<string>();
        }
    }
}
