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

namespace ProyectoConsola.Managers
{
/// <summary>
/// Clase que contiene las tablas de estados, transiciones y reducciones
/// </summary>
    public class LALRTableManager
    {
        Dictionary<int, List<LALRStateProduction>> _states;
        private SectionsManager _sectionsManager;
        private NFFTableManager _nffTable;
        //public Dictionary<int, List<LALRStateProduction>> _states { get; set; }
        public List<Tuple<int, string, int>> _gotos { get; set; }
        public List<Tuple<int, string, int>> _shifts { get; set; }
        public List<Tuple<int, List<string>, int>> _reductions { get; set; }
        public Tuple<int, List<string>, int> _acceptanceReduction { get; set; }

        public LALRTableManager(SectionsManager sections, NFFTableManager nffTable)
        {
            _sectionsManager = sections;
            _nffTable = nffTable;
            //_states = new Dictionary<int, List<LALRStateProduction>>();
            _gotos = new List<Tuple<int, string, int>>();
            _shifts = new List<Tuple<int, string, int>>();
            _reductions = new List<Tuple<int, List<string>, int>>();
            GenerateLALRTable();

        }

        public void GenerateLALRTable()
        {
            // Paso 1: Generar la tabla de parser
            try
            {
                _states = GenerateStates();
                if(_acceptanceReduction == null)
                {
                    throw new Exception("La gramatiica no tiene reduccion de aceptacion.");
                }

            }
            catch (Exception e)
            {
                string mensaje = e.Message;
                throw;
            }
        }
        public void ExportStatesToExcel(string rutaArchivo)
        {
            // Establecer el contexto de licencia
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Ruta de salida de la compilación
            var rutaSalidaCompilacion = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Validar la ruta del archivo
            if (string.IsNullOrWhiteSpace(rutaArchivo) || !Directory.Exists(Path.GetDirectoryName(rutaArchivo)))
            {
                // Si la ruta es inválida, usar la ruta de salida de la compilación
                rutaArchivo = Path.Combine(rutaSalidaCompilacion, "estados.xlsx");
            }

            // Asegurarse de que el directorio existe
            var directorio = Path.GetDirectoryName(rutaArchivo);
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            using (ExcelPackage paquete = new ExcelPackage())
            {
                // Crea una nueva hoja de trabajo
                ExcelWorksheet worksheet = paquete.Workbook.Worksheets.Add("Estados");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Estado actual";
                worksheet.Cells[2, 1].Value = "Identificador";
                worksheet.Cells[2, 2].Value = "Producción";
                worksheet.Cells[2, 3].Value = "Simbolo/Indice actual";
                worksheet.Cells[2, 4].Value = "Lookahead";

                int row = 3; // Comenzar desde la tercera fila para los datos

                // Recorrer el diccionario de estados
                foreach (var state in _states)
                {
                    int stateNumber = state.Key;
                    var productions = state.Value;

                    // Escribir el número de estado
                    worksheet.Cells[row, 1].Value = $"Estado: {stateNumber}";
                    row++;

                    // Escribir las producciones
                    foreach (var production in productions)
                    {
                        worksheet.Cells[row, 1].Value = production._identifier; // Identificador
                        worksheet.Cells[row, 2].Value = production._production; // Producción
                        if(production._actualIndex < production._production.Split(' ').Length)
                            worksheet.Cells[row, 3].Value = $"{production._production.Split(' ')[production._actualIndex]} / {production._actualIndex}"; // Índice actual
                        else
                            worksheet.Cells[row, 3].Value = $"E.O.P. / {production._actualIndex}";
                        worksheet.Cells[row, 4].Value = string.Join(", ", production._lookahead); // Lookahead
                        row++;
                    }

                    // Espaciado entre estados
                    row++;
                }

                // Guardar el archivo
                var archivoInfo = new FileInfo(rutaArchivo);
                paquete.SaveAs(archivoInfo);
            }
        }
        public void ExportActionsToExcel(string rutaArchivo)
        {
            // Establecer el contexto de licencia
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Ruta de salida de la compilación
            var rutaSalidaCompilacion = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Validar la ruta del archivo
            if (string.IsNullOrWhiteSpace(rutaArchivo) || !Directory.Exists(Path.GetDirectoryName(rutaArchivo)))
            {
                // Si la ruta es inválida, usar la ruta de salida de la compilación
                rutaArchivo = Path.Combine(rutaSalidaCompilacion, "actions.xlsx");
            }

            // Asegurarse de que el directorio existe
            var directorio = Path.GetDirectoryName(rutaArchivo);
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            // Paso 1: Obtener todos los estados y símbolos
            var estados = new HashSet<int>();
            var simbolos = new HashSet<string>();

            // Agregar estados de las listas
            foreach (var gotoTuple in _gotos)
            {
                estados.Add(gotoTuple.Item1);
                simbolos.Add(gotoTuple.Item2);
            }
            foreach (var shiftTuple in _shifts)
            {
                estados.Add(shiftTuple.Item1);
                simbolos.Add(shiftTuple.Item2);
            }
            foreach (var reductionTuple in _reductions)
            {
                estados.Add(reductionTuple.Item1);
                foreach (var simbolo in reductionTuple.Item2)
                {
                    simbolos.Add(simbolo);
                }
            }

            // Convertir a listas y ordenar
            var listaEstados = new List<int>(estados);
            listaEstados.Sort();
            var listaSimbolos = new List<string>(simbolos);
            listaSimbolos.Sort();

            // Paso 2: Crear el archivo Excel
            using (var paquete = new ExcelPackage())
            {
                var hoja = paquete.Workbook.Worksheets.Add("Tabla Parser");

                // Escribir la primera fila con los símbolos
                hoja.Cells[1, 1].Value = "Estado";
                for (int i = 0; i < listaSimbolos.Count; i++)
                {
                    hoja.Cells[1, i + 2].Value = listaSimbolos[i];
                }

                // Llenar la tabla con los valores
                for (int i = 0; i < listaEstados.Count; i++)
                {
                    hoja.Cells[i + 2, 1].Value = listaEstados[i]; // Estado
                    for (int j = 0; j < listaSimbolos.Count; j++)
                    {
                        string valor = ""; // Inicializamos como vacío
                        (bool tieneReduce, int reducePorLa) search = SearchShift(i, listaSimbolos[j]);
                        if (search.Item1)
                        {
                            valor = "S" + search.Item2.ToString();
                        }
                        else
                        {
                            
                            search = SearchReduction(i, listaSimbolos[j]);
                            if (search.Item1)
                            {
                                if(_acceptanceReduction.Item1.Equals(i) && _acceptanceReduction.Item2.Contains(listaSimbolos[j]))
                                {
                                    valor = "OK";
                                }
                                else
                                    valor = "R" + search.Item2.ToString();
                            }
                            else
                            {
                                foreach(var gotoOp in _gotos)
                                {
                                    if(gotoOp.Item1.Equals(i) && gotoOp.Item2.Equals(listaSimbolos[j]))
                                    {
                                        valor = "G" + gotoOp.Item3.ToString();
                                    }
                                }
                            }
                        }
                        hoja.Cells[i + 2, j + 2].Value = valor;
                    }
                }

                // Guardar el archivo
                var archivoInfo = new FileInfo(rutaArchivo);
                paquete.SaveAs(archivoInfo);
            }
        }
        
