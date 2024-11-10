using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProyectoConsola.JJ
{
    public class LeerArchivoDePrueba
    {
        public List<string> Leer(string inputString){
            FileManager fm = new FileManager();
            string inString;
            if (!inputString.Equals(String.Empty) && !inputString.Equals(" "))
                inString = fm.ReadNewFile(inputString);
            else
            {
                inString = inputString;
            }
            List<string> input;
            if (inString.Equals(""))
            {
                input = StringToListString(inputString);
            }
            else
            {
                input = StringToListString(inString);
            }
            input.Add("$");

            return input;
        }

        private List<string> StringToListString(string str)
        {
            // Reemplazamos múltiples espacios en blanco por un solo espacio
            Regex spacingRegex = new Regex(@"\s+");
            string cleanedString = spacingRegex.Replace(str.Trim(), " ");
            // Dividimos la cadena en palabras usando el espacio como delimitador
            List<string> result = cleanedString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return result;
        }
    }
}