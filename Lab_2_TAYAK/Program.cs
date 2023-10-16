namespace Lab_2_TAYAK
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class FiniteAutomaton
    {
        public Dictionary<string, Dictionary<char, HashSet<string>>> transitions; // переходы
        public HashSet<string> finalStates; // финальные состояния

        public FiniteAutomaton()
        {
            transitions = new Dictionary<string, Dictionary<char, HashSet<string>>>();
            finalStates = new HashSet<string>();
        }

        public void AddTransition(string fromState, char inputSymbol, string toState)
        {
            if (!transitions.ContainsKey(fromState))
            {
                transitions[fromState] = new Dictionary<char, HashSet<string>>();
            }

            if (!transitions[fromState].ContainsKey(inputSymbol))
            {
                transitions[fromState][inputSymbol] = new HashSet<string>();
            }

            transitions[fromState][inputSymbol].Add(toState);
        }

        public void AddFinalState(string state)
        {
            finalStates.Add(state);
        }

        public bool IsDeterministic()
        {
            foreach (var state in transitions.Keys)
            {
                foreach (var inputSymbol in transitions[state].Keys)
                {
                    if (transitions[state][inputSymbol].Count > 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public FiniteAutomaton ConvertToDeterministic()
        {
            if (IsDeterministic())
            {
                return this;
            }

            FiniteAutomaton deterministicAutomaton = new FiniteAutomaton();
            Dictionary<HashSet<string>, string> newStatesMap = new Dictionary<HashSet<string>, string>();

            HashSet<string> startState = Closure(new HashSet<string> { "q0" });
            Queue<HashSet<string>> stateQueue = new Queue<HashSet<string>>();
            stateQueue.Enqueue(startState);
            newStatesMap[startState] = "q0";

            while (stateQueue.Count > 0)
            {
                HashSet<string> currentStateSet = stateQueue.Dequeue();

                foreach (char inputSymbol in GetInputSymbols(currentStateSet))
                {
                    HashSet<string> newStateSet = new HashSet<string>();
                    foreach (string state in currentStateSet)
                    {
                        if (transitions.ContainsKey(state) && transitions[state].ContainsKey(inputSymbol))
                        {
                            newStateSet.UnionWith(transitions[state][inputSymbol]);
                        }
                    }

                    if (!newStatesMap.ContainsKey(newStateSet))
                    {
                        string newStateName = "q" + newStatesMap.Count;
                        newStatesMap[newStateSet] = newStateName;
                        stateQueue.Enqueue(newStateSet);
                    }

                    string fromStateName = newStatesMap[currentStateSet];
                    string toStateName = newStatesMap[newStateSet];
                    deterministicAutomaton.AddTransition(fromStateName, inputSymbol, toStateName);

                    if (newStateSet.Overlaps(finalStates))
                    {
                        deterministicAutomaton.AddFinalState(toStateName);
                    }
                }
            }

            return deterministicAutomaton;
        }

        public bool Accepts(string input)
        {
            string currentState = "q0";

            foreach (char symbol in input)
            {
                if (!transitions.ContainsKey(currentState) || !transitions[currentState].ContainsKey(symbol))
                {
                    return false;
                }
                Console.Write(symbol.ToString());
                currentState = transitions[currentState][symbol].First();
            }

            return finalStates.Contains(currentState);
        }

        private HashSet<char> GetInputSymbols(HashSet<string> states)
        {
            HashSet<char> symbols = new HashSet<char>();
            foreach (string state in states)
            {
                if (transitions.ContainsKey(state))
                {
                    symbols.UnionWith(transitions[state].Keys);
                }
            }
            return symbols;
        }

        private HashSet<string> Closure(HashSet<string> states)
        {
            HashSet<string> closure = new HashSet<string>(states);
            bool added = true;

            while (added)
            {
                added = false;
                foreach (string state in closure.ToList())
                {
                    if (transitions.ContainsKey(state) && transitions[state].ContainsKey('\0'))
                    {
                        foreach (string nextState in transitions[state]['\0'])
                        {
                            if (!closure.Contains(nextState))
                            {
                                closure.Add(nextState);
                                added = true;
                            }
                        }
                    }
                }
            }

            return closure;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("Введите имя файла с описанием автомата: ");
            //string filename = Console.ReadLine();
            string filename = "Files\\var1.txt";

            FiniteAutomaton automaton = ParseAutomatonFromFile(filename);
            Console.WriteLine("Детерминированный: " + automaton.IsDeterministic());

            if (!automaton.IsDeterministic())
            {
                FiniteAutomaton deterministicAutomaton = automaton.ConvertToDeterministic();
                PrintAutomaton(deterministicAutomaton);
            }

            Console.Write("Введите строку для проверки: ");
            string inputString = Console.ReadLine();
            bool accepted = automaton.Accepts(inputString);
            Console.WriteLine("Разбор возможен: " + accepted);
        }

        static FiniteAutomaton ParseAutomatonFromFile(string filename)
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
                            if (line.Contains("=="))
                            {
                                parts = line.Split("=", 2);
                                sourceAndSymbol = parts[0].Split(',', 2);
                                sourceAndSymbol[1] += "=";
                            }
                            else
                            {
                                parts = line.Split('=');
                                sourceAndSymbol = parts[0].Split(',', 2);
                            }

                            string fromState = sourceAndSymbol[0];
                            char inputSymbol = sourceAndSymbol[1][0];

                            string toState = parts[1];

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

        static void PrintAutomaton(FiniteAutomaton automaton)
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