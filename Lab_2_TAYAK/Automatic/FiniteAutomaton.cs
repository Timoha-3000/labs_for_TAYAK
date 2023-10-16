using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_2_TAYAK.Automatic
{
    public class FiniteAutomaton
    {
        public Dictionary<string, Dictionary<char, List<string>>> transitions; // переходы
        public HashSet<string> finalStates; // финальные состояния

        public FiniteAutomaton()
        {
            transitions = new Dictionary<string, Dictionary<char, List<string>>>();
            finalStates = new HashSet<string>();
        }

        public void AddTransition(string fromState, char inputSymbol, string toState)
        {
            if (!transitions.ContainsKey(fromState))
            {
                transitions[fromState] = new Dictionary<char, List<string>>();
            }

            if (!transitions[fromState].ContainsKey(inputSymbol))
            {
                transitions[fromState][inputSymbol] = new List<string>();
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
            FiniteAutomaton deterministicAutomaton = new FiniteAutomaton();
            Queue<string> statesQueue = new Queue<string>();
            statesQueue.Enqueue("q0");
            HashSet<string> visitedStates = new HashSet<string>();

            while (statesQueue.Count > 0)
            {
                string currentState = statesQueue.Dequeue();

                foreach (char symbol in GetAllInputSymbols())
                {
                    List<string> toStates = GetToStates(currentState, symbol);
                    if (toStates.Count > 0)
                    {
                        string newState = string.Join(",", toStates);
                        deterministicAutomaton.AddTransition(currentState, symbol, newState);
                        if (!visitedStates.Contains(newState))
                        {
                            statesQueue.Enqueue(newState);
                            visitedStates.Add(newState);
                        }
                    }
                }

                if (finalStates.Overlaps(currentState.Split(',')))
                {
                    deterministicAutomaton.AddFinalState(currentState);
                }
            }

            return deterministicAutomaton;
        }

        public bool Accepts(string input)
        {
            string currentState = "q0";
            string final_state = "";

            foreach (char symbol in input)
            {
                if (!transitions.ContainsKey(currentState) || !transitions[currentState].ContainsKey(symbol))
                {
                    return false;
                }
                Console.Write("->" + currentState);

                if (!transitions[currentState][symbol].First().Contains("f"))
                    currentState = transitions[currentState][symbol].First();
                else
                    final_state = transitions[currentState][symbol].First();
            }

            return finalStates.Contains(currentState);
        }

        private List<string> GetToStates(string fromState, char inputSymbol)
        {
            List<string> toStates = new List<string>();
            if (transitions.ContainsKey(fromState) && transitions[fromState].ContainsKey(inputSymbol))
            {
                toStates = transitions[fromState][inputSymbol];
            }
            return toStates;
        }

        private IEnumerable<char> GetAllInputSymbols()
        {
            HashSet<char> symbols = new HashSet<char>();
            foreach (var state in transitions.Keys)
            {
                foreach (var inputSymbol in transitions[state].Keys)
                {
                    symbols.Add(inputSymbol);
                }
            }
            return symbols;
        }

        private void PrintAutomaton(FiniteAutomaton automaton)
        {
            foreach (var fromState in automaton.transitions.Keys)
            {
                foreach (var inputSymbol in automaton.transitions[fromState].Keys)
                {
                    foreach (var toState in automaton.transitions[fromState][inputSymbol])
                    {
                        Console.WriteLine($"{fromState},{inputSymbol}={toState}");
                    }
                }
            }
            Console.WriteLine("Конечные состояния: " + string.Join(",", automaton.finalStates));
        }
    }
}