        public bool VerifyInputString(string param_input)
        {
            try
            {
                bool result = false;
                string[] splitInput = param_input.Trim().Split(' ');
                Stack<InputStackItem> itemStack = new();
                itemStack.Push(new InputStackItem(0, 0));
                string currentSymbol;
                // Mientras la cadena de entrada no haya sido totalmente analizada ...

                for (int i = 0; i < splitInput.Length; i++)
                {
                    InputStackItem currentItem = itemStack.Peek();
                    // Si el primer elemento del stack es un estado se puede realizar shift o reduction
                    if (currentItem._type.Equals(0))
                    {
                        currentSymbol = TrimSymbol(splitInput[i]);
                        InputStackItem itemizedSymbol;
                        if (_sectionsManager.IsToken(currentSymbol))
                        {
                            string idenfier = _sectionsManager.GetTokenIdentifier(currentSymbol);
                            itemizedSymbol = new InputStackItem(idenfier, 1, currentSymbol);
                        }
                        else
                        {
                            itemizedSymbol = new InputStackItem(currentSymbol, 1);
                        }

                        //validacion si tiene shift
                        (bool tieneShif, int shifPorLa) search = SearchShift(currentItem.GetIntValueForSymbol(), itemizedSymbol.GetStringValueForSymbol());
                        if (search.tieneShif)
                        {
                            Console.WriteLine("SHIFT " + search.shifPorLa);
                            itemStack.Push(itemizedSymbol);
                            itemStack.Push(new InputStackItem(search.shifPorLa, 0));
                        }

                        //validacion si tiene reduce
                        (bool tieneReduce, int reducePorLa) search2 = SearchReduction(currentItem.GetIntValueForSymbol(), itemizedSymbol.GetStringValueForSymbol());
                        if (search2.tieneReduce)
                        {
                            // Si se debe realizar una reduccion tambien hay que hacer los actions de la produccion
                            Tuple<string, string> identifierProduction = _sectionsManager._orderedNonTerminals[search2.reducePorLa];
                            Console.WriteLine("REDUCTION " + search2.reducePorLa + ", " + identifierProduction.Item1 + " = " + identifierProduction.Item2);
                            
                            string[] splitProduction = identifierProduction.Item2.Split(' ');
                            object[] valuesForActions = new string[splitProduction.Length];
                            
                            for (int j = splitProduction.Length - 1; j > -1; j--)
                            {
                                string trimSymbol = TrimSymbol(splitProduction[j]);
                                itemStack.Pop(); // Se saca el item con el ESTADO del stack
                                if (trimSymbol.Equals(itemStack.Peek().GetStringValueForSymbol()))
                                {
                                    InputStackItem itemSymbol = itemStack.Pop(); // Se saca el item con el SIMBOLO del stack
                                    if(itemSymbol._value != null)
                                    {
                                        valuesForActions[j] = itemSymbol._value;
                                    } 
                                    else if (!_sectionsManager.IsNonTerminal(trimSymbol))
                                    {
                                        valuesForActions[j] = trimSymbol;
                                    }
                                }
                                else
                                {
                                    string error = "Se esperaba '" + itemStack.Peek().GetStringValueForSymbol() + "' , pero se entontro '" + trimSymbol + "'";
                                    throw new Exception(error);
                                }
                            }
                            
                            InputStackItem nonTerminalItem = new InputStackItem(_sectionsManager._orderedNonTerminals[search2.reducePorLa].Item1, 1);
                            if (_sectionsManager._nonTerminalActions.Keys.Contains(identifierProduction.Item1))
                            {
                                if (_sectionsManager._nonTerminalActions[identifierProduction.Item1].Keys.Contains(identifierProduction.Item2))
                                {
                                    List<string> actions = _sectionsManager._nonTerminalActions[identifierProduction.Item1][identifierProduction.Item2];
                                    nonTerminalItem._value = DoActions(actions, splitProduction, valuesForActions);
                                }

                            }
                            else if (currentItem._value != null)
                            {
                                nonTerminalItem._value = currentItem._value;
                            }
                            itemStack.Push(nonTerminalItem);
                        }
                        if (!search.tieneShif && !search2.tieneReduce)
                        {
                            if(itemizedSymbol._value == null)
                                throw new Exception($"No existe operacion para: {itemizedSymbol.GetStringValueForSymbol()}\nPrimer objeto en el stack: {itemStack.Peek()._symbol}, tipo: {itemStack.Peek()._type}");
                            else
                                throw new Exception($"No existe operacion para: {itemizedSymbol.GetStringValueForSymbol()}, valor: {itemizedSymbol.GetStringValueForValue()}\nPrimer objeto en el stack: {itemStack.Peek()._symbol}, tipo: {itemStack.Peek()._type}");

                        }

                    }// Si no hay estado al inicio del stack se debe realiar un goto
                    else
                    {
                        InputStackItem nextItem = itemStack.ElementAt(1);
                        // Determinar el estado a insertar en la pila
                        foreach(var gotoOperation in _gotos)
                        {
                            if(gotoOperation.Item1.Equals(nextItem.GetIntValueForSymbol()) && gotoOperation.Item2.Equals(currentItem.GetStringValueForSymbol()))
                            {
                                Console.WriteLine("GOTO " + gotoOperation.Item3);
                                itemStack.Push(new InputStackItem(gotoOperation.Item3, 0));
                            }
                        }
                    }
                }
                //
                currentSymbol = "$";

                return result;
            }
            catch (System.Exception e)
            {
                string mensaje = e.Message;
                Console.WriteLine("Error al realizar la validacion: ");
                Console.WriteLine(mensaje);
                return false;
            }
        }

