using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaPvP.Models
{
    public class Player
    {
        //public int PlayerId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }

        public Player(string name)
        {
            Name = name;
            Score = 0;
        }

        internal void AddScore(int points)
        {
            Score += points;
        }
    }
}
