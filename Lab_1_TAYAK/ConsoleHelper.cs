using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_1_TAYAK
{
    public static class ConsoleHelper
    {
        public static void WriteAnswer(string answer)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("\n\n\t" + answer + "\n\n");
            Console.WriteLine("---------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteQuestion(string question)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(question);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
