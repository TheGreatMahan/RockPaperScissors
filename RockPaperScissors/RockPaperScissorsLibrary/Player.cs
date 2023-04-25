using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockPaperScissorsLibrary
{
    internal class Player
    {
        public string Name { get; set; }
        public bool IsHost { get; set; }
        public ICallback Callback { get; set; }
        public int Score { get; set; }
        public string Hand { get; set; }
    }
}
