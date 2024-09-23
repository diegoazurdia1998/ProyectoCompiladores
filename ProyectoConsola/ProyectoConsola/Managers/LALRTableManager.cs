using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ProyectoConsola.Estructuras;

namespace ProyectoConsola.Managers
{
    public class LALRTableManager
    {
        private NFFTableManager _nFFTable;
        private SectionsManager _sectionsManager;
        public Dictionary<int, List<LALRStateProduction>> _states { get; set; }
        public List<LALRTransition> _transitions { get; set; }
        public List<LALRReduction> _reductions { get; set; }

        public LALRTableManager(NFFTableManager NFFTable, SectionsManager sections)
        {
            _nFFTable = NFFTable;
            _sectionsManager = sections;
            _states = new Dictionary<int, List<LALRStateProduction>>();
            _transitions = new List<LALRTransition>();
            _reductions = new List<LALRReduction>();
            GenerateLALRTable();

        }

        

        public void GenerateLALRTable()
        {
            // Paso 1: Generar la tabla de estados
            _states = GenerateStates();

            // Paso 2: Generar la tabla de transiciones
            _transitions = GenerateTransitions(_states);

            // Paso 3: Generar la tabla de reducciones
            _reductions = GenerateReductions(_states, _transitions);
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
            Dictionary<int, List<LALRStateProduction>> states = new Dictionary<int, List<LALRStateProduction>>();

            // Generar el primer estado
            LALRStateProduction firstStateProduction = new LALRStateProduction(0, _sectionsManager._startSymbol, _sectionsManager._nonTerminals[_sectionsManager._startSymbol][0], new List<string> { "$" });
            states.Add(0, new List<LALRStateProduction>());
            states[0].Add(firstStateProduction);

            // Lógica para generar los estados restantes
            List<LALRStateProduction> stateProductionsQueue = new List<LALRStateProduction>();
            stateProductionsQueue.Add(firstStateProduction);
            int stateIndex = 0;
            while (stateProductionsQueue.Count > 0) //Mientras haya estados por procesar
            {
                LALRStateProduction currentProduction = stateProductionsQueue.ElementAt(0); //Se saca el estado el inicio de la pila
                
                int currentIndex = currentProduction._actualIndex; 
                string[] stateProduction = currentProduction._production.Split(' '); 
                if (currentIndex < stateProduction.Length) //Si no se ha procesado completamente la produccion
                {
                    string currentSymbol = stateProduction[currentIndex]; //Se determina el simbolo actual
                    currentSymbol = TrimSymbol(currentSymbol);
                    //Añadir producciones al estado
                    if(_nFFTable.IsNonTerminal(currentSymbol))
                    {
                        List<LALRStateProduction> tempStateProductions = GenerateStateProductionsForNonTerminal(currentSymbol, currentProduction);
                        states[stateIndex].Union(tempStateProductions);
                        stateProductionsQueue.AddRange(tempStateProductions);
                    }
                    //Nuevo estado (procesar produccion/es del estado anterior)
                    List<LALRStateProduction> initialStateProductions = GenerateNewStateProductions(stateIndex, stateIndex + 1,currentProduction, states[stateIndex]);
                    states.Add(stateIndex++, initialStateProductions);
                    foreach(var sp in initialStateProductions)
                    {
                        stateProductionsQueue.RemoveAt(0);
                        stateProductionsQueue.Add(sp);
                    }
                    
                }
            }

            return states;
        }

