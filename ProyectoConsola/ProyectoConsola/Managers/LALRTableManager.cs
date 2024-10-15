using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ProyectoConsola.Estructuras;
using static System.Net.Mime.MediaTypeNames;

namespace ProyectoConsola.Managers
{
/// <summary>
/// Clase que contiene las tablas de estados, transiciones y reducciones
/// </summary>
    public class LALRTableManager
    {
        private SectionsManager _sectionsManager;
        //public Dictionary<int, List<LALRStateProduction>> _states { get; set; }
        public List<Tuple<int, string, int>> _gotos { get; set; }
        public List<Tuple<int, string, int>> _shifts { get; set; }
        public List<Tuple<int, List<string>, int>> _reductions { get; set; }

        public LALRTableManager(SectionsManager sections)
        {
            _sectionsManager = sections;
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
                //_states = 
                GenerateStates();

            }
            catch (Exception e)
            {
                string mensaje = e.Message;
                throw;
            }
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
                        List<LALRStateProduction> newState_StateProductions = GenerateNewStateProductions(currentProduction, states[actualStateIndex]);
                        // Eliminar las producciones recien procesadas y extenderlas si es necesario
                        List<LALRStateProduction> aux = new List<LALRStateProduction>();
                        foreach (var production in newState_StateProductions)
                        {
                            pendingStateProductionsList.RemoveAt(0);
                            countOfStateProductionsProcessedInActualStateIndex++;
                            string trimProductionSymbol = TrimSymbol(production.GetCurrentSybol());
                            if (!trimProductionSymbol.Equals("") && _sectionsManager.IsNonTerminal(trimProductionSymbol))
                            {
                                aux = GenerateStateProductionsForNonTerminal(trimProductionSymbol, production);
                            }
                        }
                        newState_StateProductions.AddRange(aux);
                        // Asegurar que el estado sea unico
                        Tuple<bool, int> unique = EnsureUniquenessOfStates(newState_StateProductions, states);
                        //Si ...
                        if (unique.Item1)
                        {// ... es unico se crea un nuevo estado
                            states.Add(unique.Item2, newState_StateProductions);
                            pendingStateProductionsList.AddRange(newState_StateProductions);
                        }
                        GenerateGotosShifts(actualStateIndex, currentSymbol, unique.Item2);

                    }
                    else
                    {// ... Si el simbolo esta al final de la produccion

                        countOfStateProductionsProcessedInActualStateIndex++;
                        pendingStateProductionsList.RemoveAt(0);
                        
                        //Reduction
                        int productionIndex = _sectionsManager._orderedNonTerminals.IndexOf(new Tuple<string, string>(currentProduction._identifier, currentProduction._production));
                        if (!currentProduction._identifier.Equals(_sectionsManager._startSymbol) && productionIndex >= 0)
                        {
                            GenerateReductionForState(actualStateIndex, currentProduction._lookahead, productionIndex);
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
        private List<LALRStateProduction> GenerateNewStateProductions(LALRStateProduction firstStateProduction, List<LALRStateProduction> currentStateProductions)
        {
            List<LALRStateProduction> productionsForNewState = new List<LALRStateProduction>();

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
                        LALRStateProduction tempProduction = production.Clone();
                        tempProduction._actualIndex++; // Se aumenta el indice del simbolo actual de la produccion
                        productionsForNewState.Add(tempProduction);
                    }
                }
            }

