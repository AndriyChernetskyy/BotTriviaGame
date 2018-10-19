using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot_Builder_Echo_Bot_V4
{
    public class LuckyTrivia
    {
        public bool AnswerToFirstAnswer { get; set; }

        public bool AnswerToSecondAnswer { get; set; }

        public bool AnswerToThirdAnswer { get; set; }

        public bool AnswerToFourthAnswer { get; set; }

        public bool AnswerToFifthAnswer { get; set; }

        public int Points { get; set; } = 0;
    }
}
