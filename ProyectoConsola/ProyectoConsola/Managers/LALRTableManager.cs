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
            //Diccionario a llenar
            Dictionary<int, LALRState> states = new Dictionary<int, LALRState>();
            //Generar el primer estado
            LALRState firstState = new LALRState(0, _sectionsManager._startSymbol, _sectionsManager._nonTerminals[_sectionsManager._startSymbol][0], new List<string>());

        }
        private List<string> GenerateLookaheadForNonSymbol(string nonTerminal)
        {

        }

        private List<LALRTransition> GenerateTransitions(Dictionary<int, LALRState> states)
        {
            
        }

        private List<LALRReduction> GenerateReductions(Dictionary<int, LALRState> states, List<LALRTransition> transitions)
        {
            
        }
        
    }
}
