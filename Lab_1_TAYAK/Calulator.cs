using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lab_1_TAYAK
{
    public class Calulator
    {
        /// <summary>
        /// начальное состояние (положение) калькулятора, в котором он просит 
        /// ввести выражение, если выражение введено, то запускает метод для
        /// вычисления выражения. Можно сказать что это MainCycle
        /// </summary>
        public void StartPosition()
        {
            ConsoleHelper.WriteQuestion("Чтобы выйти введите exit \nВведите выражение, которое хотите вычестить: ");
            string? expression = Console.ReadLine();

            if (expression == null)
            {
                ConsoleHelper.WriteQuestion("Введите выражение, которое хотите вычестить: ");
                Console.Clear();
                StartPosition();
            }
            else if (expression == "exit")
            {
                Environment.Exit(0);
            }
            else
            {
                try
                {
                    double result = CalculateInfix(expression);
                    ConsoleHelper.WriteAnswer("Результат: " + result);
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError("Ошибка: " + ex.Message);
                }

                StartPosition();
            }
        }

        private double CalculateInfix(string expression)
        {
            expression = NormalizeExpression(expression);
            Queue<string> output = MainYard(expression);
            return EvaluateRPN(output);
        }

        private string NormalizeExpression(string expression)
        {
            expression = expression.Replace(" ", "");
            expression = Regex.Replace(expression, @"(?<=\d)-", " - 0 -"); // Заменяем унарный минус перед числами на 'N'
            expression = expression.Replace("(-", "(0-"); // Заменяем (- на (0-
            return expression;
        }

        private Queue<string> MainYard(string expression)
        {
            Queue<string> output = new Queue<string>();
            Stack<string> operators = new Stack<string>();

            string[] tokens = Tokenize(expression);

            if (tokens.Contains("log"))
            {
                // сюды над будет что-то придумывать, если log не идёт первым аргументом в выражении
                while (tokens.Contains("log"))
                {
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        if (tokens[i] == "log")
                        {
                            operators.Push(tokens[i]);
                            output.Enqueue(tokens[i + 2].ToString());
                            tokens[i] = "";
                            tokens[i + 1] = "";
                            tokens[i + 2] = "";
                            tokens[i + 3] = "";

                        }
                    }
                }
            }

            foreach (string token in tokens)
            {
                if (IsNumber(token))
                {
                    output.Enqueue(token);
                }
                else if (IsFunction(token))
                {
                    operators.Push(token); // log
                }
                else if (IsOperator(token))
                {
                    while (operators.Count > 0 && IsOperator(operators.Peek()) &&
                           Precedence(operators.Peek()) >= Precedence(token))
                    {
                        output.Enqueue(operators.Pop());
                    }
                    operators.Push(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Enqueue(operators.Pop());
                    }
                    if (operators.Count == 0 || operators.Pop() != "(")
                    {
                        throw new ArgumentException("скобки, скобки, блин, скобки!!!");
                    }
                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                    {
                        output.Enqueue(operators.Pop());
                    }
                }
                else if (token == "" && operators.Contains("log")) { }
                else if (token.Length == 1) { }
                else
                {
                    throw new ArgumentException("Неверный токен: " + token);
                }
            }

            while (operators.Count > 0)
            {
                if (operators.Peek() == "(" || operators.Peek() == ")")
                {
                    throw new ArgumentException("чё со скобками, ээ");
                }
                output.Enqueue(operators.Pop());
            }

            return output;
        }

        private double EvaluateRPN(Queue<string> rpn)
        {
            Stack<double> stack = new Stack<double>();
            for (int i = 0; rpn.Count > 0; i++)
            {
                string token = rpn.Dequeue();
                string logArg = "";
                if (IsNumber(token) || token.Contains(";"))
                {
                    if (rpn.Count != 0)
                        if (IsFunction(rpn.First()))
                        {
                            for (int j = 0; token[j] != char.Parse(";"); j++)
                                logArg += token[j];
                            stack.Push(double.Parse(logArg.ToString()));

                            logArg = "";

                            for (int j = token.IndexOf(";"); j < token.Length - 1; j++)
                                logArg += token[j + 1];
                            stack.Push(double.Parse(logArg.ToString()));
                        }
                        else
                            stack.Push(double.Parse(token));
                }
                else if (IsOperator(token))
                {
                    if (stack.Count < 2)
                    {
                        throw new ArgumentException("Недостаточно операндов для оператора =<" + token);
                    }
                    double operand2 = stack.Pop();
                    double operand1 = stack.Pop();
                    double result = PerformOperation(token, operand1, operand2);
                    stack.Push(result);
                }
                else if (IsFunction(token))
                {
                    if (stack.Count < 2)
                    {
                        throw new ArgumentException("Недостаточно аргументов для функции " + token);
                    }
                    double argument2 = stack.Pop();
                    double argument1 = stack.Pop();
                    double result = PerformFunction(token, argument1, argument2);
                    stack.Push(result);
                }
            }
            if (stack.Count != 1)
            {
                Console.WriteLine(stack);
                //throw //new ArgumentException("Неверное выражение");
            }
            return stack.Pop();
        }

        /// <summary>
        /// метод, разбивающий обычное представление выражений,
        /// в представление обратной польской нотации
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string[] Tokenize(string expression)
        {
            List<string> tokens = new List<string>();
            StringBuilder currentToken = new StringBuilder();
            foreach (char c in expression)
            {
                if (IsOperator(c.ToString()) || c == '(' || c == ')')
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                    }
                    tokens.Add(c.ToString());
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }
            return tokens.ToArray();
        }

        private bool IsNumber(string token)
        {
            double result;

            return double.TryParse(token, out result);
        }

        bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/";
        }

        bool IsFunction(string token)
        {
            return token == "log";
        }

        /// <summary>
        /// метод замещающий таблицу Дийикстры
        /// раздаёт приоритеты по выполнению функций
        /// </summary>
        /// <param name="op"> операнд</param>
        /// <returns></returns>
        int Precedence(string op)
        {
            if (op == "+" || op == "-") return 1;
            if (op == "*" || op == "/") return 2;
            return 0;
        }

        /// <summary>
        /// Здесь обрабатываются различные мат. операции с дефолтными операндами
        /// типа +,-,*,/
        /// </summary>
        /// <param name="operation"> операнд </param>
        /// <param name="operand1"> 1-ый аргумент</param>
        /// <param name="operand2"> 2-ой аргумент</param>
        /// <returns></returns>
        /// <exception cref="DivideByZeroException"> Исключение, к-ое возникает при делении на ноль </exception>
        /// <exception cref="ArgumentException"> Ошибка при обработке аргумента </exception>
        private double PerformOperation(string operation, double operand1, double operand2)
        {
            switch (operation)
            {
                case "+":
                    return operand1 + operand2;
                case "-":
                    return operand1 - operand2;
                case "*":
                    return operand1 * operand2;
                case "/":
                    if (operand2 == 0)
                        throw new DivideByZeroException("Деление на ноль");
                    
                    return operand1 / operand2;
                default:
                    throw new ArgumentException("Неизвестная операция: " + operation);
            }
        }

        /// <summary>
        /// метод в который поступает имя функции, и два её аргумента
        /// </summary>
        /// <param name="function"> имя функции (мат. операции)</param>
        /// <param name="argument1"> первый аргумент</param>
        /// <param name="argument2"> второй аргумент</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private double PerformFunction(string function, double argument1, double argument2)
        {
            if (function == "log")
            {
                if (argument1 <= 0 || argument2 <= 0)
                {
                    throw new ArgumentException("Логарифмы должны быть положительными числами");
                }
                return Math.Log(argument2, argument1);
            }
            else
            {
                throw new ArgumentException("Неизвестная функция: " + function);
            }
        }
    }
}
