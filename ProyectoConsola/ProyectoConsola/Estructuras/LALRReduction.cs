namespace ProyectoConsola.Estructuras
{
    /// <summary>
    /// Representa una reducción LALR en el análisis sintáctico.
    /// </summary>
    public class LALRReduction
    {
        /// <summary>
        /// Obtiene o establece el ID del estado.
        /// </summary>
        public int StateId { get; set; }

        /// <summary>
        /// Obtiene o establece el símbolo.
        /// </summary>
        public List<string> Symbols { get; set; }

        /// <summary>
        /// Obtiene o establece el ID de la producción.
        /// </summary>
        public int ProductionId { get; set; }
        /// <summary>
        /// Crea un objeto Reduccion
        /// </summary>
        /// <param name="currentStateIndex">Estado del que se parte</param>
        /// <param name="consumedSymbol">Simbolo consumido</param>
        /// <param name="indexOfProduccion">Indice de la produccion que reduce</param>
        public LALRReduction(int currentStateIndex, List<string> consumedSymbol, int indexOfProduccion)
        {
            StateId = currentStateIndex;
            Symbols = consumedSymbol;
            ProductionId = indexOfProduccion;
        }
    }
}