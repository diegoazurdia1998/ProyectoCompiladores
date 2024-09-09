using ProyectoConsola.Estructuras;

using ProyectoConsola.Managers;

/// <summary>
/// Clase responsable de generar las tablas de Nullable, First y Follow para un conjunto de reglas de producción de un lenguaje formal.
/// </summary>
public class NFFTableManager
{
    // Atributos para almacenar las tablas de Nullable, First y Follow
    public Dictionary<string, bool> _nullable; // Tabla de Nullable
    public Dictionary<string, HashSet<string>> _first; // Tabla de First
    public Dictionary<string, HashSet<string>> _follow; // Tabla de Follow
    private Dictionary<string, bool> _computedFollow; // Tabla para evitar la recursividad infinita en la generación de la tabla de Follow
    private SectionsManager _sectionsManager; // Referencia al administrador de secciones

    /// <summary>
    /// Constructor de la clase NFFTableManager.
    /// </summary>
    /// <param name="manager">Administrador de secciones.</param>
    public NFFTableManager(SectionsManager manager)
    {
        _nullable = new Dictionary<string, bool>();
        _first = new Dictionary<string, HashSet<string>>();
        _follow = new Dictionary<string, HashSet<string>>();
        _computedFollow = new Dictionary<string, bool>();
        _sectionsManager = manager;
        // Inicializar tablas para cada no terminal
        foreach (string nonTerminal in manager.nonTerminals.Keys)
        {
            _nullable[nonTerminal] = false;
            _first[nonTerminal] = new HashSet<string>();
            _follow[nonTerminal] = new HashSet<string>();
        }
        GenertaeTables();
    }

    /// <summary>
    /// Genera las tablas de Nullable, First y Follow.
    /// </summary>
    private void GenertaeTables()
    {
        // Nullable
        GenerateNullableTable();
        // First
        GenerateFirstTable();
        // Follow
        GenerateFollowTable();
    }

    /// <summary>
    /// Genera la tabla de Nullable.
    /// </summary>
    private void GenerateNullableTable()
    {
        Dictionary<string, List<string>> producciones = _sectionsManager.nonTerminals;
        foreach (string nonTerminal in _sectionsManager.nonTerminals.Keys)
        {
            if (producciones[nonTerminal].Contains("ε"))
            {
                _nullable[nonTerminal] = true;
            }
        }
    }

