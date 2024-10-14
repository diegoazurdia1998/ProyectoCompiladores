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
        public Dictionary<int, List<LALRStateProduction>> _states { get; set; }
        public List<LALRTransition> _transitions { get; set; }
        public List<LALRTransition> _gotos { get; set; }
        public List<LALRTransition> _shifts { get; set; }
        public List<LALRReduction> _reductions { get; set; }

        public LALRTableManager(SectionsManager sections)
        {
            _sectionsManager = sections;
            _states = new Dictionary<int, List<LALRStateProduction>>();
            _transitions = new List<LALRTransition>();
            _reductions = new List<LALRReduction>();
            GenerateLALRTable();

        }

        public void GenerateLALRTable()
        {
            // Paso 1: Generar la tabla de estados
            try
            {
                _states = GenerateStates();

            }
            catch (Exception e)
            {
                string mensaje = e.Message;
                throw;
            }

            // Paso 2: Generar la tabla de transiciones
            _transitions = GenerateGotosShifts(_states);
            _reductions = GenerateReductions(_states);
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
                states[0].Add(firstStateProduction);

                // Lógica para generar los estados restantes

                // Lista para las producciones pendientes de procesar
                List<LALRStateProduction> pendingStateProductionsList = [firstStateProduction];

                //Se añade la produccion inicial a la lista
                //pendingStateProductionsList.Add(firstStateProduction);

                //Se inicia el contador de estados
                int actualStateIndex = 0;
                //int productionsInState = 0;

                while (pendingStateProductionsList.Count > 0) //Mientras haya producciones por procesar en la lista
                {

                    LALRStateProduction currentProduction = pendingStateProductionsList.ElementAt(0); //Se copia el valor de la produccion al inicio de la lista

                    if (currentProduction._actualIndex < currentProduction.GetProductionLenght()) //Si no se ha procesado completamente la produccion ...
                    {
                        string currentSymbol = TrimSymbol(currentProduction.GetCurrentSybol()); //Se determina el simbolo actual
                                                                                                //Si el simbolo actual es  ...
                                                                                                //if (!_sectionsManager.IsTerminal(currentSymbol) && pendingStateProductionsList.Count == 0) //... No terminal se deben agregar las producciones de ese simbolo no terminal
                        
                        //... No terminal se deben agregar las producciones de ese simbolo no terminal
                        if (!_sectionsManager.IsTerminal(currentSymbol)) 
                        {
                            if (actualStateIndex > 0)
                            {
                                states.Add(actualStateIndex, [currentProduction]);
                            }

                            //Puede existir un contexto nuevo, segun el caso
                            //Se genera(n) la(s) produccion(es) segun su contexto
                            List<LALRStateProduction> tempStateProductions = GenerateStateProductionsForNonTerminal(currentSymbol, currentProduction);

                            //Se añaden al estado actual las producciones adicionales
                            states[actualStateIndex].AddRange(tempStateProductions);

                            //Se añaden a la pilistala de producciones las nuevas producciones
                            pendingStateProductionsList.AddRange(tempStateProductions);
                        }

                        //SE determinan las producciones iniciales del nuevo estado
                        List<LALRStateProduction> initialStateProductions = GenerateNewStateProductions(currentProduction, pendingStateProductionsList);

                        //Si las producciones iniciales son mas de 0
                        if (initialStateProductions.Count > 0)
                        {
                            // Se asegura que el nuevo estado sea unico
                            Tuple<bool, int> unique = EnsureUniquenessOfStates(initialStateProductions, states);

                            //Si el estado no es unico se genera una transicion
                            if (unique.Item1)
                            {
                                //Se agrega un nuevo estado al diccionario de estados                            
                                states.Add(states.Count, initialStateProductions);
                                //Se agregagan la(s) produccion(es) inicial(es) al estado
                                //states[newStateIndex].AddRange(initialStateProductions);
                            }
                            else
                            {
                                //Transicion 
                                GenereteTransitionForState(actualStateIndex, currentSymbol, unique.Item2);
                            }

                            //Se remueven las producciones de estado procesadas de la lista de estados pendeintes
                            // y se agregan las nuevas producciones estado

                            //pendingStateProductionsList.RemoveAt(currentProduction);
                            pendingStateProductionsList.Remove(currentProduction);
                            foreach (var sp in initialStateProductions)
                            {
                                //productionsInState++;
                                pendingStateProductionsList.Add(sp);
                            }

                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        pendingStateProductionsList.Remove(currentProduction);
                        //productionsInState++;
                        // Si el simbolo esta al final de la produccion
                        //Reduction
                        //int productionIndex = _sectionsManager._orderedNonTerminals.IndexOf(new Tuple<string, string>(currentProduction._identifier, currentProduction._production));
                        //if (!currentProduction._identifier.Equals(_sectionsManager._startSymbol))
                        //{
                        //    GenerateReductionForState(actualStateIndex, currentProduction._lookahead, productionIndex);
                        //}
                        //

                    }

                    actualStateIndex++;
                    //if (states[actualStateIndex].Count == pendingStateProductionsList.Count)
                    //{
                    //    //productionsInState = 0;
                    //    //if(actualStateIndex == 9)
                    //    //{
                    //    //    productionsInState = 0;
                    //    //}
                    //}
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
        /// <param name="lastStateProductions">Listado de producciones del estado anterior</param>
        /// <returns>Lista de producciones correspondientes al nuevo estado</returns>
        private List<LALRStateProduction> GenerateNewStateProductions(LALRStateProduction firstStateProduction, List<LALRStateProduction> lastStateProductions)
        {
            List<LALRStateProduction> productionsForNewState = new List<LALRStateProduction>();

            //Se determina el simbolo actual de la produccion inicial
            string consumedSymbol = TrimSymbol(firstStateProduction._production.Split(' ')[firstStateProduction._actualIndex]);

            //Del estado anterior se seleccionan las producciones que ...
            foreach(var production in lastStateProductions)
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
                    foreach (var production in prospectProductions)
                    {
                        bool found = false;
                        foreach (var stateProduction in stateProductions)
                        {
                            if (production.EqualsStateProduction(stateProduction))
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
            return new Tuple<bool, int>(true, -1);

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
            
            // Auxiliar para contexto de la produccion estado
            //LALRStateProduction auxContextState = contextState;
            
            // Por cada produccion del simbolo no terminal ...
            foreach(var crudeProduction in crudeProductions)
            {
                // Se crea la produccion estado a partir de la produccion
                LALRStateProduction newStateProduction = new LALRStateProduction(0, nonTerminal, crudeProduction, contextState._lookahead);
                string[] splitCrudeProduction = crudeProduction.Split(' ');
                //Se determina el simbolo actual de la nueva produccion estado
                string currentSymbol = TrimSymbol(splitCrudeProduction[0]);
                
                // Si el simbolo actual es no terminal ...
                if (!_sectionsManager.IsTerminal(currentSymbol))
                {
                    // ... Generar las producciones estado del simbolo no terminal
                    productionsOfNonTerminal.AddRange(GenerateStateProductionsForNonTerminal(currentSymbol, newStateProduction));
                }

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

                    // Se verifica que el no terminal no sea el ultimo simbolo de la produccion
                    if (index < splitCrudeProduction.Length - 1)
                    {
                        // El lookahead de la produccion de estado se modifica
                        // ... implementacion de la logica de las llamadas a aumentar o reducir lookahead    !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                    }
                    // Si es el ultimo simbolo de la produccion de estado el lookahead no se modifica
                }
                productionsOfNonTerminal.Add(newStateProduction);
            }

            return productionsOfNonTerminal;
        }

        private List<string> ExpandLookaheadForNonTerminalSymbol(LALRStateProduction contextState)
        {
            // Lógica para generar el lookahead basado en el contexto
            List<string> newLookahead = contextState._lookahead;
           


            return newLookahead;
        }

        private List<string> ReduceLookaheadForNonTerminalSymbol(LALRStateProduction contextState)
        {
            List<string> newLookahead = contextState._lookahead;

            return newLookahead;
        }

        private void GenereteTransitionForState(int currentStateIndex, string consumedSymbol, int nextStateIndex)
        {
            LALRTransition transition = new LALRTransition(currentStateIndex, nextStateIndex, consumedSymbol);
            _transitions.Add(transition);
        }
        private void GenerateReductionForState(int currentStateIndex, List<string> consumedSymbol, int indexOfProduccion)
        {
            _reductions.Add(new LALRReduction(currentStateIndex, consumedSymbol, indexOfProduccion));
        }
        private List<LALRTransition> GenerateGotosShifts(Dictionary<int, List<LALRStateProduction>> states)
        {
            // Lógica para generar las transiciones entre estados
            List<LALRTransition> transitions = new List<LALRTransition>();
           


            return transitions;
        }

  

        private List<LALRReduction> GenerateReductions(Dictionary<int, List<LALRStateProduction>> states)
        {
            // Lógica para generar las reducciones
            List<LALRReduction> reductions = new List<LALRReduction>();
      
            return reductions;
        }
    }
}
