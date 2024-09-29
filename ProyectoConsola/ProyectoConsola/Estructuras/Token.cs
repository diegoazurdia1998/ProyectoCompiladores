/// <summary>
/// Clase que representa un token en un lenguaje de programación.
/// Un token es una unidad básica de código que puede ser un identificador, una palabra clave, un símbolo, etc.
/// </summary>
namespace ProyectoConsola.Estructuras
{
    public class Token
    {
        // Atributo que almacena el identificador del token
        /// <summary>
        /// Identificador del token.
        /// </summary>
        public string identifier { get; set; }

        // Atributo que almacena la producción del token
        /// <summary>
        /// Producción del token, es decir, la regla de producción que lo genera.
        /// </summary>
        public string production { get; set; }

        // Atributo que almacena la asociatividad del token
        /// <summary>
        /// Asociatividad del token, es decir, si es asociativo a la izquierda o a la derecha.
        /// </summary>
        public string associativity { get; set; }

        /// <summary>
        /// Constructor de la clase Token.
        /// Inicializa los atributos del token con los valores proporcionados.
        /// </summary>
        /// <param name="identifier">Identificador del token.</param>
        /// <param name="production">Producción del token.</param>
        /// <param name="associativity">Asociatividad del token.</param>
        public Token(string identifier, string production, string associativity)
        {
            // Inicializa el identificador del token
            this.identifier = identifier;
            // Inicializa la producción del token
            this.production = production;
            // Inicializa la asociatividad del token
            this.associativity = associativity;
        }

        /// <summary>
        /// Método que compara el token con un identificador proporcionado.
        /// </summary>
        /// <param name="other">Identificador a comparar.</param>
        /// <returns>True si el token es igual al identificador proporcionado, false en caso contrario.</returns>
        public bool TokenEquals(string other)
        {
            if (this.identifier.Equals(other))
            { 
                return true; 
            }
            if (this.production.Equals(other))
            {
                return true;
            }
            return false;
        }
    }
}
