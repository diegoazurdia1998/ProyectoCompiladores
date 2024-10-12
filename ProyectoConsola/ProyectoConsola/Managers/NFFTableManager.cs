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
    public Dictionary<string, HashSet<string>> _followGenerationAux;
    private SectionsManager _sectionsManager; // Referencia al administrador de secciones
    private string _startSymbol; //Simbolo no terminal del que parte la gramatica

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
        _followGenerationAux = new Dictionary<string, HashSet<string>>();
        _sectionsManager = manager;
        _startSymbol = manager._startSymbol;
        // Inicializar tablas para cada no terminal
        foreach (string nonTerminal in manager._nonTerminals.Keys)
        {
            _nullable[nonTerminal] = false;
            _first[nonTerminal] = new HashSet<string>();
            _follow[nonTerminal] = new HashSet<string>();
        }
        GenertaeTables();
    }
    /// <summary>
    /// Imprime las tablas de Nullable, First y Follow en consola.
    /// </summary>
    public void PrintTables()
    {
        Console.WriteLine("\nTabla de Nullable, First y Follow:");
        int i = 1;
        foreach (string nonTerminal in _sectionsManager._nonTerminals.Keys)
        {
            Console.Write("\nSimbolo No Terminal --    " + i + "    --: \n\t\t\t\t\t" + nonTerminal + "\n\tNullable: ");
            Console.Write(_nullable[nonTerminal] + "\n\tFirst: ");
            Console.Write(string.Join(", ", _first[nonTerminal]) + "\n\tFollow: ");
            Console.WriteLine(string.Join(", ", _follow[nonTerminal]));
            i++;
        }
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
        Dictionary<string, List<string>> producciones = _sectionsManager._nonTerminals;
        foreach (string nonTerminal in _sectionsManager._nonTerminals.Keys)
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
        foreach (var nonTerminal in _sectionsManager._nonTerminals.Keys)
        {
            
            if (_first[nonTerminal].Count < 1)
            {
                
                _first[nonTerminal] = GenerateFirstForNonTerminal(nonTerminal);
            }
        }
    }

    private HashSet<string> GenerateFirstForNonTerminal(string nonTerminal)
    {
        HashSet<string> firstofNonTeminal = new HashSet<string>();
        foreach (var item in _sectionsManager._nonTerminals[nonTerminal])
        {
            string[] production = item.Split(' ');
            string firstSymbol = production[0];
            if ((firstSymbol.Contains("<") || firstSymbol.Contains(">")) && firstSymbol.Contains("\'"))
            {
                firstSymbol = production[0].Trim().Trim('\'').Trim('(');
            }
            else
            {
                if(!firstSymbol.Contains("("))
                    firstSymbol = production[0].Trim().Trim('\'').Trim('(').Trim('<').Trim('>');
                else
                    firstSymbol = production[0].Trim().Trim('\'').Trim('<').Trim('>');
            }
            if (IsTerminal(firstSymbol) && !firstSymbol.Equals(""))
            {
                firstofNonTeminal.Add(firstSymbol);
            }
            else if (IsNonTerminal(firstSymbol))
            {
                if (_first[nonTerminal].Count < 1 && !nonTerminal.Equals(firstSymbol))
                {
                    _first[firstSymbol] = GenerateFirstForNonTerminal(firstSymbol);
                }
                firstofNonTeminal.UnionWith(_first[firstSymbol]);
                if (_nullable[firstSymbol] && production.Length > 1)
                {
                    firstofNonTeminal.UnionWith(GenerateFirstForNullableNonTerminal(nonTerminal, production));

                }

            }
        }
        return firstofNonTeminal;
    }

    private HashSet<string> GenerateFirstForNullableNonTerminal(string nonTerminal, string[] production)
    {
        string nextSymbol;
        HashSet<string> firstofNullableNonTeminal = new HashSet<string>();
        for (int i = 1; i < production.Length; i++)
        {
            nextSymbol = production[i].Trim().Trim('\'').Trim('(').Trim('<').Trim('>');
            if (!nextSymbol.Equals(nonTerminal))
            {
                if (IsTerminal(nextSymbol))
                {
                    firstofNullableNonTeminal.Add(nextSymbol);
                    break;
                }
                else if (IsNonTerminal(nextSymbol))
                {
                    if (_first[nextSymbol].Count < 1)
                    {
                        _first[nextSymbol] = GenerateFirstForNonTerminal(nextSymbol);
                    }
                    firstofNullableNonTeminal.UnionWith(_first[nextSymbol]);
                    if (!_nullable[nextSymbol])
                    {
                        break;
                    }
                }
            }
        }
        return firstofNullableNonTeminal;
    }

    /// <summary>
    /// Verifica si un símbolo es terminal.
    /// </summary>
    /// <param name="symbol">Símbolo a verificar.</param>
    /// <returns>True si el símbolo es terminal, false en caso contrario.</returns>
    public bool IsTerminal(string symbol)
    {
        bool isTerminal = true;
        if (!_sectionsManager._terminals.Contains(symbol))
        {
            isTerminal = false;
            foreach (Token token in _sectionsManager._tokens)
            {
                if (token.TokenEquals(symbol))
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
        Dictionary<string, List<string>> producciones = _sectionsManager._nonTerminals;
        foreach (string nonTerminal in _sectionsManager._nonTerminals.Keys)
        {
            _followGenerationAux.Add(nonTerminal, new HashSet<string>());
            _followGenerationAux[nonTerminal] = GenerateProductionsNonTermial(nonTerminal);
        }
        foreach (string nonTerminal in _sectionsManager._nonTerminals.Keys)
        {
            if (_follow[nonTerminal].Count == 0)
            {
                _follow[nonTerminal] = ComputeFollowForNonTerminal(nonTerminal);
            }
        }
    }
    private HashSet<string> GenerateProductionsNonTermial(string symbol)
    {
        HashSet<string> result = new HashSet<string>();
        Dictionary<string, List<string>> producciones = _sectionsManager._nonTerminals;
        foreach (string ProductionsKey in _sectionsManager._nonTerminals.Keys)
        {
            List<string> productions = _sectionsManager._nonTerminals[ProductionsKey];
            foreach (string production in productions)
            {
                if (production.Contains('<' + symbol + '>'))
                {
                    result.Add(ProductionsKey + '=' + production);
                }
            }
        }
        return result;
    }
    private bool ProductionsOfNT1CointainsNT2(string NT1, string NT2)
    {
        bool result = false;
        List<string> productionsNT1 = _sectionsManager._nonTerminals[NT1];
        foreach (var production in productionsNT1)
        {
            if (production.Contains(NT2))
            {
                result = true;
            }
        }
        return result;
    }
    /// <summary>
    /// Calcula el conjunto de Follow para un no terminal.
    /// </summary>
    /// <param name="nonTerminal">No terminal para el que se calcula el conjunto de Follow.</param>
    private HashSet<string> ComputeFollowForNonTerminal(string nonTerminal)
    {
        HashSet<string> productionWhereNonTerminalAppears = _followGenerationAux[nonTerminal],
            followOfNonTerminal = new HashSet<string>();
        foreach (string production in productionWhereNonTerminalAppears)
        {
            string identifier = production.Substring(0, production.IndexOf('=')).Trim();
            string[] realProduction = production.Substring(production.IndexOf('=') + 1).Trim().Split(' ');
            int nonTerminalIndex = Array.IndexOf(realProduction, '<' + nonTerminal + '>');

            if (nonTerminalIndex == realProduction.Length - 1)
            {
                if (_follow[identifier].Count == 0 && !identifier.Equals(nonTerminal))
                {
                    if(!ProductionsOfNT1CointainsNT2(identifier, nonTerminal) && !ProductionsOfNT1CointainsNT2(nonTerminal, identifier))
                    {
                        _follow[identifier] = ComputeFollowForNonTerminal(identifier);                        
                    }
                    else
                    {
                        _follow[nonTerminal].UnionWith(followOfNonTerminal);
                        _follow[nonTerminal].UnionWith(_follow[identifier]);
                        _follow[identifier].UnionWith(_follow[nonTerminal]);
                        
                    }
                }
                followOfNonTerminal.UnionWith(_follow[identifier]);
            }
            else if (nonTerminalIndex < realProduction.Length - 1)
            {
                int nextNonTerminalIndex = nonTerminalIndex + 1;
                string nextNonTerminal = realProduction[nextNonTerminalIndex].Trim().Trim('\'').Trim('(').Trim('<').Trim('>');
                if (IsTerminal(nextNonTerminal))
                {
                    followOfNonTerminal.Add(nextNonTerminal);
                }
                else if (IsNonTerminal(nextNonTerminal))
                {
                    followOfNonTerminal.UnionWith(_first[nextNonTerminal]);
                    if (_nullable[nextNonTerminal])
                    {
                        followOfNonTerminal.UnionWith(GenerateFollowForNullableNonTerminal(nonTerminal, nextNonTerminalIndex, identifier, realProduction));
                    }
                }
            }

        }
        if(_sectionsManager._startSymbol.Equals(nonTerminal))
        {
            if (ContainsSymbol(nonTerminal, "eof"))
                followOfNonTerminal.Add("eof");
            else if (ContainsSymbol(nonTerminal, "$"))
                followOfNonTerminal.Add("$");
            else if (ContainsSymbol(nonTerminal, "'$'"))
                followOfNonTerminal.Add("$");
        }
        if(productionWhereNonTerminalAppears.Count == 0)
        {
            followOfNonTerminal.Add("$");
        }
        return followOfNonTerminal;
    }
    private bool ContainsSymbol(string nonTerminal, string symbol)
    {
        bool contains = false;
        List<string> productions = _sectionsManager._nonTerminals[nonTerminal];
        foreach (string production in productions)
        {
            string[] aux = production.Split(' ');
            if(aux.Contains(symbol))
            {
                contains = true; 
                break; 
            }
        }
        return contains;
    }
    private HashSet<string> GenerateFollowForNullableNonTerminal(string nonTermial, int nextNonTerminalIndex, string identifier, string[] production)
    {
        HashSet<string> followOfNullableNonTerminal = new HashSet<string>();

        for (int i = nextNonTerminalIndex; i < production.Length; i++)
        {
            string nextSymbol = production[i].Trim().Trim('\'').Trim('(').Trim('<').Trim('>');
            if (IsTerminal(nextSymbol))
            {
                followOfNullableNonTerminal.Add(nextSymbol);
            }
            else if (IsNonTerminal(nextSymbol))
            {
                if (i != production.Length - 1)
                {
                    followOfNullableNonTerminal.UnionWith(_first[nextSymbol]);
                    if (!_nullable[nextSymbol])
                    {
                        break;
                    }
                }
                else
                {
                    followOfNullableNonTerminal.UnionWith(_first[nextSymbol]);
                    if (_nullable[nextSymbol])
                    {
                        if (_follow[nextSymbol].Count == 0)
                        {
                            _follow[nextSymbol] = ComputeFollowForNonTerminal(nextSymbol);
                            
                        }
                        if (!nextSymbol.Equals(nonTermial))
                        {
                            followOfNullableNonTerminal.UnionWith(_follow[nextSymbol]);

                        }
                    }
                }
            }
            
            
        }

        return followOfNullableNonTerminal;
    }

    /// <summary>
    /// Verifica si un símbolo es no terminal.
    /// </summary>
    /// <param name="symbol">Símbolo a verificar.</param>
    /// <returns>True si el símbolo es no terminal, false en caso contrario.</returns>
    public bool IsNonTerminal(string symbol)
    {
        return _sectionsManager._nonTerminals.ContainsKey(symbol);
    }

    

}

