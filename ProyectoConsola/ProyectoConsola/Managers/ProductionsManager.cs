using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProyectoConsola.Managers
{/// <summary>
/// Clase manejadora de la seccion PRODUCTIONS
/// </summary>
    internal class ProductionsManager
    {
        private static List<string> terminals, nonTerminals, tokens, productions;

        /// <summary>
        /// Constructor de la clse
        /// Inicializa las listas para la clase
        /// </summary>
        /// <param name="section">Seccion PRODUCTIONS</param>
        public ProductionsManager(List<string> section)
        {
            //PRODUCTIONS
            productions = section; 
            //Inicializar Tokens
            tokens = new List<string>();
            //Inicializacion de terminaes
            terminals = new List<string>();
            //Iniccializacion de no terminales
            nonTerminals = new List<string>();

        }
        /// <summary>
        /// Valida si la seccion una sintaxis correcta, True: llama ala clase para tokenizar
        /// </summary>
        /// <returns>True: si es valida False: si no es valida</returns>
        public bool ValidateProductions()
        {
            bool checkSection = false;
            string pattern1 = @"(\s*<[A-Za-z][A-Za-z_]+>\s*=\s*)";
            Regex regex1 = new(pattern1);

            foreach (string line in productions)
            {
                Match match = regex1.Match(line);
                if (match.Success)
                {
                    int startIndex = match.Index + match.Length;
                    string restOfLine = line.Substring(startIndex).Trim();
                    string pattern2 = @"(\(?(\s*'(([A-Za-z][A-Za-z_]+)|([.:,;()<>*""+/-])|:=|<>|<=|>=)'\s*|\s*<[A-Za-z][A-Za-z_]+>\s*|\s*[A-Za-z]+\s*|\s*ε\s*)\)?\|?)+\$";
                    Regex regex2 = new(pattern2);
                    if (regex2.IsMatch(restOfLine + "$")) checkSection = true;
                    else
                    {
                        checkSection = false;
                        break;
                    }
                }
                else
                {
                    checkSection = false;
                    break;
                }
            }
            if(checkSection) TokenizeProductions();
            return checkSection;
        }
        /// <summary>
        /// Tokeniza los simbolos de seccion
        /// </summary>
        private void TokenizeProductions()
        {
            string lineAux;
            List<string> tempNT = new List<string>(), tempT = new List<string>();
            Regex identifierRegex = new(@"(\s*<[A-Za-z]\w*>\s*=\s*)"), 
                nonTerminalRegex = new(@"<[A-Za-z]\w*>"),
                terminalRegex = new(@"'(([A-Za-z]\w*)|([.:,;()<>*""+/-])|:=|<>|<=|>=)'|ε"),
                tokenRegex = new(@"[A-Za-z]\w*");
            Match match;
            MatchCollection matchCollection;
            foreach (string line in productions)
            {
                lineAux = line;
                match = identifierRegex.Match(lineAux);
                if (match.Success)
                {
                    int index = match.Index + match.Length;
                    lineAux = lineAux.Substring(index).Trim();                    
                    
                    matchCollection = nonTerminalRegex.Matches(lineAux);
                    foreach (Match NT in matchCollection)
                    {
                        if (!nonTerminals.Contains(NT.Value))
                        {
                            tempNT.Add(NT.Value.Replace("<", string.Empty).Replace(">", string.Empty));
                            nonTerminals.Add(NT.Value);
                        }
                    }
                    matchCollection = terminalRegex.Matches(lineAux);
                    foreach (Match T in matchCollection)
                    {
                        if (!terminals.Contains(T.Value))
                        {
                            terminals.Add(T.Value);
                            tempT.Add(T.Value.Replace("'", string.Empty));
                        }
                    }
                    matchCollection = tokenRegex.Matches(lineAux);
                    foreach (Match T in matchCollection)
                    {
                        if (!tokens.Contains(T.Value) && !tempNT.Contains(T.Value) && !tempT.Contains(T.Value)) tokens.Add(T.Value);
                    }
                }
            }
            //
        }   
        



    }
}
