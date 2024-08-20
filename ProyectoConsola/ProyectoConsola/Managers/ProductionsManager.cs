using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProyectoConsola.Managers
{
    internal class ProductionsManager
    {
        private static List<string> terminals, nonTerminals, tokens, productions;

        
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

        public bool ValidateProductions()
        {
            bool checkSection = false;
            string pattern1 = @"(\s*<[A-Za-z][A-Za-z_]+>\s*=\s*)";
            Regex regex1 = new Regex(pattern1);

            foreach (string line in productions)
            {
                Match match = regex1.Match(line);
                if (match.Success)
                {
                    int startIndex = match.Index + match.Length;
                    string restOfLine = line.Substring(startIndex).Trim();
                    // Ahora puedes procesar el resto de la línea
                    // ...
                    string pattern2 = @"(\(?(\s*'(([A-Za-z][A-Za-z_]+)|([.:,;()<>*""+/-])|:=|<>|<=|>=)'\s*|\s*<[A-Za-z][A-Za-z_]+>\s*|\s*[A-Za-z]+\s*|\s*ε\s*)\)?\|?)+\$";
                    Regex regex2 = new Regex(pattern2);
                    if (regex2.IsMatch(restOfLine + "$"))
                    {
                        checkSection = true;
                    }
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
            TokenizeProductions();
            return checkSection;
        }

        private void TokenizeProductions()
        {
            string lineAux;
            Regex identifierRegex = new Regex(@"(\s*<[A-Za-z]\w*>\s*=\s*)"), 
                nonTerminalRegex = new Regex(@"<[A-Za-z]\w*>"),
                terminalRegex = new Regex(@"'(([A-Za-z]\w*)|([.:,;()<>*""+/-])|:=|<>|<=|>=)'|ε"),
                tokenRegex = new Regex(@"[A-Za-z]\w*");
            Match match;
            MatchCollection matchCollection;
            foreach (string line in productions)
            {
                lineAux = line + '$';
                match = identifierRegex.Match(lineAux);
                if (match.Success)
                {
                    int index = match.Index + match.Length;
                    lineAux = lineAux.Substring(index).Trim();                    
                    
                    matchCollection = nonTerminalRegex.Matches(lineAux);
                    foreach (Match NT in matchCollection)
                    {
                        if(!nonTerminals.Contains(NT.Value)) nonTerminals.Add(NT.Value.Replace("<", string.Empty).Replace(">", string.Empty));
                    }
                    matchCollection = terminalRegex.Matches(lineAux);
                    foreach (Match T in matchCollection)
                    {
                        if (!terminals.Contains(T.Value)) terminals.Add(T.Value.Replace("'", string.Empty));
                    }
                    matchCollection = tokenRegex.Matches(lineAux);
                    foreach (Match T in matchCollection)
                    {
                        if (!tokens.Contains(T.Value) && !nonTerminals.Contains(T.Value) && !terminals.Contains(T.Value)) tokens.Add(T.Value);
                    }
                }
            }
            //
        }   
        



    }
}