            return productionsForNewState;
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
        /// <param name="contextState">Produccion actualmente analizada </param>
        /// <returns>Lista de producciones estado derivadas de la actual</returns>
        private List<LALRStateProduction> GenerateStateProductionsForNonTerminal(string nonTerminal, LALRStateProduction contextState)
        {
            // Lista de produccionees estado para el simbolo no terminal
            List<LALRStateProduction> productionsOfNonTerminal = new List<LALRStateProduction>();
            // Lisado de producciones del simbolo no terminal
            List<string> crudeProductions = _sectionsManager._nonTerminals[nonTerminal];
            
            // Por cada produccion del simbolo no terminal ...
            foreach(var crudeProduction in crudeProductions)
            {
                // Se crea la produccion estado a partir de la produccion
                LALRStateProduction newStateProduction = new LALRStateProduction(0, nonTerminal, crudeProduction, contextState._lookahead);
                string[] splitCrudeProduction = crudeProduction.Split(' ');
                //Se determina el simbolo actual de la nueva produccion estado
                string currentSymbol = TrimSymbol(splitCrudeProduction[0]);

                // Si la nueva produccion estado contiene al simbolo no terminal debe ajustarse el lookahead
                if (splitCrudeProduction.Contains('<' + nonTerminal + '>'))
                {
                    //Se determina la posicion del no terminal en la produccion
                    int index = 0;
                    foreach (var symbol in splitCrudeProduction)
                    {
                        if (symbol.Equals('<' + nonTerminal + '>'))
                        {
                            break;
                        }
                        index++;
                    }
                    bool expand = true;
                    // La podruccion d contexto tiene almenos un simbolo delante se reduce el lookahead
                    if(contextState._actualIndex < contextState._production.Length)
                    {
                        expand = false;
                    }
                    var newLookahead = contextState._lookahead;
                    // El lookahead de la produccion de estado se modifica 
                    if(expand)
                    {
                        //Se expande
                        newLookahead.AddRange(ExpandLookaheadForNonTerminalSymbol(newStateProduction, index));
                    }
                    else
                    {
                        //Se reduce
                        newLookahead = ReduceLookaheadForNonTerminalSymbol(newStateProduction, index);
                    }
                    newStateProduction._lookahead = newLookahead;
                }
                productionsOfNonTerminal.Add(newStateProduction);

                // Si el simbolo actual es no terminal ...
                if (_sectionsManager.IsNonTerminal(currentSymbol) && !nonTerminal.Equals(currentSymbol))
                {
                    // ... Generar las producciones estado del simbolo no terminal
                    productionsOfNonTerminal.AddRange(GenerateStateProductionsForNonTerminal(currentSymbol, newStateProduction));
                }
            }

            return productionsOfNonTerminal;
        }

        private List<string> ExpandLookaheadForNonTerminalSymbol(LALRStateProduction contextState, int nonTerminalIndex)
        {
            // Lógica para expandir el lookahead basado en el contexto
            List<string> newLookahead = contextState._lookahead;
            if (nonTerminalIndex < contextState._production.Length)
            {
                string[] strings = contextState._production.Split(' ');
                if(!newLookahead.Contains(strings[nonTerminalIndex + 1]))
                    newLookahead.Add(strings[nonTerminalIndex + 1]);
            }
            return newLookahead;
        }

        private List<string> ReduceLookaheadForNonTerminalSymbol(LALRStateProduction contextState, int nonTerminalIndex)
        {
            // Lógica para expandir el lookahead basado en el contexto
            List<string> newLookahead = contextState._lookahead;
            if (nonTerminalIndex < contextState._production.Length)
            {
                string[] strings = contextState._production.Split(' ');
                if (!newLookahead.Contains(strings[nonTerminalIndex + 1]))
                    newLookahead.Add(strings[nonTerminalIndex + 1]);
            }
            return newLookahead;
        }

        private void GenerateReductionForState(int currentStateIndex, List<string> consumedSymbol, int indexOfProduccion)
        {
            Tuple<int, List<string>, int> reduction = new Tuple<int, List<string>, int>(currentStateIndex, consumedSymbol, indexOfProduccion);
            if (!_reductions.Contains(reduction))
            {
                _reductions.Add(reduction);
            }
        }
        private void GenerateGotosShifts(int currentStateIndex, string consumedSymbol, int nextStateIndex)
        {
            Tuple<int, string, int> transition = new Tuple<int, string, int>(currentStateIndex, consumedSymbol, nextStateIndex);
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
