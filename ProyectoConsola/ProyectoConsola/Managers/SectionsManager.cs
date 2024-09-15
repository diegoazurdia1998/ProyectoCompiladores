using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProyectoConsola.Estructuras;

namespace ProyectoConsola.Managers
    {/// <summary>
    /// Clase SectionsManager, responsable de gestionar las secciones de un archivo de configuración.
    /// </summary>
    public class SectionsManager
    {
        /// <summary>
        /// Nombre del compilador.
        /// </summary>
        public string _compilerName;

        /// <summary>
        /// Símbolo de inicio de comentario.
        /// </summary>
        public string _commentStart;

        /// <summary>
        /// Símbolo de fin de comentario.
        /// </summary>
        public string _commentEnd;

        /// <summary>
        /// Lista de símbolos terminales.
        /// </summary>
        public List<string> _terminals;

        /// <summary>
        /// Lista de palabras clave identificadas.
        /// </summary>
        public List<string> _keywords;

        /// <summary>
        /// Lista de tokens simples.
        /// </summary>
        public List<string> _tokens_simple;

        /// <summary>
        /// Diccionario de secciones.
        /// </summary>
        private static Dictionary<string, List<string>> sections;

        /// <summary>
        /// Diccionario de conjuntos identificados.
        /// </summary>
        public Dictionary<string, List<string>> _sets;

        /// <summary>
        /// Diccionario de símbolos no terminales.
        /// </summary>
        public Dictionary<string, List<string>> _nonTerminals;

        /// <summary>
        /// Lista de tokens.
        /// </summary>
        public List<Token> _tokens;
        /// <summary>
        /// Simbolo de extension para inicio de las derivaciones
        /// </summary>
        public string _startSymbol;

        //Constructores
        /// <summary>
        /// Constructor de la clase SectionsManager.
        /// </summary>
        /// <param name="param_Sections">Diccionario de secciones.</param>
        public SectionsManager(Dictionary<string, List<string>> param_Sections) 
        { 
            sections = param_Sections;
            _sets = new Dictionary<string, List<string>>();
            _terminals = new List<string>();
            _terminals.Add("eof");
            _nonTerminals = new Dictionary<string, List<string>>();
            _nonTerminals.Add("SimboloInicial", new List<string>());
            _startSymbol = "SimboloInicial";
            _compilerName = sections["COMPILER"].ToArray()[0];
            _tokens = new List<Token>();
            _keywords = new List<string>();
            _tokens_simple = new List<string>();
            _commentEnd = "";
            _commentStart = "";
            StartManagers();
        }
        /// <summary>
        /// Imprime las secciones del lenguaje de programación en consola.
        /// </summary>
        public void PrintSections()
        {
            Console.WriteLine("Nombre del compilador: " + _compilerName + "\n");
            Console.WriteLine("Simbolos de comentario:\n\tInicio: " + _commentStart + "\n\tFin: " + _commentEnd + "\n");
            Console.WriteLine("Simbolos no terminales: \n" + string.Join(", ", _nonTerminals.Keys) + "\n");
            Console.WriteLine("Palabras reservadas: \n" + string.Join(", ", _keywords) + "\n");
            Console.WriteLine("Sets: ");
            int i = 1;
            foreach (var set in _sets)
            {
                Console.WriteLine(set.Key + ": " + string.Join(", ", set.Value));
            }
            Console.WriteLine("\nTokens: ");
            foreach (var token in _tokens)
            {
                Console.WriteLine("ID: " + token.identifier + " VALUE: " + token.production + " ACTION: " + token.associativity);
            }
            Console.WriteLine("\nProducciones: ");
            foreach (var production in _nonTerminals)
            {
                Console.WriteLine(i + ".\n<" + production.Key + "> = \n\t " + string.Join("\n\t", production.Value));
                Console.WriteLine();
                i++;
            }

        }
        // Métodos
        /// <summary>
        /// Método que inicia la gestión de secciones.
        /// </summary>
        public void StartManagers()
        {
            //Sets
            SetsManager();
            //Tokens
            TokensManager();
            //Keywords
            KeywordsManager();
            //Productions
            ProductionsManager();
        }

        //Metodos para seccion: SETS  
        private void SetsManager() 
        {
            //Verificar SETS
            if (VerifySets())
            {
                //Identificar SETS
                IdentifySets();
            }
        }
        
        /// <summary>
        /// Método que verifica la sección de conjuntos.
        /// </summary>
        /// <returns>True si la sección es válida, false 
        private bool VerifySets()
        {
            bool ok = false;

            List<string> setsList = sections["SETS"];
            Regex identifierRegex = new(@"\s*[A-Za-z]\w*\s*=\s*"),
                rightSideRegex = new(@"(('[A-Za-z0-9_]'((\.\.|\+)'[A-Za-z0-9_]')+)|(chr\(\)((\.\.|\+)chr(\(\d(\d|\d{2})?\)))))\s*;\s*");

            foreach (string set in setsList)
            {
                Match identifierMatch = identifierRegex.Match(set);
                if (identifierMatch.Success)
                {
                    int rightSideIndex = identifierMatch.Index + identifierMatch.Length;
                    string rightSide = set.Substring(rightSideIndex).Trim();
                    if (rightSideRegex.IsMatch(rightSide))
                    { 
                        ok = true;  
                    }
                    else break;
                }
                else break;
            }
            return ok;
        }
        /// <summary>
        /// Método que identifica los conjuntos en la sección de conjuntos.
        /// </summary>
        private void IdentifySets() 
        {
            string auxIdentifier, auxRightSide;
            List<string> setsList = sections["SETS"];
            Regex identifierRegex = new(@"\s*[A-Za-z]\w*\s*=\s*"),
                wordRegex = new(@"[A-Za-z]\w*"),
                rightSideRegex = new(@"(('[A-Za-z0-9_]'((\.\.|\+)'[A-Za-z0-9_]')+)|(chr\(\d(\d|\d{2})?\)((\.\.|\+)chr(\(\d(\d|\d{2})?\))))\s*);\s*");

            Match universalMatch;
            MatchCollection universalMatchCollection;
            foreach (string set in setsList) 
            {
                universalMatch = identifierRegex.Match(set);
                int rightSide = universalMatch.Index + universalMatch.Length;
                auxIdentifier = set.Substring(universalMatch.Index, universalMatch.Length).Trim(); 
                universalMatch = wordRegex.Match(auxIdentifier);
                int identifierIndex = universalMatch.Index + universalMatch.Length;
                auxIdentifier = auxIdentifier.Substring(0, identifierIndex).Trim();
                _sets.Add(auxIdentifier, new List<string>());
                auxRightSide = set.Substring(rightSide).Trim();
                universalMatchCollection = rightSideRegex.Matches(auxRightSide);
                foreach (Match match in universalMatchCollection) 
                {
                    if(match.Value.StartsWith("'")) // CASO: ('[A-Za-z0-9_]'((\.\.|\+)'[A-Za-z0-9_]')+)  <-->  'A'..'Z'+'a'..'z'+'_'
                    {
                        if (match.Value.Contains("+") && match.Value.Contains("..")) 
                        {
                            string[] parts1 = match.Value.Split('+');
                            foreach (string part in parts1)
                            {
                                if(part.Contains(".."))
                                {
                                    string[] parts = part.Split("..");
                                    string aux = '[' + parts[0].Trim('\'') + '-' + parts[1].Trim('\'') + ']';
                                    _sets[auxIdentifier].Add(aux);
                                }
                                else
                                {
                                    string aux = '[' + part.Substring(0, part.Length - 2).Trim('\'') + ']';
                                    _sets[auxIdentifier].Add(aux);
                                }
                            }

                        }
                        else if (match.Value.Contains("+"))
                        {
                            string[] parts = match.Value.Split('+');
                            foreach (string part in parts)
                            {
                                _sets[auxIdentifier].Add('[' + part.Trim('\'') + ']');
                            }
                        }
                        else if (match.Value.Contains(".."))
                        {
                            string[] parts = match.Value.Split("..");
                            string aux = '[' + parts[0].Trim('\'') + '-' + parts[1].Substring(0, parts[1].Length - 1).Trim('\'') + ']';
                            _sets[auxIdentifier].Add(aux);

                        }
                        else
                        {
                            _sets[auxIdentifier].Add('[' + match.Value.Trim('\'') + ']');
                        }
                    }
                    else if (match.Value.Contains("chr")) // CASO: (chr\(\d(\d|\d{2})?\)((\.\.|\+)chr(\(\d(\d|\d{2})?\)  <-->  chr(32)..chr(254)
                    {
                        if (match.Value.Contains("+") && match.Value.Contains(".."))
                        {
                            string[] parts1 = match.Value.Split('+');
                            foreach(string part in parts1)
                            {
                                string[] parts = match.Value.Split("..");
                                string aux = '[' + parts[0].Substring(4).Trim(')') + '-' + parts[1].Substring(4).Trim(')') + ']';

                            }
                        }
                        else if (match.Value.Contains("+"))
                        {
                            string[] parts = match.Value.Split('+');
                            foreach(string part in parts)
                            {
                                string aux = part.Substring(4).Trim(')');
                                if (int.TryParse(aux, out int asciiCode))
                                {
                                    char asciiValue = Convert.ToChar(asciiCode);
                                    _sets[auxIdentifier].Add('[' + asciiValue.ToString() + ']');
                                }
                                else
                                {
                                    // manejar error de conversión
                                }
                            }
                        }
                        else if (match.Value.Contains(".."))
                        {
                            string[] parts = match.Value.Split("..");
                            char minLimit = Convert.ToChar(Convert.ToInt32(parts[0].Substring(4).Trim(')'))),
                                maxLimit = Convert.ToChar(Convert.ToInt32(parts[1].Substring(4).Trim(';').Trim(')')));
                            string aux = "[" + minLimit + '-' + maxLimit + ']';
                            _sets[auxIdentifier].Add(aux);
                        }
                        else
                        {
                            string aux = match.Value.Substring(4).Trim(';').Trim(')');
                            if (int.TryParse(aux, out int asciiCode))
                            {
                                char asciiValue = Convert.ToChar(asciiCode);
                                _sets[auxIdentifier].Add(asciiValue.ToString());
                            }
                            else
                            {
                                // manejar error de conversión
                            }
                        }
                    }
                }
            }
        }
        //Metodos para seccion: TOKENS
        private void TokensManager()
        {
            if (VerifyTokens())
            {
                IdentifyTokens();
            }
        }
        // Métodos para la sección de tokens
        /// <summary>
        /// Método que verifica la sección de tokens.
        /// </summary>
        /// <returns>True si la sección es válida, false
        private bool VerifyTokens() 
        {
            bool ok = false;

            List<string> tokensList = sections["TOKENS"];
            Regex identifierRegex = new(@"\s*[A-Za-z]\w*\s*=\s*"),
                rightSideRegex = new(@"(((\w\s+(\w*\*)?)|(\w\s*\(\s*\w+\s*\|?\s*(\w+\*?)\)\*?)|('.'(,'.')*))(\s*(Left|Right|(c|C)heck)?));");

            foreach (string token in tokensList)
            {
                Match identifierMatch = identifierRegex.Match(token);
                if (identifierMatch.Success)
                {
                    int rightSideIndex = identifierMatch.Index + identifierMatch.Length;
                    string rightSide = token.Substring(rightSideIndex).Trim();
                    if (rightSideRegex.IsMatch(rightSide))
                    {
                        ok = true;
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }
                else break;
            }
            return ok;
        }
        /// <summary>
        /// Método que identifica los tokens en la sección de tokens.
        /// </summary>
        private void IdentifyTokens()
        {
            string auxIdentifier, auxRightSide;
            List<string> tokensList = sections["TOKENS"];
            Regex identifierRegex = new(@"\s*[A-Za-z]\w*\s*=\s*"),
                rightSideRegex = new(@"(((((\w+[+*?]?\s+)?\|?\(?(\w+[+*?]?\s+)(\|\w+[+*?]?\s+)*(\)[+*?])?)+)|('.'(,'.')*))(\s*(Left|Right|Check)?));"),
                actionRegex = new(@"\w+;");

            Match universalMatch;
            foreach (string token in tokensList)
            {
                universalMatch = identifierRegex.Match(token.Trim());
                if(universalMatch.Success)
                {
                    auxIdentifier = token.Trim();
                    auxIdentifier = auxIdentifier.Substring(0, universalMatch.Length - 2).Trim();
                    int rightSideIndex = universalMatch.Index + universalMatch.Length;
                    auxRightSide = token.Substring(rightSideIndex).Trim();
                    universalMatch = actionRegex.Match(auxRightSide);
                    if (universalMatch.Success)
                    {
                        string action = auxRightSide.Substring(universalMatch.Index, universalMatch.Length - 1);
                        auxRightSide = auxRightSide.Substring(0, (auxRightSide.Length  - universalMatch.Length));
                        _tokens.Add(new Token(auxIdentifier, auxRightSide.Trim(';'), action));
                    }
                    else
                    {
                        _tokens.Add(new Token(auxIdentifier, auxRightSide.Trim(';'), ""));
                    }
                }
                else
                {
                    auxRightSide = token;
                    bool Left = false;
                    if (auxRightSide.Contains("Left"))
                    {
                        auxRightSide = auxRightSide.Substring(0, auxRightSide.Length - 5).Trim();
                        Left = true;
                    }
                    else if (auxRightSide.Contains("Right"))
                    {
                        auxRightSide = auxRightSide.Substring(0, auxRightSide.Length - 6).Trim();
                    }
                    string[] parts = auxRightSide.Split(',');
                    List<string> tempList = new List<string>();
                    foreach (string part in parts)
                    {
                        if (Left)
                        {
                            _tokens.Add(new Token("", part.Trim('\''), "LEFT"));
                            
                        }
                        else
                        {
                            _tokens.Add(new Token("", part.Trim('\''), "RIGHT"));
                        }
                    }

                }
            }
        }
        //Metodos para seccion:KEYWORDS
        public void KeywordsManager()
        {
            if (VerifyKeywords())
            {
                IdentifyKeywords();
            }
        }
        // Métodos para la sección de palabras clave
        /// <summary>
        /// Método que verifica la sección de palabras clave.
        /// </summary>
        /// <returns>True si la sección es válida, false en caso contrario.</returns>

        private bool VerifyKeywords()
        {
            bool ok = true;
            Regex keywordRegex = new(@"(\s*'\w+'\s*)|(Comments\s*'.+'\s*TO\s*'.+'\s*\w+;)");
            List<string> keywordsList = sections["KEYWORDS"];
            foreach (string keyword in keywordsList)
            {
                if (!keywordRegex.IsMatch(keyword))
                {
                    ok = false;
                }
            }
            return ok;
        }
        /// <summary>
        /// Método que identifica las palabras clave en la sección de palabras clave.
        /// </summary>
        private void IdentifyKeywords()
        {
            Regex keywordRegex = new(@"('\w+')|(Comments\s*'.+'\s*TO\s*'.+'\s*\w+;)");
            List<string> keywordsList = sections["KEYWORDS"];
            foreach (string keyword in keywordsList)
            {
                MatchCollection mc = keywordRegex.Matches(keyword);

                if (mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        if (m.Value.Contains("Comments"))
                        {
                            Regex tempRegex = new(@"'\W+'");
                            MatchCollection tempMc = tempRegex.Matches(m.Value);
                            if (tempMc.Count == 2)
                            {
                                _commentStart = tempMc[0].Value.Trim('\'');
                                _commentEnd = tempMc[1].Value.Trim('\'');
                            }
                        }
                        else
                        {
                            _keywords.Add(m.Value.Trim().Trim('\''));
                        }
                    }
                }
                
            }
        }
        //Metodos para seccion: PRODUCTIONS
        private void ProductionsManager()
        {
            if (VerifyProductions())
            {
                IdentifyProductions();
            }
        }
        // Métodos para la sección de producciones
        /// <summary>
        /// Método que verifica la sección de producciones.
        /// </summary>
        /// <returns>True si la sección es válida, false en caso contrario.</returns>
        private bool VerifyProductions()
        {
            bool ok = true;
            List<string> prodictionsList = sections["PRODUCTIONS"];
            Regex prodictionsRegex = new(@"(\s*<[A-Za-z][A-Za-z_]+>\s*=\s*)((\(?(\s*'(([A-Za-z][A-Za-z_]+)|([.:,;()<>*""+/-=])|:=|<>|<=|>=)'\s*|\s*<[A-Za-z][A-Za-z_]+>\s*|\s*[A-Za-z]+\s*|\s*ε\s*)\)?\|?)+)");
            Match match;
            foreach (string production in prodictionsList)
            {
                match = prodictionsRegex.Match(production);
                if (!match.Success)
                {
                    ok = false;
                    break;
                }
            }

            return ok;
        }
        /// <summary>
        /// Método que identifica las producciones en la sección de producciones.
        /// </summary>
        private void IdentifyProductions()
        {
            int rightSidendex;
            string identifier, rightSide;
            List<string> prodictionsList = sections["PRODUCTIONS"];
            Regex identifierRegex = new(@"(\s*<[A-Za-z]\w*>\s*=\s*)"),
                nonTerminalRegex = new(@"<[A-Za-z]\w*>"),
                terminalRegex = new(@"'(([A-Za-z]\w*)|:=|<>|<=|>=)'|'\W'|ε"),
                tokenRegex = new(@"[A-Za-z]\w*");
            Match match;
            MatchCollection matchCollection;
            identifier = prodictionsList[0];
            match = identifierRegex.Match(identifier);
            if (match.Success)
            {
                rightSidendex = match.Index + match.Length;
                identifier = identifier.Trim();
                identifier = identifier.Substring(0, rightSidendex - 2).Trim();
                _nonTerminals["SimboloInicial"].Add(identifier + " eof");
            }
            foreach (string production in prodictionsList)
            {
                match = identifierRegex.Match(production);
                rightSidendex = match.Index + match.Length;
                identifier = production.Trim();
                identifier = identifier.Substring(0, rightSidendex -2).Trim().Trim('<').Trim('>');
                if(!_nonTerminals.Keys.Contains(identifier))
                {
                    _nonTerminals.Add(identifier, new List<string>());
                }
                rightSide = production.Trim();
                rightSide = production.Substring(rightSidendex).Trim();
                if (rightSide.Contains('|'))
                {
                    if (!rightSide.Contains('('))
                    {
                        string[] tempProductionsArray = rightSide.Split('|');
                        foreach (string tempProductions in tempProductionsArray)
                        {
                            _nonTerminals[identifier].Add(tempProductions.Trim());
                        }
                    }
                    else
                    {
                        if(rightSide.Contains("\'(\'"))
                        {
                            string[] tempProductionsArray = rightSide.Split('|');
                            foreach (string tempProductions in tempProductionsArray)
                            {
                                _nonTerminals[identifier].Add(tempProductions.Trim());
                            }
                        }
                        else
                        {
                            // Buscamos la posición del primer paréntesis que no está dentro de otro paréntesis
                            int startIndex = 0;
                            int balance = 0;
                            for (int i = 0; i < rightSide.Length; i++)
                            {
                                if (rightSide[i] == '(')
                                {
                                    balance++;
                                }
                                else if (rightSide[i] == ')')
                                {
                                    balance--;
                                }
                                if (balance == 1 && rightSide[i] == '(')
                                {
                                    startIndex = i;
                                    break;
                                }
                            }

                            // Extraemos la parte dentro del paréntesis
                            string innerPart = rightSide.Substring(startIndex + 1, rightSide.IndexOf(')', startIndex) - startIndex - 1);

                            // Extraemos las partes separadas por "|"
                            string[] tempProductionsArray = innerPart.Split('|');

                            // Extraemos la parte después del paréntesis, solo la primera parte
                            string afterParenthesis = rightSide.Substring(rightSide.IndexOf(')', startIndex) + 1).Trim();

                            // Separamos las producciones en dos partes: la parte dentro del paréntesis y la parte después del paréntesis
                            string[] productionsArray = afterParenthesis.Split('|');
                            afterParenthesis = afterParenthesis.Split('|')[0].Trim();
                            // La primera producción es la que contiene el paréntesis
                            string productionWithParenthesis = productionsArray[0];

                            // Las demás producciones son las que no contienen paréntesis
                            string[] otherProductions = new string[productionsArray.Length - 1];
                            Array.Copy(productionsArray, 1, otherProductions, 0, productionsArray.Length - 1);

                            // Agregamos las producciones
                            foreach (string tempProduction in tempProductionsArray)
                            {
                                _nonTerminals[identifier].Add(tempProduction.Trim() + " " + afterParenthesis);
                            }

                            // Agregamos las demás producciones
                            foreach (string otherProduction in otherProductions)
                            {
                                _nonTerminals[identifier].Add(otherProduction.Trim());
                            }
                        }
                    }
                }
                else
                {
                    _nonTerminals[identifier].Add(rightSide);
                }

                string auxTrim;
                matchCollection = nonTerminalRegex.Matches(rightSide);
                foreach(Match nonTerminal in matchCollection)
                {
                    auxTrim = nonTerminal.Value.Trim('<').Trim('>');
                    if (!_nonTerminals.Keys.Contains(auxTrim))
                    {
                        _nonTerminals.Add(auxTrim, new List<string>());
                    }
                }
                matchCollection = terminalRegex.Matches(rightSide);
                foreach(Match matchTerminal in matchCollection)
                {
                    auxTrim = matchTerminal.Value.Trim('\'');
                    if(!_terminals.Contains(auxTrim))
                    {
                        _terminals.Add(auxTrim);
                    }
                }
                matchCollection = tokenRegex.Matches(rightSide);
                foreach(Match matchToken in matchCollection)
                {
                    auxTrim = matchToken.Value.Trim('<').Trim('>');
                    if(!_terminals.Contains(auxTrim) && !_tokens_simple.Contains(auxTrim) && 
                        !_nonTerminals.Keys.Contains(auxTrim))
                    {
                        _tokens_simple.Add(auxTrim);
                    }
                }
            }
        }
    }
}
