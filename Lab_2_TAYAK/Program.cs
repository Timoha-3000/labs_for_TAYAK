using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lab_2_TAYAK.Automatic;

namespace Lab_2_TAYAK
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("Введите имя файла с описанием автомата: ");
            //string filename = Console.ReadLine();
            string filename = "Files\\var1.txt"; // ab+cd*e=357
            //string filename = "Files\\var3_nd.txt"; // не детерминированный автомат

            FiniteAutomaton automaton = AutomaticHelper.ParseAutomatonFromFile(filename);
            Console.WriteLine("Детерминированный: " + automaton.IsDeterministic());

            if (!automaton.IsDeterministic())
            {
                FiniteAutomaton deterministicAutomaton = automaton.ConvertToDeterministic();
                AutomaticHelper.PrintAutomaton(deterministicAutomaton);
            }

            Console.Write("Введите строку для проверки: ");
            string inputString = Console.ReadLine();
            bool accepted = automaton.Accepts(inputString);
            Console.WriteLine("\nРазбор возможен: " + accepted);
        }
    }
}