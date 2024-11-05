using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ProyectoConsola.Estructuras;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using ProyectoConsola.Enumeraciones;
using static OfficeOpenXml.ExcelErrorValue;

namespace ProyectoConsola.Managers
{
    /// <summary>
    /// Clase que contiene las tablas de estados, transiciones y reducciones
    /// </summary>
    public class LALRTableManager
    {
        private SectionsManager _sectionsManager;
        private NFFTableManager _nffTable;
        public Dictionary<int, Dictionary<object, (LALRAction, int)>> _actionTable; // Tabla de acción

        public LALRTableManager(SectionsManager sectionsManager, NFFTableManager nFFTableManager)
        {
            _sectionsManager = sectionsManager;
            _nffTable = nFFTableManager; 
            _actionTable = new Dictionary<int, Dictionary<object, (LALRAction, int)>>();

            GenerateLALRTable();
        }
        public void ExportToExcel(string filePath)
        {
            // Verifica si la ruta es válida
            if (!Path.IsPathRooted(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                // Si la ruta no es válida, establece la ruta predeterminada
                string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Actions.xlsx");
                filePath = defaultPath;
            }

            // Verifica si el archivo ya existe y lo elimina
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            // Establecer el contexto de licencia
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Crea un nuevo paquete Excel
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Agrega una nueva hoja de trabajo
                var worksheet = excelPackage.Workbook.Worksheets.Add("Action Table");

                // Obtener todos los estados
                var states = new List<int>(_actionTable.Keys);

                // Usar un HashSet para almacenar símbolos únicos
                var uniqueSymbols = new HashSet<string>();
                var symbolList = new List<object>();

                // Recorre el diccionario para obtener todos los símbolos únicos
                foreach (var innerDict in _actionTable.Values)
                {
                    foreach (var innerPair in innerDict)
                    {
                        if (innerPair.Key is string singleSymbol)
                        {
                            uniqueSymbols.Add(TrimSymbol(singleSymbol));
                        }
                        else if (innerPair.Key is List<string> symbolListItem)
                        {
                            foreach (var item in symbolListItem)
                            {
                                uniqueSymbols.Add(TrimSymbol(item));
                            }
                        }
                    }
                }

                // Convertir el HashSet a una lista
                uniqueSymbols.Add("$");
                symbolList.AddRange(uniqueSymbols);

                // Escribir encabezados de columnas (símbolos)
                worksheet.Cells[1, 1].Value = "Estado"; // Encabezado de la primera columna
                int currentColumn = 2; // Comenzar en la columna 2

                foreach (var symbol in symbolList)
                {
                    worksheet.Cells[1, currentColumn].Value = symbol.ToString(); // Inserta el símbolo en su celda
                    currentColumn++;
                }

                // Escribir estados y sus acciones
                for (int row = 0; row < states.Count; row++)
                {
                    int state = states[row];
                    worksheet.Cells[row + 2, 1].Value = state; // Estado en la primera columna

                    // Obtener el diccionario interno para el estado actual
                    if (_actionTable.TryGetValue(state, out var innerDict))
                    {
                        foreach (var innerPair in innerDict)
                        {
                            object symbol = innerPair.Key;
                            (LALRAction action, int index) = innerPair.Value;

                            // Formatear la acción según el tipo
                            string actionFormatted = action switch
                            {
                                LALRAction.Accept => "OK",
                                LALRAction.Shift => $"S{index}",
                                LALRAction.Goto => $"G{index}",
                                LALRAction.Reduce => $"R{index}",
                                _ => string.Empty // Manejo de acciones no definidas
                            };

                            // Si el símbolo es un string, inserta en su celda correspondiente
                            if(symbol is HashSet<string> symbols)
                            {
                                foreach(var symbol_i in symbols)
                                {
                                    int colIndex = symbolList.IndexOf(symbol_i) + 2; // +2 porque la primera columna es para estados
                                    if (colIndex > 1) // Asegurarse de que el símbolo se encontró
                                    {
                                        if (worksheet.Cells[row + 2, colIndex].GetValue<string>() != null)
                                        {
                                            if(!worksheet.Cells[row + 2, colIndex].GetValue<string>().Equals(actionFormatted))
                                                worksheet.Cells[row + 2, colIndex].Value = String.Concat(worksheet.Cells[row + 2, colIndex].GetValue<string>(), ' ',',',actionFormatted); // Almacena la acción formateada
                                        }
                                        else
                                            worksheet.Cells[row + 2, colIndex].Value = actionFormatted;
                                    }
                                }
                                
                            }
                            else
                            {
                                int colIndex = symbolList.IndexOf(symbol) + 2; // +2 porque la primera columna es para estados
                                if (colIndex > 1) // Asegurarse de que el símbolo se encontró
                                {
                                    if (worksheet.Cells[row + 2, colIndex].GetValue<string>() != null)
                                        worksheet.Cells[row + 2, colIndex].Value = String.Concat(worksheet.Cells[row + 2, colIndex].GetValue<string>(), ' ', ',', actionFormatted); // Almacena la acción formateada
                                    else
                                        worksheet.Cells[row + 2, colIndex].Value = actionFormatted;
                                }
                            }
                            
                        }
                    }
                }

                // Ajusta el ancho de las columnas
                worksheet.Cells.AutoFitColumns();

                // Guarda el archivo
                FileInfo excelFile = new FileInfo(filePath);
                excelPackage.SaveAs(excelFile);
            }
        }
        public void ExportStatesToExcel(List<State> states, string filePath)
        {
            // Asegúrate de que EPPlus pueda trabajar con archivos Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Verifica si la ruta es válida
            if (!Path.IsPathRooted(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                // Si la ruta no es válida, establece la ruta predeterminada
                string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Estados.xlsx");
                filePath = defaultPath;
            }

            // Verifica si el archivo ya existe y lo elimina
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Estados");

                // Encabezados de las columnas
                worksheet.Cells[1, 1].Value = "Estado";
                worksheet.Cells[1, 2].Value = "Identificador";
                worksheet.Cells[1, 3].Value = "Producción (Right)";
                worksheet.Cells[1, 4].Value = "Index";
                worksheet.Cells[1, 5].Value = "Simbolo";
                worksheet.Cells[1, 6].Value = "Lookahead";
                worksheet.Cells[1, 7].Value = "Action";
                worksheet.Cells[1, 8].Value = "#";

                // Rellenar los datos
                int row = 2; // Comenzar en la segunda fila
                foreach (var state in states)
                {
                    foreach (var item in state.Items)
                    {
                        worksheet.Cells[row, 1].Value = state.Index; // Estado
                        worksheet.Cells[row, 2].Value = item.ItemProduction.Left; // Producción (Left)
                        worksheet.Cells[row, 3].Value = string.Join(" ", item.ItemProduction.Right); // Producción (Right)
                        worksheet.Cells[row, 4].Value = item.ItemProduction.CurrentSymbolIndex; // CurrentSymbolIndex
                        worksheet.Cells[row, 5].Value = item.ItemProduction.CurrentSymbol(); // CurrentSymbol
                        worksheet.Cells[row, 6].Value = string.Join(", ", item.Lookahead); // Lookahead
                        worksheet.Cells[row, 7].Value = item.ItemAction.ToString(); // ItemAction
                        worksheet.Cells[row, 8].Value = item.ActionInt; // ActionInt
                        row++;
                    }
                }

                // Ajustar el ancho de las columnas
                worksheet.Cells.AutoFitColumns();

                // Guardar el archivo
                FileInfo excelFile = new FileInfo(filePath);
                package.SaveAs(excelFile);
            }
        }
        private void GenerateLALRTable()
        {
            // Paso 1: Construir el conjunto de estados
            List<State> states = ConstructStates();
            // Paso 2: Agregar los reduce y aceptacion
            foreach (var state in states)
            {
                foreach (var item in state.Items)
                {
                    if (item.ItemProduction.IsAtEnd())
                    {
                        if(VerifyAcceptance(item, new Item(["$"], _sectionsManager._startSymbol, _sectionsManager._nonTerminals[_sectionsManager._startSymbol][0])))
                        { 
                            item.SetAction(LALRAction.Accept); 
                        }
                        else
                        {
                            item.SetAction(LALRAction.Reduce);
                        }
                        item.SetActionInt(GetIndexOfReduceProduction(item.ItemProduction));

                    }
                    else
                    {
                        string currentSymbol = TrimSymbol(item.ItemProduction.CurrentSymbol());
                        if (_sectionsManager.IsNonTerminal(currentSymbol))
                        {
                            item.SetAction(LALRAction.Goto);
                        }
                        else
                        {
                            item.SetAction(LALRAction.Shift);
                        }
                    }
                }
            }

            // Paso 3: Llenar la tabla de action
            foreach (var state in states)
            {
                foreach (var item in state.Items)
                {
                    switch (item.ItemAction)
                    {
                        case LALRAction.Accept:
                            AddAction(state.Index, item.Lookahead, item.ItemAction, item.ActionInt);
                            break;
                        case LALRAction.Reduce:
                            AddAction(state.Index, item.Lookahead, item.ItemAction, item.ActionInt);
                            break;
                        case LALRAction.Shift:
                            AddAction(state.Index, item.ItemProduction.CurrentSymbol(), item.ItemAction, item.ActionInt);
                            break;
                        case LALRAction.Goto:
                            AddAction(state.Index, item.ItemProduction.CurrentSymbol(), item.ItemAction, item.ActionInt);
                            break;
                    }
                }
            }
            ExportStatesToExcel(states, "");
            ExportToExcel("");
        }
        private void AddAction(int state, object value, LALRAction action, int index)
        {
            if (!_actionTable.ContainsKey(state))
            {
                _actionTable[state] = new Dictionary<object, (LALRAction, int)>();
            }
            if(value is List<string> sValues)
            {
                foreach(var svalue in sValues)
                {
                    string trim = TrimSymbol(svalue);
                    _actionTable[state][trim] = (action, index);
                }
            }
            else if(value is HashSet<string> hValues)
            {
                foreach (var svalue in hValues)
                {
                    string trim = TrimSymbol(svalue);
                    _actionTable[state][trim] = (action, index);
                }
            }
            else
            {
                string trim = TrimSymbol(value.ToString());
                _actionTable[state][trim] = (action, index);
            }
            
        }
        private List<State> ConstructStates()
        {
            List<State> states = new List<State>();
            Dictionary<string, State> visitedStates = new Dictionary<string, State>();

            // Paso 1: Crear el estado inicial
            List<Item> initialItems  = Closure(new List<Item> { new Item(["$"],_sectionsManager._startSymbol, _sectionsManager._nonTerminals[_sectionsManager._startSymbol][0]) });
            State initialState = new State(0, initialItems);
            // Agregar el estado inicial a la lista de estados
            states.Add(initialState);
            visitedStates.Add(GetStateKey(initialState), initialState);
            //

            // Paso 2: Generar estados
            Queue<State> stateQueue = new Queue<State>();
            stateQueue.Enqueue(initialState);
            while (stateQueue.Count > 0)
            {
                State currentState = stateQueue.Dequeue();
                foreach(var symbol in GetSymbols(currentState.Items))
                {
                    State newState = Goto(currentState, symbol, states.Count);
                    if(newState.Items.Count > 0)
                    {
                        string newStateKey = GetStateKey(newState);
                        if (!visitedStates.ContainsKey(newStateKey))
                        {
                            states.Add(newState);
                            visitedStates.Add(newStateKey, newState);
                            stateQueue.Enqueue(newState);
                        }
                    }
                }

            }

            // Paso 3: Combinar estados para LALR
            //return states;
            return CombineStates(states);
        }
        private string GetStateKey(State state)
        {
            List<string> keys = new List<string>();
            foreach(var item in state.Items)
            {
                Production production = item.ItemProduction;
                string right = string.Join(" ", production.Right.Take(production.CurrentSymbolIndex)),
                    left = string.Join(" ", production.Right.Skip(production.CurrentSymbolIndex));
            // Crear una representación del ítem que incluya la posición del punto
            string itemRepresentation = $"{production.Left} -> {right} . {left}";

                // Agregar la representación a la lista de claves
                keys.Add(itemRepresentation);
            }
            keys.Sort();
            return String.Join(" | ", keys);
        }
        private List<Item> Closure(List<Item> items)
        {
            HashSet<Item> closureSet = new HashSet<Item>(items);
            Dictionary<string, List<string>> nonTerminals = _sectionsManager._nonTerminals;
            bool added;
            do 
            {
                added = false;
                foreach(var item in closureSet.ToList())
                {
                    if (!item.ItemProduction.IsAtEnd())
                    {
                        string currentSymbol = TrimSymbol(item.ItemProduction.CurrentSymbol());
                        if(nonTerminals.TryGetValue(currentSymbol, out var productions))
                        {
                            // Determinar lookahead 
                            HashSet<string> lookahead = CalculateLookahead(item);
                            foreach(var rightSideProduction in productions)
                            {
                                Item newItem = new Item(lookahead, currentSymbol, rightSideProduction);
                                if (!SetContains(closureSet, newItem))
                                {
                                    closureSet.Add(newItem);
                                    added = true;
                                }
                            }
                            
                        }                        
                    }
                }
            }
            while (added);
            
            return new List<Item>(closureSet);
        }
        private bool SetContains(HashSet<Item> sets, Item itemCandidate)
        {
            foreach(var item in sets)
            {
                if (itemCandidate.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        private HashSet<string> CalculateLookahead(Item item)
        {
            HashSet<string> lookahead = item.Lookahead;
            // Si el símbolo actual no está al final
            if (!item.ItemProduction.IsAtEnd())
            {
                int nextIndex = item.ItemProduction.CurrentSymbolIndex + 1;
                // Si hay un símbolo siguiente
                if (nextIndex < item.ItemProduction.Right.Count)
                {
                    lookahead = new HashSet<string>();
                    string nextSymbol = TrimSymbol(item.ItemProduction.Right[nextIndex]);
                    // Si el siguiente símbolo es un no terminal
                    if (_sectionsManager.IsNonTerminal(nextSymbol))
                    {
                        lookahead.UnionWith(GetLookaheadForNonTerminal(nextSymbol, item));
                    }
                    else
                    {
                        // Si es un terminal, simplemente lo agregas
                        lookahead.Add(nextSymbol);
                    }
                }
            }

            return lookahead;
        }
        private HashSet<string> GetLookaheadForNonTerminal(string symbol, Item item)
        {
            HashSet<string> la = new HashSet<string>();
            la.UnionWith(_nffTable._first[symbol]);
            if (_nffTable._nullable[symbol])
            {
                int i = item.ItemProduction.CurrentSymbolIndex + 2,
                    count = item.ItemProduction.RightCount();
                for (; i < count; i++)
                {
                    string nextSymbol = TrimSymbol(item.ItemProduction.Right[i]);
                    if (_sectionsManager.IsNonTerminal(nextSymbol))
                    {
                        la.UnionWith(_nffTable._first[nextSymbol]);
                        if (_nffTable._nullable[nextSymbol])
                        {
                            if (i == count - 1)
                                la.UnionWith(item.Lookahead);
                            continue;
                        }
                        else
                            break;
                    }
                    else
                    {
                        la.Add(nextSymbol);
                        break;
                    }
                }
                if (i == item.ItemProduction.CurrentSymbolIndex + 2)
                {
                    la.UnionWith(item.Lookahead);
                }
            }
            return la;
        }
        private State Goto(State state, string symbol, int stateCount)
        {
            List<Item> newItems = new List<Item>();
            foreach (var item in state.Items)
            {
                if (!item.ItemProduction.IsAtEnd())
                {
                    string currentSymbol = TrimSymbol(item.ItemProduction.CurrentSymbol());
                    if(currentSymbol.Equals(symbol))
                    {
                        Item item2 = item.Clone();
                        item2.ItemProduction.IncreaseIndex();
                        newItems.Add(item2);
                        if (_sectionsManager.IsNonTerminal(symbol))
                        {
                            item.SetAction(LALRAction.Goto);
                        }
                        else
                        {
                            item.SetAction(LALRAction.Shift);
                        }
                        item.SetActionInt(stateCount);
                    }

                }
            }
            return new State(stateCount, Closure(newItems));
        }
        private bool VerifyAcceptance(Item item, Item initialItem)
        {
            string nonterminal = TrimSymbol(initialItem.ItemProduction.Right[0]),
                production = _sectionsManager._nonTerminals[nonterminal][0];
            Production accept = new Production(nonterminal, production, production.Split(' ').Length);
            return item.ItemProduction.Equals(accept);
        }
        private int GetIndexOfReduceProduction(Production production)
        {
            Tuple<string, string> s = Tuple.Create(production.Left, String.Join(" ", production.Right));
            return _sectionsManager._orderedNonTerminals.FindIndex(t => t.Item1.Equals(s.Item1) && t.Item2.Equals(s.Item2));
        }
        private HashSet<string> GetSymbols(List<Item> items)
        {
            HashSet<string> symbols = new HashSet<string>();

            foreach (var item in items)
            {
                string ?currentSymbol = TrimSymbol(item.ItemProduction.CurrentSymbol());
                if(currentSymbol != null)
                    symbols.Add(currentSymbol);
            }

            return symbols;
        }
        private List<State> CombineStates(List<State> states)
        {
            Dictionary<string, State> combinedStatesMap = new Dictionary<string, State>();
            List<State> combinedStates = new List<State>();

            foreach (var state in states)
            {
                // Generar una clave única para el estado basado en sus ítems
                string stateKey = GetStateKey(state);

                // Si el estado ya ha sido combinado, continuar
                if (combinedStatesMap.ContainsKey(stateKey))
                {
                    continue; // Este estado ya ha sido procesado
                }

                // Agregar el estado a la lista de estados combinados
                combinedStates.Add(state);
                combinedStatesMap[stateKey] = state;

                // Combinar estados equivalentes
                foreach (var otherState in states)
                {
                    if (!state.Equals(otherState) && GetStateKey(state).Equals(GetStateKey(otherState)))
                    {
                        // Si los estados son equivalentes, combinar sus ítems
                        foreach (var item in otherState.Items)
                        {
                            if (!combinedStatesMap.ContainsKey(GetStateKey(new State (otherState.Index, [item]))))
                            {
                                foreach(var item1 in otherState.Items)
                                {
                                    if (item1.EqualsWithoutLookahead(item))
                                    {

                                    }
                                    else
                                        state.Items.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            return combinedStates;
        }
        public string TrimSymbol(string currentSymbol)
        {
            if(currentSymbol != null)
            {
                //Logica de preparacion para adecuar el simbolo actual a las operaciones
                if (currentSymbol.Contains("\'") &&
                    (currentSymbol.Contains("<") || currentSymbol.Contains(">") || currentSymbol.Contains("(")))
                {
                    currentSymbol = currentSymbol.Trim().Trim('\'');
                }
                else
                {
                    currentSymbol = currentSymbol.Trim().Trim('\'').Trim('(').Trim('<').Trim('>');
                }
                return currentSymbol;
            }
            return null;
        }
    }

    
}
