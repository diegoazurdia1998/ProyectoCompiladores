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
using static OfficeOpenXml.ExcelErrorValue;
using System.Collections.ObjectModel;

namespace ProyectoConsola.Parsing
{
    public class LALRParser
    {
        private readonly LALRTableManager _lALRTableManager;
        private readonly Dictionary<int, Dictionary<object, (LALRAction, int)>> _actionTable;
        private readonly string _startSymbol;
        SectionsManager _sectionsManager;


        public LALRParser(LALRTableManager lALRTableManager, SectionsManager sm)
        {
            _lALRTableManager = lALRTableManager;
            _actionTable = lALRTableManager._actionTable;
            _startSymbol = sm._startSymbol;
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
        private List<(LALRAction action, int actionInt)> GetNextAction(int currentState, string currentSymbol)
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
                    List<string> values = new List<string>();
                    if (splitProduction.Length == 1 && splitProduction[0].Equals("ε"))
                    {
                        
                    }
                    else
                    {
                        // Realiza la reducción normal
                        foreach (var symbol in splitProduction)
                        {
                            stateStack.Pop(); // Remueve el estado
                            (string symbol, string value) symbol_Value = symbolStack.Pop(); // Remueve el simbolo
                            string trim = _lALRTableManager.TrimSymbol(symbol);
                            if (trim.Equals(symbol_Value.symbol))
                            {
                                values.Add(symbol_Value.value);
                            }
                            else
                            {
                                return false;
                            }
                            
                        }
                    }
                    // Luego empujar el nuevo estado correspondiente
                    var action1 = GetNextAction(stateStack.Peek(), production.Item1);
                    if (action1[0].action.Equals(LALRAction.Goto))
                    {
                        int newState = action1[0].actionInt; // Determina el nuevo estado
                        stateStack.Push(newState);
                        symbolStack.Push((production.Item1, DoSyntaxActions(production.Item1, production.Item2, values))); // Empuja el símbolo no terminal
                    }
                    else
                    {
                        return false; 
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
                List<(LALRAction action, int actionInt)> actions = GetNextAction(currentState, currentSymbol);
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
        private string DoSyntaxActions(string nonTerminal, string production, List<string> values)
        {
            string result = "";
            List<string> actions;
            var semanticActionsDictionary = _sectionsManager._nonTerminalActions;
            if (semanticActionsDictionary.Keys.Contains(nonTerminal) && semanticActionsDictionary[nonTerminal].Keys.Contains(production))
            {
                actions = _sectionsManager._nonTerminalActions[nonTerminal][production];
                foreach (var action in actions)
                {
                    switch (action)
                    {
                        case "save_program":
                            // 5

                            break;
                        case "save_block":
                            // 2
                            break;
                        case "save_declarations":
                            // 2
                            break;
                        case "keep_value":
                            // 1
                            break;
                        case "save_var_declaration":
                            // 6
                            break;
                        case "save_procedure_declaration":
                            // 8
                            break;
                        case "save_parameter_list":
                            // 4
                            break;
                        case "save_parameter_list_ext":
                            // 5
                            break;
                        case "save_type":
                            // 1
                            break;
                        case "save_compound_statement":
                            // 3
                            break;
                        case "save_statement_list":
                            // 2
                            break;
                        case "save_statement_list_ext":
                            // 3
                            break;
                        case "save_statement":
                            // 1
                            break;
                        case "save_assignment":
                            // 3
                            break;
                        case "save_if_statement":
                            // 5
                            break;
                        case "save_else_statement":
                            // 2
                            break;
                        case "save_while_statement":
                            // 4
                            break;
                        case "save_procedure_call":
                            // 4
                            break;
                        case "save_argument_list":
                            // 2
                            break;
                        case "save_argument_list_ext":
                            // 3
                            break;
                        case "save_io_statement":
                            // 4
                            break;
                        case "save_expression":
                            // 2
                            break;
                        case "save_expression_extension":
                            // 2
                            break;
                        case "save_simple_expression":
                            // 3
                            break;
                        case "save_term":
                            // 3
                            break;
                        case "save_factor":
                            // 3
                            break;
                        case "save_identifier":
                            // 1
                            break;
                        case "save_number":
                            // 1
                            break;
                        case "save_operator":
                            // 1
                            break;
                        case "save_string":
                            // 3
                            break;
                        case "save_boolean":
                            // 1
                            break;
                        default:
                            // Manejo de caso por defecto
                            break;
                    }
                }
            }
            return String.Join(" ", values);
        }
        // save_program
        // save_block
        // save_declarations
        // save_declarations
        // keep_value
        // save_var_declaration
        // keep_value
        // save_procedure_declaration
        // save_parameter_list
        // keep_value
        // save_parameter_list_ext
        // save_type
        // save_type
        // save_type
        // save_type
        // save_compound_statement
        // save_statement_list
        // save_statement_list
        // save_statement_list_ext
        // keep_value
        // save_statement
        // save_statement
        // save_statement
        // save_statement
        // save_statement
        // save_assignment
        // save_if_statement
        // save_else_statement
        // keep_value
        // save_while_statement
        // save_procedure_call
        // save_argument_list
        // keep_value
        // save_argument_list_ext
        // keep_value
        // save_io_statement
        // save_io_statement
        // save_expression
        // save_expression_extension
        // keep_value
        // save_simple_expression
        // keep_value
        // save_term
        // keep_value
        // save_factor
        // save_identifier
        // save_number
        // save_operator
        // save_string
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_operator
        // save_boolean
        // save_boolean
        // save_string

    }
}