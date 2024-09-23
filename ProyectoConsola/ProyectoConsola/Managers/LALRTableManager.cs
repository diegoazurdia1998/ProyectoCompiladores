using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProyectoConsola.Estructuras;

namespace ProyectoConsola.Managers
{
    public class LALRTableManager
    {
        private NFFTableManager _nFFTable;
        private SectionsManager _sectionsManager;
        public Dictionary<int, LALRState> _states { get; set; }
        public List<LALRTransition> _transitions { get; set; }
        public List<LALRReduction> _reductions { get; set; }

        public LALRTableManager(NFFTableManager NFFTable, SectionsManager sections)
        {
            _nFFTable = NFFTable;
            _sectionsManager = sections;
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

        private Dictionary<int, LALRState> GenerateStates()
        {
            // Diccionario a llenar
            Dictionary<int, LALRState> states = new Dictionary<int, LALRState>();

            // Generar el primer estado
            LALRState firstState = new LALRState(0, _sectionsManager._startSymbol, _sectionsManager._nonTerminals[_sectionsManager._startSymbol][0], new List<string> { "$" });
            states.Add(0, firstState);

            // Lógica para generar los estados restantes
            Queue<LALRState> stateQueue = new Queue<LALRState>();
            stateQueue.Enqueue(firstState);
            int stateIndex = 1;
            while (stateQueue.Count > 0)
            {
                LALRState currentState = stateQueue.Dequeue();
                int currentIndex = currentState._actualIndex;

                if (currentIndex < currentState._productions.Length)
                {
                    char currentSymbol = currentState._productions[currentIndex];

                    if (_sectionsManager._nonTerminals.ContainsKey(currentSymbol.ToString()))
                    {
                        List<string> lookahead = GenerateLookaheadForNonSymbol(currentSymbol.ToString(), currentState);

                        foreach (var production in _sectionsManager._nonTerminals[currentSymbol.ToString()])
                        {
                            LALRState newState = new LALRState(stateIndex, currentSymbol.ToString(), production, lookahead);
                            states.Add(stateIndex, newState);
                            stateQueue.Enqueue(newState);
                            stateIndex++;
                        }
                    }
                }
            }

            return states;
        }

        private List<string> GenerateLookaheadForNonSymbol(string nonTerminal, LALRState contextState)
        {
            // Lógica para generar el lookahead basado en el contexto
            List<string> lookahead = new List<string>();

            // Obtener el índice actual y la producción previa del estado de contexto
            int currentIndex = contextState._actualIndex;
            string previousProduction = contextState._productions;

            // Si el símbolo en el índice actual es un terminal, el lookahead es ese símbolo
            if (_sectionsManager._terminals.Contains(previousProduction[currentIndex].ToString()))
            {
                lookahead.Add(previousProduction[currentIndex].ToString());
            }
            else
            {
                // Si el símbolo en el índice actual es un no terminal, determinar el lookahead en función del contexto
                foreach (var production in _sectionsManager._nonTerminals[nonTerminal])
                {
                    foreach (var symbol in production)
                    {
                        if (_sectionsManager._terminals.Contains(symbol.ToString()))
                        {
                            lookahead.Add(symbol.ToString());
                        }
                    }
                }
            }

            return lookahead;
        }

        private List<LALRTransition> GenerateTransitions(Dictionary<int, LALRState> states)
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

        private int GetNextStateId(LALRState currentState, string symbol)
        {
            // Lógica para determinar el siguiente estado basado en el símbolo actual
            foreach (var state in _states.Values)
            {
                if (state._identifier == currentState._identifier &&
                    state._productions == currentState._productions &&
                    state._actualIndex == currentState._actualIndex + 1 &&
                    state._lookahead.SequenceEqual(currentState._lookahead))
                {
                    return state._actualIndex;
                }
            }
            return -1;
        }

        private List<LALRReduction> GenerateReductions(Dictionary<int, LALRState> states, List<LALRTransition> transitions)
        {
            // Lógica para generar las reducciones
            List<LALRReduction> reductions = new List<LALRReduction>();
            foreach (var state in states.Values)
            {
                if (state._actualIndex == state._productions.Length)
                {
                    foreach (var lookahead in state._lookahead)
                    {
                        LALRReduction reduction = new LALRReduction
                        {
                            StateId = state._actualIndex,
                            Symbol = lookahead,
                            ProductionId = _sectionsManager._nonTerminals[state._identifier].IndexOf(state._productions)
                        };
                        reductions.Add(reduction);
                    }
                }
            }
            return reductions;
        }
    }
}