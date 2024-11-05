using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProyectoConsola.Enumeraciones;
using ProyectoConsola.Managers;
using ProyectoConsola.Estructuras;
using static System.Collections.Specialized.BitVector32;

namespace ProyectoConsola.Parsing
{
    public class LALRParser
    {
        private readonly Dictionary<int, Dictionary<object, (LALRAction, int)>> _actionTable;
        private readonly string _startSymbol;
        SectionsManager _sectionsManager;


        public LALRParser(Dictionary<int, Dictionary<object, (LALRAction, int)>> actionTable, string startSymbol, SectionsManager sm)
        {
            _actionTable = actionTable;
            _startSymbol = startSymbol;
            _sectionsManager = sm;
        }
        private List<(string identifier, string production)> RearrangeTokens(List<Token> tuplas)
        {
            List<(string identifier, string production)> tokens = new List<(string identifier, string production)>();
            foreach (Token t in tuplas)
            {
                (string identifier, string production) tok = (t.identifier, t.production);
                tokens.Add(tok);
            }
            return tokens;
        }
        private List<string> StringToListString(string str)
        {
            // Reemplazamos múltiples espacios en blanco por un solo espacio
            Regex spacingRegex = new Regex(@"\s+");
            string cleanedString = spacingRegex.Replace(str.Trim(), " ");
            // Dividimos la cadena en palabras usando el espacio como delimitador
            List<string> result = cleanedString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return result;
        }
        private List<(LALRAction action, int actionInt)> GetAction(int currentState, string currentSymbol)
        {
            if (_actionTable.TryGetValue(currentState, out var actions))
            {
                if (actions.TryGetValue(currentSymbol, out var action))
                {
                    return new List<(LALRAction action, int actionInt)> { action };
                }

                foreach (var action1 in actions)
                {
                    if (action1.Key is List<string> keySymbols && keySymbols.Contains(currentSymbol))
                    {
                        return new List<(LALRAction action, int actionInt)> { action1.Value };
                    }
                }
            }
            return new List<(LALRAction action, int actionInt)> { (LALRAction.Default, -1) };
        }
        private string VerifyToken(string procpectToken)
        {
            if (_sectionsManager.IsToken(procpectToken))
            {

                return _sectionsManager.GetTokenIdentifier(procpectToken);
            }
            else
                return procpectToken;
        }
        private bool ValidateAction((LALRAction action, int actionInt) action, Stack<int> stateStack, Stack<(string symbol, string value)> symbolStack, ref string currentSymbol, ref int index, List<string> input)
        {
            Console.WriteLine($"{action.action}{action.actionInt}, simbolo actual: {currentSymbol}");
            switch (action.action)
            {
                case LALRAction.Shift:
                    // Realiza un "shift"
                    stateStack.Push(action.actionInt);
                    symbolStack.Push((VerifyToken(currentSymbol), currentSymbol));
                    index++;
                    if (index < input.Count)
                        currentSymbol = VerifyToken(input[index]);
                    else
                        currentSymbol = "$"; // Fin de la entrada
                    return true;

                case LALRAction.Reduce:
                    Tuple<string, string> production = _sectionsManager._orderedNonTerminals[action.actionInt];
                    string[] splitProduction = production.Item2.Split(' ');
                    if (splitProduction.Length == 1 && splitProduction[0].Equals("ε"))
                    {
                        
                    }
                    else
                    {
                        List<string> values = new List<string>();
                        // Realiza la reducción normal
                        foreach (var symbol in splitProduction)
                        {
                            stateStack.Pop();
                            (string symbol, string value) symbol_Value = symbolStack.Pop();
                            values.Add(symbol_Value.value);
                        }
                        // Luego empujar el nuevo estado correspondiente
                        var action1 = GetAction(stateStack.Peek(), production.Item1);
                        if (action1.ToArray()[0].action.Equals(LALRAction.Goto))
                        {
                            int newState = action1.ToArray()[0].actionInt; // Determina el nuevo estado
                            stateStack.Push(newState);
                            symbolStack.Push((production.Item1, DoAction(production.Item1, production.Item2, values))); // Empuja el símbolo no terminal
                        }
                        
                    }
                    return true;

                case LALRAction.Accept:
                    // Aceptar la cadena
                    return true;

                default:
                    // Manejar error: acción no válida
                    return false;
            }
        }
        private string DoAction(string nonTerminal,string production,List<string> values)
        {
            string result = "";
            List<string> actions ;
            var actionsDictionary = _sectionsManager._nonTerminalActions;
            if (actionsDictionary.Keys.Contains(nonTerminal) && actionsDictionary[nonTerminal].Keys.Contains(production))
            {
                actions = _sectionsManager._nonTerminalActions[nonTerminal][production];
                foreach(var  action in actions)
                {
                    
                    switch (action)
                    {

                        case default:
                            break;
                    }

                }
            }
            return result;
        }
        public bool Parse(string inputString)
        {

            FileManager fm = new FileManager();
            string inString;
            if (!inputString.Equals(String.Empty) && !inputString.Equals(" "))
                inString = fm.ReadNewFile(inputString);
            else
            {
                inString = inputString;
            }
            List<string> input;
            if (inString.Equals(""))
            {
                input = StringToListString(inputString);
            }
            else
            {
                input = StringToListString(inString);
            }
            input.Add("$");
            Stack<int> stateStack = new Stack<int>();
            Stack<(string symbol, string value)> symbolStack = new Stack<(string symbol, string value)>();
            stateStack.Push(0); // Estado inicial

            int index = 0; // Índice de la cadena de entrada
            string currentSymbol = input[index];

            while (true)
            {
                int currentState = stateStack.Peek();
                List<(LALRAction action, int actionInt)> actions = GetAction(currentState, currentSymbol);
                if (actions.Count > 1)
                {
                    var actionsL = actions.ToList();
                    foreach (var action in actionsL)
                    {
                        if (ValidateAction(action, stateStack, symbolStack, ref currentSymbol, ref index, input))
                        {
                            if (action.action.Equals(LALRAction.Accept))
                            {
                                return true;
                            }
                        }
                        else if(actionsL.Last().Equals(action))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    (LALRAction action, int actionInt)  action = actions.ToArray()[0];
                    if (ValidateAction(action, stateStack, symbolStack, ref currentSymbol, ref index, input))
                    {
                        if (action.action.Equals(LALRAction.Accept))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Manejar error: no hay acción válida
                        return false;
                    }
                }
            }
        }
    }
}