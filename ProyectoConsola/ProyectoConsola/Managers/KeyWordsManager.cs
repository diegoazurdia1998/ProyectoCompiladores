using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ProyectoConsola.Managers
{
    internal class KeyWordsManager
    {
        static string ValidacionDeLasProducciones(List<string> listadoDeKeywords, List<string> listadoDeProducciones)
        {
            return "en proceso";
        }

        static List<string> ObtenerLasKeyWords(Dictionary<string, List<string>> ListadoDeSecciones)
        {
            List<string> listadoDeKeywords = [];
            
            foreach (var seccion in ListadoDeSecciones)
            {
                if (seccion.Key.ToUpper() == "KEYWORDS")
                {
                    foreach (string fila in seccion.Value)
                    {
                        string[] filaTemp = fila.Split(",");
                        listadoDeKeywords.AddRange(filaTemp
                            .Where(x => !string.IsNullOrEmpty(x.Trim()))
                            .Select(x => x.Contains(";") ? x.Replace(";", "").Trim() : x.Trim()));
                    }
                }
            }

            return listadoDeKeywords;
        }

        public string ValidarKeywords(Dictionary<string, List<string>> ListadoDeSecciones)
        {
            List<string> listadoKeywords = ObtenerLasKeyWords(ListadoDeSecciones);

            foreach (var seccion in ListadoDeSecciones)
            {
                if (seccion.Key.ToUpper() == "PRODUCTIONS")
                {
                    return ValidacionDeLasProducciones(listadoKeywords, seccion.Value);
                }
            }

            return "Validacion de Keywords correctamente";
        }
        


    }
}
