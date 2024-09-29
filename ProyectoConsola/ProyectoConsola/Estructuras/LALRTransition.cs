namespace ProyectoConsola.Estructuras
{
    /// <summary>
    /// Representa una transición LALR en el análisis sintáctico.
    /// </summary>
    public class LALRTransition
    {
        /// <summary>
        /// Obtiene o establece el ID del estado de origen.
        /// </summary>
        public int FromStateId { get; set; }

        /// <summary>
        /// Obtiene o establece el ID del estado de destino.
        /// </summary>
        public int ToStateId { get; set; }

        /// <summary>
        /// Obtiene o establece el símbolo consumido.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="LALRTransition"/>.
        /// </summary>
        /// <param name="currentState">ID del estado actual.</param>
        /// <param name="nextState">ID del siguiente estado.</param>
        /// <param name="consumedSymbol">Símbolo consumido.</param>
        public LALRTransition(int currentState, int nextState, string consumedSymbol)
        {
            FromStateId = currentState;
            ToStateId = nextState;
            Symbol = consumedSymbol;
        }
    }
}