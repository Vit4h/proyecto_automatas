using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace LFA_Proyecto2_AutomataNoDeterminista_
{
    class Automata
    {
        public int NumberOfStates { get; set; }
        public int InitialState { get; set; }
        public List<int> FinalStates { get; set; } = new List<int>();
        // Diccionario para manejar múltiples transiciones para cada par de estado y entrada.
        public Dictionary<Tuple<int, string>, List<int>> Transitions { get; set; } = new Dictionary<Tuple<int, string>, List<int>>();

        public void AddTransition(int initialState, string readString, List<int> states)
        {
            var key = new Tuple<int, string>(initialState, readString);
            if (Transitions.ContainsKey(key))
            {
                Transitions[key].AddRange(states);
            }
            else
            {
                Transitions[key] = new List<int>(states);
            }
        }

        public List<int> GetTransitions(int state, string symbol)
        {
            var key = new Tuple<int, string>(state, symbol);
            return Transitions.ContainsKey(key) ? Transitions[key] : new List<int>();
        }

        public List<int> EpsilonClosure(List<int> states)
        {
            var closure = new HashSet<int>(states);
            var stack = new Stack<int>(states);

            while (stack.Count > 0)
            {
                int currentState = stack.Pop();
                var epsilonTransitions = GetTransitions(currentState, ""); // "" representa una transición epsilon.

                foreach (int nextState in epsilonTransitions)
                {
                    if (!closure.Contains(nextState))
                    {
                        closure.Add(nextState);
                        stack.Push(nextState);
                    }
                }
            }

            return closure.ToList();
        }

        public bool IsAccepted(string input)
        {
            var currentStates = EpsilonClosure(new List<int> { InitialState });
            Console.WriteLine($"Iniciando en los estados: {string.Join(", ", currentStates)}");

            foreach (char c in input)
            {
                var newStates = new List<int>();
                string currentInput = c.ToString();
                foreach (int currentState in currentStates)
                {
                    newStates.AddRange(GetTransitions(currentState, currentInput));
                }

                if (!newStates.Any())
                {
                    Console.WriteLine($"No hay transición válida con el símbolo '{c}'. Cadena no aceptada.");
                    return false;
                }

                currentStates = EpsilonClosure(newStates.Distinct().ToList());
                Console.WriteLine($"Transición con '{c}' a los estados {string.Join(", ", currentStates)}");
            }

            Console.WriteLine($"Cadena completa procesada. Estados finales: {string.Join(", ", currentStates)}");
            return currentStates.Any(s => FinalStates.Contains(s));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            bool continueProcessing = true;
            string filePath = "";

            while (continueProcessing)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.Write("Ingrese la ruta del archivo: ");
                    filePath = Console.ReadLine();
                }

                Automata automata = LoadAutomata(filePath);
                if (automata == null)
                {
                    Console.WriteLine("No se pudo cargar el autómata. Verifique la ruta del archivo y el formato.");
                    continue;
                }

                Console.Write("Ingrese una cadena para verificar si es aceptada por el autómata: ");
                string userInput = Console.ReadLine();
                bool isAccepted = automata.IsAccepted(userInput);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"La cadena es {(isAccepted ? "aceptada" : "no aceptada")} por el autómata.");
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write("¿Desea ingresar otra cadena (si/no)? ");
                if (Console.ReadLine().Trim().ToLower() != "si")
                {
                    continueProcessing = false;
                    continue;
                }

                Console.Write("¿Desea cambiar la ruta del archivo (si/no)? ");
                if (Console.ReadLine().Trim().ToLower() == "si")
                {
                    filePath = "";
                }
            }
        }

        static Automata LoadAutomata(string filePath)
        {
            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".txt":
                    return ReadTxtFile(filePath);
                case ".csv":
                    return ReadCsvFile(filePath);
                case ".json":
                    return ReadJsonFile(filePath);
                default:
                    Console.WriteLine("Formato de archivo no soportado.");
                    return null;
            }
        }

        static Automata ReadTxtFile(string filePath)
        {
            Automata automata = new Automata();
            string[] lines = File.ReadAllLines(filePath);
            automata.NumberOfStates = int.Parse(lines[0].Trim());
            automata.InitialState = int.Parse(lines[1].Trim());

            foreach (var part in lines[2].Trim().Split(','))
            {
                automata.FinalStates.Add(int.Parse(part.Trim()));
            }

            for (int i = 3; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(',');
                int initialState = int.Parse(parts[0].Trim());
                string readString = parts[1].Trim();
                List<int> states = parts[2].Trim().Split('|').Select(x => x.Equals("e", StringComparison.OrdinalIgnoreCase) ? (int?)null : int.Parse(x.Trim())).Where(x => x.HasValue).Select(x => x.Value).ToList();

                automata.AddTransition(initialState, readString, states);
            }
            return automata;
        }

        static Automata ReadCsvFile(string filePath)
        {
            Automata automata = new Automata();
            string[] lines = File.ReadAllLines(filePath);

            automata.NumberOfStates = int.Parse(lines[0].Replace("\"", "").Trim());
            automata.InitialState = int.Parse(lines[1].Replace("\"", "").Trim());
            automata.FinalStates.AddRange(lines[2].Replace("\"", "").Trim().Split(',')
                                      .Select(part => int.Parse(part.Trim())));

            for (int i = 3; i < lines.Length; i++)
            {
                string[] parts = lines[i].Replace("\"", "").Split(',')
                                     .Select(part => part.Trim()).ToArray();
                int initialState = int.Parse(parts[0]);
                string readString = parts[1];
                List<int> states = parts[2].Split('|').Select(x => x.Equals("e", StringComparison.OrdinalIgnoreCase) ? (int?)null : int.Parse(x.Trim())).Where(x => x.HasValue).Select(x => x.Value).ToList();

                automata.AddTransition(initialState, readString, states);
            }
            return automata;
        }

        static Automata ReadJsonFile(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            var jsonObject = JObject.Parse(jsonString);

            var automata = new Automata
            {
                NumberOfStates = (int)jsonObject["no_estados"],
                InitialState = (int)jsonObject["estado_inicial"]
            };

            foreach (var finalState in jsonObject["estados_finales"])
            {
                automata.FinalStates.Add((int)finalState);
            }

            foreach (var element in jsonObject["transiciones"])
            {
                int initialState = (int)element[0];
                string readString = (string)element[1];
                List<int> states = ((JArray)element[2]).Select(x => x.ToString().Equals("e", StringComparison.OrdinalIgnoreCase) ? (int?)null : (int)x).Where(x => x.HasValue).Select(x => x.Value).ToList();

                automata.AddTransition(initialState, readString, states);
            }
            return automata;
        }
    }
}
