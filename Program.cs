using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LFA_PROYECTO1
{
    class Automata
    {
        public int NumberOfStates { get; set; }
        public int InitialState { get; set; }
        public List<int> FinalStates { get; set; } = new List<int>();
        public List<Tuple<int, string, int?>> Transitions { get; set; } = new List<Tuple<int, string, int?>>();

        public Automata()
        {
        }

        public void AddTransition(int initialState, string readString, int? state)
        {
            Transitions.Add(new Tuple<int, string, int?>(initialState, readString, state));
        }
        public bool IsAccepted(string input)
        {
            int currentState = InitialState;
            Console.WriteLine($"Iniciando en el estado: {currentState}");
            foreach (char c in input)
            {
                string currentInput = c.ToString();
                var transition = Transitions.FirstOrDefault(t => t.Item1 == currentState && t.Item2 == currentInput);
                if (transition == null)
                {
                    Console.WriteLine($"No hay transición válida desde el estado {currentState} con el símbolo '{c}'. Cadena no aceptada.");
                    return false;
                }
                Console.WriteLine($"Transición desde el estado {currentState} a {transition.Item3} con '{c}'");
                currentState = transition.Item3.HasValue ? transition.Item3.Value : currentState; // Manejo de transiciones epsilon
            }

            Console.WriteLine($"Cadena completa procesada. Estado final: {currentState}");
            bool isFinalStateAccepted = FinalStates.Contains(currentState);
            //Console.WriteLine($"La cadena es {(isFinalStateAccepted ? "aceptada" : "no aceptada")}.");
            return isFinalStateAccepted;
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
                int? state = parts[2].Trim().Equals("e", StringComparison.OrdinalIgnoreCase) ? null : (int?)int.Parse(parts[2].Trim());

                automata.AddTransition(initialState, readString, state);
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
                string readString = parts[1].Equals("e", StringComparison.OrdinalIgnoreCase) ? "" : parts[1];
                int? state = parts[2].Equals("e", StringComparison.OrdinalIgnoreCase) ? null : (int?)int.Parse(parts[2]);

                automata.AddTransition(initialState, readString, state);
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
                InitialState = (int)jsonObject["estados_inicial"]
            };

            foreach (var finalState in jsonObject["estado_final"])
            {
                automata.FinalStates.Add((int)finalState);
            }

            foreach (var element in jsonObject["transiciones"])
            {
                int initialState = (int)element[0];
                string readString = (string)element[1];
                if (readString.Equals("e", StringComparison.OrdinalIgnoreCase))
                {
                    readString = "";
                }

                int? state = (string)element[2] == "e" ? (int?)null : (int)element[2];

                automata.AddTransition(initialState, readString, state);
            }
            return automata;
        }
    }
}