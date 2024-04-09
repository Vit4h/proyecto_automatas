using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        // Método para determinar si una cadena es aceptada por el autómata
        public bool IsAccepted(string input)
        {
            int currentState = InitialState;
            foreach (char c in input)
            {
                string currentInput = c.ToString();
                var transition = Transitions.FirstOrDefault(t => t.Item1 == currentState && t.Item2 == currentInput);
                if (transition == null) // No se encontró una transición válida para el símbolo actual
                {
                    return false;
                }
                currentState = transition.Item3.HasValue ? transition.Item3.Value : currentState; // Manejo de transiciones epsilon
            }
            // Verificar si el estado actual después de procesar la cadena completa es un estado final
            return FinalStates.Contains(currentState);
        }
    }

    class Program
    {
        //C:\Users\driva\Desktop\files_example\automata.txt
        //C:\Users\driva\Desktop\files_example\automata.csv
        //C:\Users\driva\Desktop\files_example\automata.JSON
        static void Main(string[] args)
        {
            Console.Write("Ingrese la ruta del archivo: ");
            string filePath = Console.ReadLine();
            //string filePath = @"C:\Users\driva\Desktop\files_example\automata.txt";
            Automata automata = null;
            switch(Path.GetExtension(filePath).ToLower())
        {
            case ".txt":
                automata = ReadTxtFile(filePath);
                break;
            case ".csv":
                automata = ReadCsvFile(filePath); // Esta es la nueva integración
                break;
                default:
                Console.WriteLine("Formato de archivo no soportado.");
                return;
            }

            // Solicitar al usuario que ingrese una cadena
            Console.Write("Ingrese una cadena para verificar si es aceptada por el autómata: ");
            string userInput = Console.ReadLine();

            // Verificar si la cadena es aceptada por el autómata
            bool isAccepted = automata.IsAccepted(userInput);
            Console.WriteLine($"La cadena es {(isAccepted ? "aceptada" : "no aceptada")} por el autómata.");

            Console.ReadKey();
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
    }
}
//@"C:\Users\driva\Desktop\automata.txt"

