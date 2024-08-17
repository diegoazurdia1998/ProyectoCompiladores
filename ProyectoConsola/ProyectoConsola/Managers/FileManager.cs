using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProyectoConsola.FilesManagers
{
    internal class FileManager
    {
        Dictionary<string, List<string>> SECCIONES;

        /**
     * Procesa un archivo de texto y devuelve una lista de listas con las secciones del archivo procesadas.
     *
     * @param rutaArchivo La ruta del archivo de texto a procesar.
     * @return Una lista de listas con las secciones del archivo procesadas.
     */
        public Dictionary<string, List<string>> ProcesarArchivo(string rutaArchivo)
        {
            List<List<string>> seccionesProcesadas = new List<List<string>>();

            try
            {
                using (StreamReader reader = new StreamReader(rutaArchivo))
                {
                    List<string> seccionActual = new List<string>();
                    string lineaActual;
                    string patron = @"^\s*(UNITS|SETS|TOKENS|KEYWORDS|PRODUCTIONS)\s*";

                    while ((lineaActual = reader.ReadLine()) != null)
                    {
                        
                        if (Regex.IsMatch(lineaActual, patron))
                        {
                            if (seccionActual.Count > 0)
                            {
                                seccionesProcesadas.Add(seccionActual);
                                seccionActual = new List<string>();
                                seccionActual.Add(lineaActual.Trim());
                            }
                        }
                        else
                        {
                            seccionActual.Add(lineaActual.Trim());
                        }
                    }

                    // Agregar la última sección si no es vacía
                    if (seccionActual.Count > 0)
                    {
                        seccionesProcesadas.Add(seccionActual);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("El archivo no existe o no se puede leer: " + rutaArchivo);
            }

            //seccionesProcesadas = ProcesarSecciones(seccionesProcesadas);

            return ProcesarSecciones(seccionesProcesadas);
        }

        public Dictionary<string, List<string>> ProcesarSecciones(List<List<string>> secciones)
        {
            Dictionary<string, List<string>> seccionesProcesadas = new Dictionary<string, List<string>>();
            List<string> seccionActual = new List<string>();
            string patron = @"^\s*(COMPILER|UNITS|SETS|TOKENS|KEYWORDS|PRODUCTIONS)\s*", seccionString = "";

            foreach (List<string> seccion in secciones)
            {
                foreach(string linea in seccion)
                {
                    if(!Regex.IsMatch(linea, patron))
                    {
                        seccionesProcesadas[seccionString].Add(linea);
                    }
                    else
                    {
                        seccionesProcesadas.Add(linea, new List<string>());
                        seccionString = linea;
                    }                    
                }
            }
            SECCIONES = seccionesProcesadas;
            return seccionesProcesadas;
        }
    }
}
