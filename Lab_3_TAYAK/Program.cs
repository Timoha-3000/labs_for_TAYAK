namespace Lab_3_TAYAK
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            // Путь к файлу с правилами
            string filePath = "test1.txt";

            try
            {
                // Чтение правил из файла
                Dictionary<char, List<string>> grammar = ReadGrammar(filePath);

                //Console.WriteLine("Введите строку для анализа:"); // Входная строка для анализа
                //string inputString = Console.ReadLine();
                string inputString = "EmT/0-0";

                // Вызов функции анализа
                bool isAccepted = AnalyzeString(grammar, inputString);

                // Вывод результата
                Console.WriteLine(isAccepted ? "Строка допустима." : "Строка недопустима.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private static Dictionary<char, List<string>> ReadGrammar(string filePath)
        {
            var grammar = new Dictionary<char, List<string>>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var parts = line.Split('>');
                char nonTerminal = parts[0][0];
                var productions = parts[1].Split('|');

                if (!grammar.ContainsKey(nonTerminal))
                {
                    grammar[nonTerminal] = new List<string>();
                }

                foreach (var production in productions)
                {
                    grammar[nonTerminal].Add(production);
                }
            }

            return grammar;
        }

        private static bool AnalyzeString(Dictionary<char, List<string>> grammar, string inputString)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push("E"); // Стартовый символ грамматики

            while (stack.Count > 0)
            {
                string top = stack.Peek();

                if (string.IsNullOrEmpty(inputString) && top == "E" && stack.Count == 1)
                {
                    // Если входная строка пуста и на вершине стека стартовый символ
                    return true;
                }

                if (string.IsNullOrEmpty(inputString) && string.IsNullOrEmpty(top))
                {
                    // Если входная строка или вершина стека пусты
                    return false;
                }

                List<string> listRuls = GetRules(grammar, inputString);

                if (char.IsUpper(top[0]))
                {
                    // Если на вершине стека нетерминальный символ
                    stack.Pop();
                    if (grammar.ContainsKey(top[0]))
                    {
                        /*foreach (var production in grammar[top[0]])
                        {
                            foreach (var item in production)
                            {
                                if (inputString.StartsWith(item))
                                {
                                    *//*foreach (var item in production)
                                        inputString = inputString.Trim(item);*//*
                                    foreach (var symbol in production.Reverse())
                                        if (symbol != '>')
                                        {
                                            stack.Push(symbol.ToString());
                                            inputString = inputString.Trim(symbol);
                                        }
                                    break;
                                }

                            }
                        }*/
                        foreach (var rules in listRuls)
                        {
                            foreach (var rule in rules)
                            {
                                if (inputString.StartsWith(rule))
                                {
                                    foreach (var symbol in rules.Reverse())
                                        if (symbol != '>')
                                        {
                                            stack.Push(symbol.ToString());
                                            inputString = inputString.Trim(symbol);
                                        }
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (top[0] == inputString[0])
                {
                    // Если на вершине стека терминал, который совпадает с текущим символом строки
                    stack.Pop();
                    //inputString = inputString.Substring(1);
                }
                else
                {
                    stack.Pop();
                    //return false;
                }
            }

            return inputString.Length == 0;
        }

        private static List<string> GetRules(Dictionary<char, List<string>> grammar, string inputString)
        {
            List<string> rules = new List<string>();

            foreach (var ssss in inputString)
            {
                    inputString = inputString.Trim('E');
                    if (grammar.ContainsKey(ssss) && char.IsUpper(ssss))
                    {
                        foreach (var item in grammar[ssss])
                        {
                            foreach (var item_sybol in item)
                                if (inputString.StartsWith(item_sybol))
                                {
                                    rules.Add(item_sybol.ToString());
                                    inputString = inputString.Trim(item_sybol); 
                                }
                                else
                                {
                                    inputString = inputString.Trim(item_sybol);
                                }
                        }
                    }
            }
            return rules;
        }
    }
}