        //private Tuple<bool, int> SearchShift(int currentState, string consumedSymbol)
        private (bool tieneShif, int shifPorLa) SearchShift(int currentState, string consumedSymbol)
        {
            
            foreach(var shiftOperation in _shifts)
            {
                if(shiftOperation.Item1.Equals(currentState) && shiftOperation.Item2.Equals(consumedSymbol))
                {
                    //return new Tuple<bool, int>(true, shiftOperation.Item3);
                    return new (true, shiftOperation.Item3);
                }
            }
            return new (false, -1);
        }

        //private Tuple<bool, int> SearchReduction(int currentState, string consumedSymbol)
        private (bool tieneReduce, int reducePorLa) SearchReduction(int currentState, string consumedSymbol)
        {
            foreach(var reductionOperation in _reductions)
            {
                if (reductionOperation.Item1.Equals(currentState) && reductionOperation.Item2.Contains(consumedSymbol))
                {
                    //return new Tuple<bool, int>(true, reductionOperation.Item3);
                    return (true, reductionOperation.Item3);
                }
            }
            // return new Tuple<bool, int>(false, -1);
            return new (false, -1);
        }
        private object DoActions(List<string> actions, string[] production, object[]valuesOfProduction)
        {
            // Realizar las actions
            
            foreach (var action in actions)
            {
                switch(action)
                {
                    
                    case "save_type":
                        return valuesOfProduction[0];
                    case "save_string":
                        return valuesOfProduction[1];
                    case "save_bool":
                        return valuesOfProduction[0];
                    case "save_operator":
                        return valuesOfProduction[0];
                    case "save_identifier":
                        return valuesOfProduction[0];
                }
            }
            return null;
        }
        private string TrimSymbol(string symbol)
        {
            string currentSymbol = symbol;
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

        private Dictionary<int, List<LALRStateProduction>> GenerateStates()
        {
            // Diccionario a llenar
            Dictionary<int, List<LALRStateProduction>> states = [];

            try
            {

                // Generar el primer estado, con la produccion inicial
                LALRStateProduction firstStateProduction = new LALRStateProduction(0, _sectionsManager._startSymbol, _sectionsManager._nonTerminals[_sectionsManager._startSymbol][0], new List<string> { "$" });
                states.Add(0, new List<LALRStateProduction>());

                // Lógica para generar los estados restantes
                // Lista para las producciones pendientes de procesar
                List<LALRStateProduction> pendingStateProductionsList = [firstStateProduction];
                pendingStateProductionsList.AddRange(GenerateStateProductionsForNonTerminal(TrimSymbol(firstStateProduction.GetCurrentSybol()), firstStateProduction));
                states[0].AddRange(pendingStateProductionsList);
                //Se inicia el contador de estados
                int actualStateIndex = 0;
                int countOfStateProductionsProcessedInActualStateIndex = 0;
                while (pendingStateProductionsList.Count > 0) //Mientras haya producciones por procesar en la lista
                {

                    LALRStateProduction currentProduction = pendingStateProductionsList.ElementAt(0); //Se copia el valor de la produccion al inicio de la lista

                    if (currentProduction._actualIndex < currentProduction.GetProductionLenght()) //Si no se ha procesado completamente la produccion ...
                    {
                        //Se determina el simbolo actual
                        string currentSymbol = TrimSymbol(currentProduction.GetCurrentSybol()); 
                        //Procesar producciones para un nuevo estado
                        Tuple<List<LALRStateProduction>, List<LALRStateProduction>> newState_StateProductions = GenerateNewStateProductions(currentProduction, states[actualStateIndex]);
                        // Eliminar las producciones recien procesadas y extenderlas si es necesario
                        foreach(var production in newState_StateProductions.Item2)
                        {
                            pendingStateProductionsList.Remove(production);
                            countOfStateProductionsProcessedInActualStateIndex++;
                        }
                        List<LALRStateProduction> aux = new List<LALRStateProduction>();
                        foreach (var production in newState_StateProductions.Item1)
                        {
                            string trimProductionSymbol = TrimSymbol(production.GetCurrentSybol());
                            if (!trimProductionSymbol.Equals("") && _sectionsManager.IsNonTerminal(trimProductionSymbol))
                            {
                                aux = GenerateStateProductionsForNonTerminal(trimProductionSymbol, production);
                            }
                        }
                        newState_StateProductions.Item1.AddRange(aux);
                        // Asegurar que el estado sea unico
                        Tuple<bool, int> unique = EnsureUniquenessOfStates(newState_StateProductions.Item1, states);
                        //Si ...
                        if (unique.Item1)
                        {// ... es unico se crea un nuevo estado
                            states.Add(unique.Item2, newState_StateProductions.Item1);
                            pendingStateProductionsList.AddRange(newState_StateProductions.Item1);
                        }
                        GenerateGotosShifts(actualStateIndex, currentSymbol, unique.Item2);

                    }
                    else
                    {// ... Si el simbolo esta al final de la produccion

                        countOfStateProductionsProcessedInActualStateIndex++;
                        pendingStateProductionsList.RemoveAt(0);
                        
                        //Reduction
                        int productionIndex = _sectionsManager._orderedNonTerminals.IndexOf(new Tuple<string, string>(currentProduction._identifier, currentProduction._production));
                        if (productionIndex >= 0)
                        {
                            GenerateReductionForState(actualStateIndex, currentProduction._lookahead, productionIndex);
                        }
                        if(_acceptanceReduction == null && productionIndex == 0)
                        {
                            if(currentProduction._actualIndex >= currentProduction._production.Split(' ').Length)
                            {
                                _acceptanceReduction = new Tuple<int, List<string>, int>(actualStateIndex, currentProduction._lookahead, productionIndex);
                            }
                        }

                    }
                    // Verificar si aun quedan producciones del estado actual dentro de la lista de producciones pendientes
                    if (countOfStateProductionsProcessedInActualStateIndex >= states[actualStateIndex].Count) 
                    { 
                        actualStateIndex++;
                        countOfStateProductionsProcessedInActualStateIndex = 0;
                    }
                }
                return states;
            }
            catch (Exception e)
            {
                string mensaje = e.Message;
                throw;
            }
            
        }

        /// <summary>
        /// Genera un estado nuevo con las producciones que consumen el mismo simbolo
        /// </summary>
        /// <param name="firstStateProduction">Produccion principal</param>
        /// <param name="currentStateProductions">Listado de producciones del estado anterior</param>
        /// <returns>Lista de producciones correspondientes al nuevo estado</returns>
        private Tuple<List<LALRStateProduction>, List<LALRStateProduction>> GenerateNewStateProductions(LALRStateProduction firstStateProduction, List<LALRStateProduction> currentStateProductions)
        {
            List<LALRStateProduction> productionsForNewState = new List<LALRStateProduction>();
            List<LALRStateProduction> processedSateProductions = new List<LALRStateProduction>();
            //Se determina el simbolo actual de la produccion inicial
            string consumedSymbol = TrimSymbol(firstStateProduction._production.Split(' ')[firstStateProduction._actualIndex]);

            //Del estado anterior se seleccionan las producciones que ...
            foreach(var production in currentStateProductions)
            {
                string[] productionSplit = production._production.Split(' ');
                if (production._actualIndex < productionSplit.Length)
                {
                    string actualSymbol = TrimSymbol(productionSplit[production._actualIndex]);
                    // ... consumal el mismo simbolo que el simbolo consumido por la produccion inicial
                    if(consumedSymbol.Equals(actualSymbol) && !productionsForNewState.Contains(production))
                    {
                        processedSateProductions.Add(production);
                        LALRStateProduction tempProduction = production.Clone();
                        tempProduction._actualIndex++; // Se aumenta el indice del simbolo actual de la produccion
                        productionsForNewState.Add(tempProduction);
                    }
                }
            }

            return new Tuple<List<LALRStateProduction>, List<LALRStateProduction>>(productionsForNewState, processedSateProductions);
        }
        /// <summary>
         /// Asegura que el estado prospecto no sea igual a uno ya existente
         /// </summary>
         /// <param name="prospectProductions">Listado de producciones que conforman el estado prospecto</param>
         /// <param name="states">Diccionario de estados actuales</param>
         /// <returns>True: Si el estado prospecto no es unico False: Si el estado es igual a alguno ya existente</returns>
        private Tuple<bool, int> EnsureUniquenessOfStates(List<LALRStateProduction> prospectProductions, Dictionary<int, List<LALRStateProduction>> states)
        {
            foreach (var stateProductionKey in states.Keys)
            {
                List<LALRStateProduction> stateProductions = states[stateProductionKey];
                if (prospectProductions.Count == stateProductions.Count)
                {
                    bool allMatch = true;
                    foreach (var prospectProduction in prospectProductions)
                    {
                        bool found = false;
                        foreach (var stateProduction in stateProductions)
                        {
                            if (prospectProduction.EqualsStateProduction(stateProduction))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            allMatch = false;
                            break;
                        }
                    }

                    if (allMatch)
                    {
                        return new Tuple<bool, int>(false, stateProductionKey);
                    }
                }
            }
            return new Tuple<bool, int>(true, states.Count);

        }
        /// <summary>
        /// Determina todas las produccones relacionadas con el estado actual
        /// </summary>
        /// <param name="nonTerminal">Simbolo no terminal, identificador para las nuevas producciones</param>
        /// <param name="contextStateProduction">Produccion actualmente analizada </param>
        /// <returns>Lista de producciones estado derivadas de la actual</returns>
        private List<LALRStateProduction> GenerateStateProductionsForNonTerminal(string nonTerminal, LALRStateProduction contextStateProduction)
        {
            // Lista de produccionees estado para el simbolo no terminal
            List<LALRStateProduction> productionsOfNonTerminal = new List<LALRStateProduction>();
            Dictionary<string, HashSet<List<string>>>? recursiveLookaheads = null;
            // Lisado de producciones del simbolo no terminal
            List<string> crudeProductions = _sectionsManager._nonTerminals[nonTerminal];
            
            // Por cada produccion del simbolo no terminal ...
            foreach(var crudeProduction in crudeProductions)
            {
                // Se crea la produccion estado a partir de la produccion
                LALRStateProduction newStateProduction = new LALRStateProduction(0, nonTerminal, crudeProduction, contextStateProduction._lookahead);
                string[] splitCrudeProduction = crudeProduction.Split(' ');
                //Se determina el simbolo actual de la nueva produccion estado
                string currentSymbol = TrimSymbol(splitCrudeProduction[0]);
                // Analisis de lookahead en base al contexto
                string[] splitContextStateProduction = contextStateProduction._production.Split(' ');
                if (contextStateProduction._actualIndex < splitContextStateProduction.Length - 1)
                {
                    newStateProduction._lookahead = ModifyLookaheadForNonTerminalSymbol(false, contextStateProduction, contextStateProduction._actualIndex);
                }

                // Si la nueva produccion estado contiene al simbolo no terminal debe ajustarse el lookahead incluyendo el lookahead del contexto
                if (splitCrudeProduction.Contains('<' + nonTerminal + '>'))
                {
                    if(recursiveLookaheads == null) 
                        recursiveLookaheads = new Dictionary<string, HashSet<List<string>>>();
                    //Se determina la posicion del no terminal en la produccion
                    int index = 0;
                    foreach (var symbol in splitCrudeProduction)
                    {
                        if (symbol.Equals('<' + nonTerminal + '>'))
                            break;
                        index++;
                    }

                    // La podruccion d contexto tiene almenos un simbolo delante se reduce el lookahead
                    if (index < splitCrudeProduction.Length - 1)
                    {
                        newStateProduction._lookahead = (ModifyLookaheadForNonTerminalSymbol(true, newStateProduction, index));
                        if (!recursiveLookaheads.Keys.Contains(nonTerminal))
                            recursiveLookaheads.Add(nonTerminal, new HashSet<List<string>>());
                        if (!recursiveLookaheads[nonTerminal].Contains(newStateProduction._lookahead))
                        {
                            recursiveLookaheads[nonTerminal].Add(newStateProduction._lookahead);
                        }
                    }
                }
                productionsOfNonTerminal.Add(newStateProduction);

                // Si el simbolo actual es no terminal ...
                if (_sectionsManager.IsNonTerminal(currentSymbol) && !nonTerminal.Equals(currentSymbol))
                {
                    // ... Generar las producciones estado del simbolo no terminal
                    productionsOfNonTerminal.AddRange(GenerateStateProductionsForNonTerminal(currentSymbol, newStateProduction));
                }
            } 
            // Al existir varios contextos en un solo estado no se conoce de donde deriva el simbolo no terminal
            // por lo que todas las producciones relacionadas directamente con este simbolo no terminal deben tener el mismo lookahead
            if(recursiveLookaheads != null)
            {
                List<string> newLookahead = new List<string>();
                foreach(HashSet<List<string>> hashOfLists in recursiveLookaheads.Values)
                {
                    foreach(List<string> lookaheadList in hashOfLists)
                    {
                        foreach(string symbol in lookaheadList)
                        {
                            if(!newLookahead.Contains(symbol))
                                newLookahead.Add(symbol);
                        }
                    }
                }

                foreach(string nonTerminalKey in recursiveLookaheads.Keys)
                {
                    List<LALRStateProduction> newStateProductions = new List<LALRStateProduction>(productionsOfNonTerminal);
                    foreach (LALRStateProduction stateProduction in newStateProductions)
                    {
                        if (stateProduction.EqualsIdentifier(nonTerminalKey) && stateProduction.EqualsStateProduction(contextStateProduction))
                        {
                            int index = productionsOfNonTerminal.IndexOf(stateProduction);
                            productionsOfNonTerminal[index]._lookahead = newLookahead;
                        }
                    }
                }
            }

            return productionsOfNonTerminal;
        }


        private List<string> ModifyLookaheadForNonTerminalSymbol(bool expand, LALRStateProduction contextState, int nonTerminalIndex)
        {
            // Lógica para expandir el lookahead basado en el contexto
            List<string> newLookahead;
            if(expand)
            {
                newLookahead = contextState._lookahead;
            }
            else
            {
                newLookahead = new List<string>();
            }
            string[] splitContextSateProduction = contextState._production.Split(' ');
            if (nonTerminalIndex < splitContextSateProduction.Length - 1)
            {
                string nextSymbol = TrimSymbol(splitContextSateProduction[nonTerminalIndex + 1]);
                if (_sectionsManager.IsNonTerminal(nextSymbol))
                {
                    HashSet<string> firstOfNonTerminal = _nffTable._first[nextSymbol];
                    newLookahead.AddRange(AddFirst(newLookahead, firstOfNonTerminal));
                    if (_nffTable._nullable[nextSymbol] && nonTerminalIndex + 2 < splitContextSateProduction.Length)
                    {
                        for(int i = nonTerminalIndex + 2; i < splitContextSateProduction.Length; i++)
                        {
                            string nextSymbol2 = splitContextSateProduction[i];
                            if (_sectionsManager.IsNonTerminal(nextSymbol2))
                            {
                                HashSet<string> firstOfNonTerminal2 = _nffTable._first[nextSymbol2];
                                newLookahead.AddRange(AddFirst(newLookahead, firstOfNonTerminal2));
                                if (!_nffTable._nullable[nextSymbol2])
                                    break;
                            }
                            else
                            {
                                if (!newLookahead.Contains(nextSymbol2))
                                    newLookahead.Add(nextSymbol2);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (!newLookahead.Contains(nextSymbol))
                        newLookahead.Add(nextSymbol);
                }
            }
            return newLookahead;
        }
        private List<string> AddFirst(List<string> newLookahead, HashSet<string> newSymbols)
        {
            List<string> additionalLookahead = new List<string>();

            foreach (string symbol in newSymbols)
            {
                if (!newLookahead.Contains(symbol))
                {
                    additionalLookahead.Add(symbol);
                }
            }

            return additionalLookahead;
        }
        private void GenerateReductionForState(int currentStateIndex, List<string> consumedSymbol, int indexOfProduccion)
        {
            Tuple<int, List<string>, int> reduction = new Tuple<int, List<string>, int>(currentStateIndex, consumedSymbol, indexOfProduccion);
            if (!consumedSymbol.Contains("ε"))
                reduction = new Tuple<int, List<string>, int>(currentStateIndex, consumedSymbol, indexOfProduccion);
            else
            {
                consumedSymbol.Add("");
                reduction = new Tuple<int, List<string>, int>(currentStateIndex, consumedSymbol, indexOfProduccion);
            }
                
            if (!_reductions.Contains(reduction))
            {
                _reductions.Add(reduction);
            }
        }
        private void GenerateGotosShifts(int currentStateIndex, string consumedSymbol, int nextStateIndex)
        {
            Tuple<int, string, int> transition;
            if(!consumedSymbol.Equals("ε"))
                transition = new Tuple<int, string, int>(currentStateIndex, consumedSymbol, nextStateIndex);
            else
                transition = new Tuple<int, string, int>(currentStateIndex, "", nextStateIndex);
            if (_sectionsManager.IsNonTerminal(consumedSymbol))
            {
                if (!_gotos.Contains(transition))
                {
                    // Añadir a GoTos
                    _gotos.Add(transition);
                }
            }
            else
            {
                if(!_shifts.Contains(transition))
                {
                    //Añadir a Shifts
                    _shifts.Add(transition);
                }
            }
        }
    }
}
