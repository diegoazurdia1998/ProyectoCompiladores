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
        public string Symbol { get; set; }

        /// <summary>
        /// Obtiene o establece el ID de la producción.
        /// </summary>
        public int ProductionId { get; set; }
    }
}