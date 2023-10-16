using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_TAYAK.Automatic
{
    public static class AutomaticHelper
    {
        public static FiniteAutomaton ParseAutomatonFromFile(string filename)
        {
            FiniteAutomaton automaton = new FiniteAutomaton();

            using (StreamReader reader = new StreamReader(filename))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    i++;
                    string? line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            string[] parts;
                            string[] sourceAndSymbol = new string[1];
                            string toState;
                            string fromState;
                            char inputSymbol;
                            if (line.Contains("=="))
                            {
                                parts = line.Split("=");
                                sourceAndSymbol = parts[0].Split(',', 2);
                                sourceAndSymbol[1] += "=";
                                fromState = sourceAndSymbol[0];
                                inputSymbol = sourceAndSymbol[1][0];
                                toState = parts[2];
                            }
                            else
                            {
                                parts = line.Split('=');
                                sourceAndSymbol = parts[0].Split(',', 2);
                                fromState = sourceAndSymbol[0];
                                inputSymbol = sourceAndSymbol[1][0];
                                toState = parts[1];
                            }

                            automaton.AddTransition(fromState, inputSymbol, toState);
                            if (toState.Contains("f"))
                            {
                                automaton.AddFinalState(fromState);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"не верно введённая строка № {i} \n{ex.Message}");
                            throw;
                        }
                    }
                }
            }

            return automaton;
        }

        public static void PrintAutomaton(FiniteAutomaton automaton)
        {
            Console.WriteLine("Переходы детерминированного автомата:");

            foreach (var fromState in automaton.transitions)
            {
                foreach (var inputSymbol in automaton.transitions[fromState.Key])
                {
                    foreach (var toState in automaton.transitions[fromState.Key][inputSymbol.Key])
                    {
                        Console.WriteLine($"{fromState.Key}, {inputSymbol.Key} => {toState}");
                    }
                }
            }

            Console.WriteLine("Конечные состояния: " + string.Join(", ", automaton.finalStates));
        }
    }
}
