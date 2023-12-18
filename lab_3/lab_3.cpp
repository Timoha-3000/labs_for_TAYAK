#include <string>
#include <vector>
#include <algorithm>
#include <iostream>
#include <fstream>
#include <map>
#include <set>
#include <regex>

using namespace std;

/// <summary>
/// Структура для хранения информации о каждом состоянии анализа строки, включая текущий символ, входную строку, стек и индекс.
/// </summary>
struct Link
{
	char s;
	string inp;
	string stack;
	int index;
	bool term;

	Link(char s, string p, string h, bool t) : s(s), inp(p), stack(h), index(-1), term(t) { }
	Link(char s, string p, string h) : s(s), inp(p), stack(h), index(-1) { }
};

/// <summary>
/// Структура для хранения аргументов функции. Включает в себя символы состояния, входа и стека.
/// </summary>
struct Fargs
{
	char s;
	char p;
	char h;

	Fargs(char s, char p, char h) : s(s), p(p), h(h) { }
};

/// <summary>
/// Структура для хранения значения, ассоциированного с символом состояния и строкой.
/// </summary>
struct Value
{
	char s;
	string c;

	Value(char s, string c) : s(s), c(c) { }
};

/// <summary>
/// Структура для хранения команды, включающей в себя аргументы функции и вектор значений.
/// </summary>
struct Command
{
	Fargs f;
	vector<Value> values;

	Command(Fargs f, vector<Value> v) : f(f), values(v) { }
};

class Storage
{
private:
	ifstream file;
	set<char> P;
	set<char> H;
	char s0 = '0', h0 = '|', empty_symbol = '\0';
	vector<Command> commands;
	vector<Link> chain;

public:

	/// <summary>
	/// Загружает команды из файла и инициализирует начальные символы, алфавиты и набор команд.
	/// </summary>
	/// <param name="filename"></param>
	Storage(const char* filename) : file(filename)
	{
		if (!file.is_open())
			throw runtime_error("Could not open the file for reading\n");
		string tmpStr;
		int vsize;
		const regex exp("([[:upper:]])>([[:print:]]+)");
		smatch match;
		while (getline(file, tmpStr))
		{
			if (tmpStr.size() == 0)
				continue;
			if (!regex_match(tmpStr, match, exp) || tmpStr[tmpStr.size() - 1] == '|' || tmpStr[2] == '|')
			{
				throw runtime_error("Не удалось распознать содержимое файла\n");
			}
			else
			{
				H.insert(match[1].str()[0]);
				commands.push_back(Command(Fargs(s0, empty_symbol, match[1].str()[0]), vector<Value>()));
				commands[commands.size() - 1].values.push_back(Value(s0, ""));
				for (int i = 0; i < match[2].str().size(); i++)
				{
					if (match[2].str()[i] == '|')
					{
						if (match[2].str()[i - 1] != '|')
							commands[commands.size() - 1].values.push_back(Value(s0, ""));
					}
					else
					{
						P.insert(match[2].str()[i]);
						vsize = commands[commands.size() - 1].values.size();
						commands[commands.size() - 1].values[vsize - 1].c.push_back(match[2].str()[i]);
					}
				}

				for (int i = 0; i < commands[commands.size() - 1].values.size(); i++)
					reverse(commands[commands.size() - 1].values[i].c.begin(), commands[commands.size() - 1].values[i].c.end());
			}
		}
		for (const auto& c : H)
			P.erase(c);
		for (const auto& c : P)
			commands.push_back(Command(Fargs(s0, c, c), vector<Value>({ Value(s0, "\0") })));
		commands.push_back(Command(Fargs(s0, empty_symbol, h0), vector<Value>({ Value(s0, "\0") })));
	}

	/// <summary>
	/// Отображает информацию о алфавите, командах и символах.
	/// </summary>
	void showInfo() // 
	{
		cout << "Input alphabet:\nP = {";
		for (const auto& c : P)
			cout << c << ", ";
		cout << "\b\b}\n\n";
		cout << "Alphabet of symbols:\nZ = {";
		for (const auto& c : H)
			cout << c << ", ";
		for (const auto& c : P)
			cout << c << ", ";
		cout << "h0}\n\n";

		cout << "List of commands:\n";
		for (const auto& c : commands)
		{
			cout << "f(s" << c.f.s << ", ";
			if (c.f.p == empty_symbol)
				cout << "lambda";
			else
				cout << c.f.p;
			cout << ", ";
			if (c.f.h == h0)
				cout << "h0";
			else
				cout << c.f.h;
			cout << ") = {";
			for (Value v : c.values)
			{
				cout << "(s" << v.s << ", ";
				if (v.c[0] == empty_symbol)
					cout << "lambda";
				else
					cout << v.c;
				cout << "); ";

			}
			cout << "\b\b}\n";
		}
		cout << endl;
	}