        private List<LALRStateProduction> GenerateNewStateProductions(int lastState,int nextState,LALRStateProduction firstStateProduction,List<LALRStateProduction> lastStateProductions)
        {
            List<LALRStateProduction> productionsForNewState = new List<LALRStateProduction>();
            int firstStateProductionIndex = firstStateProduction._actualIndex;
            string firstStateProductionSymbol = firstStateProduction._production.Split(' ')[firstStateProductionIndex];
            firstStateProductionSymbol = TrimSymbol(firstStateProductionSymbol);
            
            //Se añaden las producciones del anterior estado que consumen el mismo simbolo
            foreach (var stateProduction in lastStateProductions)
            {
                int currentIndex = stateProduction._actualIndex;
                string[] currentProduction = stateProduction._production.Split(' ');
                string currentSymbol = currentProduction[currentIndex];
                currentSymbol = TrimSymbol(currentSymbol);
                if (currentSymbol.Equals(firstStateProductionSymbol))
                {
                    stateProduction._actualIndex++;
                    productionsForNewState.Add(stateProduction);
                }
            }
            //Se extiende el listado de producciones para el estado
            List<LALRStateProduction> auxStates = productionsForNewState;
            foreach (var stateProduction in auxStates)
            {
                int currentIndex = stateProduction._actualIndex;
                if (currentIndex < stateProduction._production.Length)
                {
                    string[] currentProduction = stateProduction._production.Split(' ');
                    string currentSymbol = currentProduction[currentIndex];
                    currentSymbol = TrimSymbol(currentSymbol);
                    if (_nFFTable.IsNonTerminal(currentSymbol))
                    {
                        List<LALRStateProduction> tempStateProductions = GenerateStateProductionsForNonTerminal(currentSymbol, stateProduction);
                        productionsForNewState.AddRange(tempStateProductions);

                    }

                }
            }
            
            return productionsForNewState;
        }
        private List<LALRStateProduction> GenerateStateProductionsForNonTerminal(string nonTerminal, LALRStateProduction contextState)
        {
            List<LALRStateProduction> statesOfNonTerminal = new List<LALRStateProduction>();

            foreach(var ntKey in _sectionsManager._nonTerminals.Keys)
            {
                if(ntKey.Equals(nonTerminal))
                { 
                    foreach(var production in _sectionsManager._nonTerminals[ntKey])
                    {
                        string firstSymbolOfNewProduction = TrimSymbol(production.Split(' ')[0]);
                        LALRStateProduction newStateProduction;
                        firstSymbolOfNewProduction = TrimSymbol(firstSymbolOfNewProduction);
                        //Calcular lookahead
                        if (_nFFTable.IsNonTerminal(firstSymbolOfNewProduction) && firstSymbolOfNewProduction.Equals(nonTerminal))
                        {//Si el primer simbolo de la nueva produccion es no terminal y es igual al simbolo analizado
                            if(production.Split(' ').Length > 1)
                            {
                                newStateProduction = new LALRStateProduction(0, ntKey, production, ExpandLookaheadForNonTerminalSymbol(firstSymbolOfNewProduction, contextState));
                                statesOfNonTerminal.Add(newStateProduction);
                            }
                            if(contextState._production.Split(" ").Length > contextState._actualIndex)
                            {
                                if(_nFFTable.IsTerminal(contextState._production.Split(" ")[contextState._actualIndex + 1]))
                                {
                                    newStateProduction = new LALRStateProduction(0, ntKey, production, ReduceLookaheadForNonTerminalSymbol(contextState));
                                    statesOfNonTerminal.Add(newStateProduction);
                                }
                            }

                        }
                        else
                        {
                            newStateProduction = new LALRStateProduction(0, ntKey, production, contextState._lookahead);
                            statesOfNonTerminal.Add(newStateProduction);
                        }
                        
                        
                    }
                }
            }

            return statesOfNonTerminal;
        }

        private List<string> ExpandLookaheadForNonTerminalSymbol(string production ,LALRStateProduction contextState)
        {
            // Lógica para generar el lookahead basado en el contexto
            List<string> newLookahead = contextState._lookahead;
            string nextSymbol = production.Split(" ")[1];
            if (_nFFTable.IsTerminal(nextSymbol))
            {
                newLookahead.Add(nextSymbol);
            }
            else 
            {
                //Que pasa si  un no terminal esta delante del simbolo analizado?
            }


            return newLookahead;
        }

        private List<string> ReduceLookaheadForNonTerminalSymbol(LALRStateProduction contextState)
        {
            List<string> newLookahead = contextState._lookahead;
            
            return newLookahead;
        }

        private List<LALRTransition> GenerateTransitions(Dictionary<int, LALRStateProduction> states)
        {
            // Lógica para generar las transiciones entre estados
            List<LALRTransition> transitions = new List<LALRTransition>();
            foreach (var state in states.Values)
            {
                foreach (var symbol in _sectionsManager._terminals)
                {
                    int toStateId = GetNextStateId(state, symbol);
                    if (toStateId != -1)
                    {
                        LALRTransition transition = new LALRTransition
                        {
                            FromStateId = state._actualIndex,
                            ToStateId = toStateId,
                            Symbol = symbol
                        };
                        transitions.Add(transition);
                    }
                }
            }
            return transitions;
        }

        private int GetNextStateId(LALRStateProduction currentState, string symbol)
        {
            // Lógica para determinar el siguiente estado basado en el símbolo actual
            foreach (var state in _states.Values)
            {
                if (state._identifier == currentState._identifier &&
                    state._productions == currentState._production &&
                    state._actualIndex == currentState._actualIndex + 1 &&
                    state._lookahead.SequenceEqual(currentState._lookahead))
                {
                    return state._actualIndex;
                }
            }
            return -1;
        }

        private List<LALRReduction> GenerateReductions(Dictionary<int, LALRStateProduction> states, List<LALRTransition> transitions)
        {
            // Lógica para generar las reducciones
            List<LALRReduction> reductions = new List<LALRReduction>();
            foreach (var state in states.Values)
            {
                if (state._actualIndex == state._production.Length)
                {
                    foreach (var lookahead in state._lookahead)
                    {
                        LALRReduction reduction = new LALRReduction
                        {
                            StateId = state._actualIndex,
                            Symbol = lookahead,
                            ProductionId = _sectionsManager._nonTerminals[state._identifier].IndexOf(state._production)
                        };
                        reductions.Add(reduction);
                    }
                }
            }
            return reductions;
        }
    }
}