    /// <summary>
    /// Genera la tabla de First.
    /// </summary>
    private void GenerateFirstTable()
    {
        Dictionary<string, List<string>> producciones = _sectionsManager.nonTerminals;

        foreach (string nonTerminal in _sectionsManager.nonTerminals.Keys.Reverse())
        {
            foreach (string production in producciones[nonTerminal])
            {
                string firstSymbol = production.Split(' ')[0].Trim('(').Trim('\'').Trim('<').Trim('>');
                if (firstSymbol != "")
                {
                    if (IsTerminal(firstSymbol))
                    {
                        _first[nonTerminal].Add(firstSymbol);
                    }
                    else // En caso que t NO sea terminal
                    {
                        _first[nonTerminal].UnionWith(GetFirstForNonTerminal(firstSymbol, nonTerminal));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Obtiene el conjunto de First para un no terminal.
    /// </summary>
    /// <param name="firstNonTerminalSymbol">Símbolo no terminal.</param>
    /// <param name="originalNonTerminal">No terminal original.</param>
    /// <returns>Conjunto de First para el no terminal.</returns>
    private HashSet<string> GetFirstForNonTerminal(string firstNonTerminalSymbol, string originalNonTerminal)
    {
        // Si el first no terminal aún no tiene First
        if (_first[firstNonTerminalSymbol].Count == 0)
        {
            // Se extraen producciones del first que es símbolo no terminal
            List<string> firstNonterminalSymbolProductions = _sectionsManager.nonTerminals[firstNonTerminalSymbol];
            if (firstNonterminalSymbolProductions == null || firstNonterminalSymbolProductions.Count == 0)
            {
                // Si el no terminal symbol tiene no producciones, devuelve un conjunto vacío
                return new HashSet<string>();
            }

            foreach (string production in firstNonterminalSymbolProductions) // por cada producción del first no terminal
            {
                // Se calcula el primer símbolo de la producción
                string firstSymbol = production.Split(' ')[0].Trim('(').Trim('\'').Trim('<').Trim('>');
                if (firstSymbol != "")
                {
                    if (IsTerminal(firstSymbol))
                    {
                        _first[firstNonTerminalSymbol].Add(firstSymbol);
                    }
                    else // En caso que t NO sea terminal
                    {
                        // Para evitar la recursividad infinita, solo recursa si el primer símbolo no es el mismo que el no terminal actual
                        if (firstSymbol != firstNonTerminalSymbol)
                        {
                            _first[firstNonTerminalSymbol].UnionWith(GetFirstForNonTerminal(firstSymbol, originalNonTerminal));
                        }
                    }
                }
            }
        }
        return _first[firstNonTerminalSymbol];
    }

    /// <summary>
    /// Verifica si un símbolo es terminal.
    /// </summary>
    /// <param name="symbol">Símbolo a verificar.</param>
    /// <returns>True si el símbolo es terminal, false en caso contrario.</returns>
    private bool IsTerminal(string symbol)
    {
        bool isTerminal = true;
        if (!_sectionsManager.terminals.Contains(symbol))
        {
            isTerminal = false;
            foreach (Token token in _sectionsManager.tokens)
            {
                if (token.CompareTo(symbol))
                {
                    isTerminal = true;
                    break;
                }
            }
        }
        // Verificar si el símbolo es terminal
        return isTerminal;
    }

    /// <summary>
    /// Genera la tabla de Follow.
    /// </summary>
    private void GenerateFollowTable()
    {
        Dictionary<string, List<string>> producciones = _sectionsManager.nonTerminals;
        foreach (string nonTerminal in _sectionsManager.nonTerminals.Keys)
        {
            ComputeFollowForNonTerminal(nonTerminal);
        }
    }

    /// <summary>
    /// Calcula el conjunto de Follow para un no terminal.
    /// </summary>
    /// <param name="nonTerminal">No terminal para el cual se calcula el conjunto de Follow.</param>
    private void ComputeFollowForNonTerminal(string nonTerminal)
    {
        if (_computedFollow.ContainsKey(nonTerminal) && _computedFollow[nonTerminal])
        {
            return;
        }

        _computedFollow[nonTerminal] = true;

        Dictionary<string, List<string>> producciones = _sectionsManager.nonTerminals;
        foreach (string production in producciones[nonTerminal])
        {
            string[] symbols = production.Split(' ');
            for (int i = 0; i < symbols.Length - 1; i++)
            {
                string currentSymbol = symbols[i];
                string nextSymbol = symbols[i + 1];
                if (IsNonTerminal(currentSymbol) && IsTerminal(nextSymbol))
                {
                    AddToFollow(currentSymbol, nextSymbol);
                }
                else if (IsNonTerminal(currentSymbol) && IsNonTerminal(nextSymbol))
                {
                    ComputeFollowForNonTerminal(nextSymbol);
                    UnionFollowSets(currentSymbol, nextSymbol);
                }
            }
            // Caso especial: si la producción termina con un no terminal,
            // debemos agregar el conjunto de símbolos terminales que pueden seguir
            // al no terminal final a la tabla de Follow
            if (IsNonTerminal(symbols[symbols.Length - 1]))
            {
                string lastNonTerminal = symbols[symbols.Length - 1];
                ComputeFollowForNonTerminal(lastNonTerminal);
                UnionFollowSets(nonTerminal, lastNonTerminal);
            }
        }
    }

    /// <summary>
    /// Agrega un símbolo a la tabla de Follow para un no terminal.
    /// </summary>
    /// <param name="nonTerminal">No terminal para el cual se agrega el símbolo.</param>
    /// <param name="symbol">Símbolo a agregar.</param>
    private void AddToFollow(string nonTerminal, string symbol)
    {
        if (!_follow.ContainsKey(nonTerminal))
        {
            _follow[nonTerminal] = new HashSet<string>();
        }
        _follow[nonTerminal].Add(symbol);
    }

    /// <summary>
    /// Une dos conjuntos de Follow.
    /// </summary>
    /// <param name="nonTerminal1">Primer no terminal.</param>
    /// <param name="nonTerminal2">Segundo no terminal.</param>
    private void UnionFollowSets(string nonTerminal1, string nonTerminal2)
    {
        if (_follow.ContainsKey(nonTerminal1) && _follow.ContainsKey(nonTerminal2))
        {
            _follow[nonTerminal1].UnionWith(_follow[nonTerminal2]);
        }
    }

    /// <summary>
    /// Verifica si un símbolo es no terminal.
    /// </summary>
    /// <param name="symbol">Símbolo a verificar.</param>
    /// <returns>True si el símbolo es no terminal, false en caso contrario.</returns>
    private bool IsNonTerminal(string symbol)
    {
        return _sectionsManager.nonTerminals.ContainsKey(symbol);
    }
}