	/// <summary>
	/// Отображает цепочку конфигураций, которые были сгенерированы в процессе анализа строки.
	/// </summary>
	void showChain()
	{
		cout << "\nConfiguration chain: \n";
		for (const auto& link : chain)
			cout << "(s" << link.s << ", " << ((link.inp.size() == 0) ? "lambda" : link.inp) << ", h0" << link.stack << ") |– \n";
		cout << "(s0, lambda, lambda)" << endl;
	}

	/// <summary>
	/// Проверяет допустимость переходов согласно определенным правилам.
	/// </summary>
	/// <returns></returns>
	bool push_link()
	{
		int ch_size = chain.size();
		int mag_size, j, i;
		for (i = 0; i < commands.size(); i++) {
			mag_size = chain[ch_size - 1].stack.size();
			if (chain[chain.size() - 1].inp.size() != 0 && chain[chain.size() - 1].stack.size() != 0 && chain[ch_size - 1].s == commands[i].f.s && (chain[ch_size - 1].inp[0] == commands[i].f.p || empty_symbol == commands[i].f.p) && chain[ch_size - 1].stack[mag_size - 1] == commands[i].f.h)
			{
				for (j = 0; j < commands[i].values.size(); j++)
				{
					if (commands[i].f.p == empty_symbol)
					{
						chain.push_back(Link(commands[i].values[j].s, chain[ch_size - 1].inp, string(chain[ch_size - 1].stack)));
					}
					else
					{
						chain.push_back(Link(commands[i].values[j].s, chain[ch_size - 1].inp, string(chain[ch_size - 1].stack)));
						reverse(chain[ch_size].inp.begin(), chain[ch_size].inp.end());
						chain[ch_size].inp.pop_back();
						reverse(chain[ch_size].inp.begin(), chain[ch_size].inp.end());
					}

					chain[ch_size].stack.pop_back();
					chain[ch_size].stack += commands[i].values[j].c;

					if (chain[ch_size].inp.size() < chain[ch_size].stack.size())
					{
						chain.pop_back();
						chain.pop_back();
						return false;
					}
					else
					{
						if (chain[chain.size() - 1].inp.size() == 0 && chain[chain.size() - 1].stack.size() == 0 || push_link())
							return true;
					}
				}
			}
		}
		if (i == commands.size())
		{
			chain.pop_back();
			return false;
		}
	}

	/// <summary>
	/// Проверяет данную строку на соответствие правилам, определенным ранее.
	/// </summary>
	/// <param name="str"> входная строка </param>
	/// <returns></returns>
	bool check_line(const string& str) // checks a given string against the rules defined earlier
	{
		if (commands[0].values.size() == 1)
			chain.push_back(Link(s0, str, string(""), false));
		else
			chain.push_back(Link(s0, str, string(""), true));

		chain[0].stack.push_back(commands[0].f.h);

		bool res = push_link();
		if (res)
		{
			cout << "\nString is correct!\n";
			showChain();
		}
		else {
			cout << "Incorrect string, try again, fella!\n";
		}
		chain.clear();
		return res;
	}

	~Storage()
	{
		file.close();
	}
};

/// <summary>
/// MAIN emayo
/// </summary>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
int main(int argc, char* argv[])
{
	setlocale(LC_ALL, "Russian");
	string str;
	try {
		Storage strg("C:\\Users\\main\\source\\repos\\labs_for_TAYAK\\lab_3\\x64\\Debug\\test1.txt");
		strg.showInfo();
		while (true)
		{
			cout << "Input a string: ";
			getline(cin, str);
			strg.check_line(str);
			cout << endl;
		}
	}
	catch (const exception& err) {
		cerr << err.what() << endl;
	}
	return 0;
}