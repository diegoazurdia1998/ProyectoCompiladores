namespace ProyectoConsola.Estructuras
{
    /// <summary>
    /// Representa una producción de estado LALR en el análisis sintáctico.
    /// </summary>
    public class LALRStateProduction
    {
        /// <summary>
        /// Obtiene o establece el índice actual.
        /// </summary>
        public int _actualIndex { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador.
        /// </summary>
        public string _identifier { get; set; }

        /// <summary>
        /// Obtiene o establece la producción.
        /// </summary>
        public string _production { get; set; }

        /// <summary>
        /// Obtiene o establece la lista de lookahead.
        /// </summary>
        public List<string> _lookahead { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="LALRStateProduction"/>.
        /// </summary>
        /// <param name="index">Índice de la producción.</param>
        /// <param name="identifier">Identificador de la producción.</param>
        /// <param name="productions">Producción.</param>
        /// <param name="lookahead">Lista de lookahead.</param>
        public LALRStateProduction(int index, string identifier, string productions, List<string> lookahead)
        {
            _identifier = identifier;
            _actualIndex = index;
            _production = productions;
            this._lookahead = lookahead;
        }

        /// <summary>
        /// Añade un lookahead a la lista de lookahead.
        /// </summary>
        /// <param name="looakahead">Lookahead a añadir.</param>
        public void AddLookahead(string looakahead)
        {
            this._lookahead.Add(looakahead);
        }

        /// <summary>
        /// Compara si dos producciones de estado son iguales.
        /// </summary>
        /// <param name="stateProduction">Producción de estado a comparar.</param>
        /// <returns>Verdadero si las producciones de estado son iguales; de lo contrario, falso.</returns>
        public bool EqualsStateProduction(LALRStateProduction stateProduction)
        {
            if (_production.Equals(stateProduction._production) && _actualIndex.Equals(stateProduction._actualIndex))
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Compara si una producción de estado es igual a una producción de estado anterior.
        /// </summary>
        /// <param name="oldStateProduction">Producción de estado anterior a comparar.</param>
        /// <returns>Verdadero si las producciones de estado son iguales; de lo contrario, falso.</returns>
        public bool EqualsOldStateProduction(LALRStateProduction oldStateProduction)
        {
            if (_production.Equals(oldStateProduction._production) && (_actualIndex - 1).Equals(oldStateProduction._actualIndex))
            {
                return true;
            }
            else return false;
        }

        public bool StateXContainsProductionY(Dictionary<int, List<LALRStateProduction>> states, LALRStateProduction productionParam)
        {
            foreach(var state in states)
            {
                foreach(var production in state.Value)
                {
                    if (productionParam.Equals(production))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}