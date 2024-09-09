using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProyectoConsola.Estructuras;

namespace ProyectoConsola.Managers
{
    public class SectionsManager
    {
        //Atributos
        public string compilerName;
        public string commentStarSymbol, commentFinalSymbol;
        public List<string> terminals, //Lista de simbolos terminales
            keywords, // KEYWORDS identificados
            tokens_simple; //
        private static Dictionary<string, List<string>> sections; //Secciones
        public Dictionary<string, List<string>> sets, //SETS identificados
            nonTerminals; //Lista de simbolos no terminales
        public List<Token> tokens;

        //Constructores

        public SectionsManager(Dictionary<string, List<string>> param_Sections) 
        { 
            sections = param_Sections;
            sets = new Dictionary<string, List<string>>();
            terminals = new List<string>();
            nonTerminals = new Dictionary<string, List<string>>();
            compilerName = sections["COMPILER"].ToArray()[0];
            tokens = new List<Token>();
            keywords = new List<string>();
            tokens_simple = new List<string>();
            commentFinalSymbol = "";
            commentStarSymbol = "";
            StartManagers();
        }
        
        //
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
                sets.Add(auxIdentifier, new List<string>());
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
                                    sets[auxIdentifier].Add(aux);
                                }
                                else
                                {
                                    string aux = '[' + part.Substring(0, part.Length - 2).Trim('\'') + ']';
                                    sets[auxIdentifier].Add(aux);
                                }
                            }

                        }
                        else if (match.Value.Contains("+"))
                        {
                            string[] parts = match.Value.Split('+');
                            foreach (string part in parts)
                            {
                                sets[auxIdentifier].Add('[' + part.Trim('\'') + ']');
                            }
                        }
                        else if (match.Value.Contains(".."))
                        {
                            string[] parts = match.Value.Split("..");
                            string aux = '[' + parts[0].Trim('\'') + '-' + parts[1].Substring(0, parts[1].Length - 1).Trim('\'') + ']';
                            sets[auxIdentifier].Add(aux);

                        }
                        else
                        {
                            sets[auxIdentifier].Add('[' + match.Value.Trim('\'') + ']');
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
                                    sets[auxIdentifier].Add('[' + asciiValue.ToString() + ']');
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
                            sets[auxIdentifier].Add(aux);
                        }
                        else
                        {
                            string aux = match.Value.Substring(4).Trim(';').Trim(')');
                            if (int.TryParse(aux, out int asciiCode))
                            {
                                char asciiValue = Convert.ToChar(asciiCode);
                                sets[auxIdentifier].Add(asciiValue.ToString());
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
                        tokens.Add(new Token(auxIdentifier, auxRightSide.Trim(';'), action));
                    }
                    else
                    {
                        tokens.Add(new Token(auxIdentifier, auxRightSide.Trim(';'), ""));
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
                            tokens.Add(new Token("", part.Trim('\''), "LEFT"));
                            
                        }
                        else
                        {
                            tokens.Add(new Token("", part.Trim('\''), "RIGHT"));
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
                                commentStarSymbol = tempMc[0].Value.Trim('\'');
                                commentFinalSymbol = tempMc[1].Value.Trim('\'');
                            }
                        }
                        else
                        {
                            keywords.Add(m.Value.Trim().Trim('\''));
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
            foreach (string production in prodictionsList)
            {
                match = identifierRegex.Match(production);
                rightSidendex = match.Index + match.Length;
                identifier = production.Trim();
                identifier = identifier.Substring(0, rightSidendex -2).Trim().Trim('<').Trim('>');
                if(!nonTerminals.Keys.Contains(identifier))
                {
                    nonTerminals.Add(identifier, new List<string>());
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
                            nonTerminals[identifier].Add(tempProductions.Trim());
                        }
                    }
                    else
                    {
                        if(rightSide.Contains("\'(\'"))
                        {
                            string[] tempProductionsArray = rightSide.Split('|');
                            foreach (string tempProductions in tempProductionsArray)
                            {
                                nonTerminals[identifier].Add(tempProductions.Trim());
                            }
                        }
                        else
                        {
                            Regex auxRegex = new(@"\(\s*<\w+>\s*(\|\s*<\w+>\s*)*\)");
                            match = auxRegex.Match(rightSide);
                            if(match.Success)
                            {
                                string temp = match.Value;
                                rightSide = rightSide.Replace(temp, "°");
                                string[] tempProductionsArray = rightSide.Split('|');
                                foreach (string tempProductions in tempProductionsArray)
                                {
                                    nonTerminals[identifier].Add(tempProductions.Replace("°", temp).Trim());
                                }
                            }
                        }
                    }
                }
                else
                {
                    nonTerminals[identifier].Add(rightSide);
                }

                string auxTrim;
                matchCollection = nonTerminalRegex.Matches(rightSide);
                foreach(Match nonTerminal in matchCollection)
                {
                    auxTrim = nonTerminal.Value.Trim('<').Trim('>');
                    if (!nonTerminals.Keys.Contains(auxTrim))
                    {
                        nonTerminals.Add(auxTrim, new List<string>());
                    }
                }
                matchCollection = terminalRegex.Matches(rightSide);
                foreach(Match matchTerminal in matchCollection)
                {
                    auxTrim = matchTerminal.Value.Trim('\'');
                    if(!terminals.Contains(auxTrim))
                    {
                        terminals.Add(auxTrim);
                    }
                }
                matchCollection = tokenRegex.Matches(rightSide);
                foreach(Match matchToken in matchCollection)
                {
                    auxTrim = matchToken.Value.Trim('<').Trim('>');
                    if(!terminals.Contains(auxTrim) && !tokens_simple.Contains(auxTrim) && 
                        !nonTerminals.Keys.Contains(auxTrim))
                    {
                        tokens_simple.Add(auxTrim);
                    }
                }
            }
        }
    }
}
