<<<<<<< HEAD
﻿using System;
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
=======
﻿using System.Text.RegularExpressions;

/// <summary>
/// Clase FileManager que se encarga de procesar archivos de texto y extraer secciones.
/// </summary>
public class FileManager
{
    // Atributo que almacena las secciones procesadas del archivo.
    private Dictionary<string, List<string>> SECCIONES = new Dictionary<string, List<string>>();

    /// <summary>
    /// Procesa un archivo de texto y devuelve una lista de listas con las secciones del archivo procesadas.
    /// </summary>
    /// <param name="rutaArchivo">La ruta del archivo de texto a procesar.</param>
    /// <returns>Una lista de listas con las secciones del archivo procesadas.</returns>
    public Dictionary<string, List<string>> SeccionarArchivo(string rutaArchivo)
    {
        // Inicializa una lista vacía para almacenar las secciones procesadas.
        List<List<string>> seccionesProcesadas = new List<List<string>>();
        bool isEmpty = false;
        try
        {
            // Abre el archivo de texto en modo lectura.
            using (StreamReader reader = new StreamReader(rutaArchivo))
            {
                // Inicializa una lista vacía para almacenar la sección actual.
                List<string> seccionActual = new List<string>();
                // Patrón regular para identificar las secciones del archivo.
                string patron = @"^\s*(COMPILER|UNITS|SETS|TOKENS|KEYWORDS|PRODUCTIONS)\s*";

                // Lee el archivo línea por línea.
                string lineaActual;
                while ((lineaActual = reader.ReadLine()) != null)
                {
                    // Verificar si la línea actual es nula
                    if (lineaActual == null)
                    {
                        continue;
                    }
                    // Verifica si la línea actual es una sección.
                    if (Regex.IsMatch(lineaActual, patron))
                    {

                        // Si la sección actual no está vacía, la agrega a la lista de secciones procesadas.
                        if (seccionActual.Count > 1 || isEmpty)
                        {
                            if (!isEmpty) seccionesProcesadas.Add(seccionActual);
                            // Reinicia la sección actual.
                            seccionActual = new List<string>();

                        }
                        else
                        {
                            isEmpty = true;
                        }
                        // Agrega EL TITULO a la sección actual.
                        seccionActual.Add(lineaActual.Trim());
                    }
                    else
                    {
                        isEmpty = false;
                        // Agrega la línea actual a la sección actual.
                        seccionActual.Add(lineaActual.Trim());
                    }
                }

                // Agrega la última sección si no es vacía.
                if (seccionActual.Count > 1)
                {
                    seccionesProcesadas.Add(seccionActual);
                }
            }
        }
        catch (FileNotFoundException)
        {
            // Maneja la excepción si el archivo no existe o no se puede leer.
            Console.Error.WriteLine("El archivo no existe o no se puede leer: " + rutaArchivo);
        }

        // Procesa las secciones extraídas del archivo.
        return ProcesarSecciones(seccionesProcesadas);
    }

    /// <summary>
    /// Procesa las secciones extraídas del archivo y las almacena en un diccionario.
    /// </summary>
    /// <param name="secciones">La lista de secciones extraídas del archivo.</param>
    /// <returns>Un diccionario con las secciones procesadas.</returns>
    public Dictionary<string, List<string>> ProcesarSecciones(List<List<string>> secciones)
    {
        // Inicializa un diccionario vacío para almacenar las secciones procesadas.
        Dictionary<string, List<string>> seccionesProcesadas = new Dictionary<string, List<string>>();
        // Inicializa una lista vacía para almacenar la sección actual.
        List<string> seccionActual = new List<string>();
        // Patrón regular para identificar las secciones del archivo.
        string patron = @"^\s*(COMPILER|UNITS|SETS|TOKENS|KEYWORDS|PRODUCTIONS)\s*", seccionString = "";

        // Recorre las secciones extraídas del archivo.
        foreach (List<string> seccion in secciones)
        {
            // Recorre las líneas de la sección actual.
            foreach (string linea in seccion)
            {
                // Verifica si la línea actual es una sección.
                if (!Regex.IsMatch(linea, patron))
                {
                    // Agrega la línea actual a la sección actual en el diccionario.
                    seccionesProcesadas[seccionString].Add(linea);
                }
                else
                {
                    // Agrega la sección actual al diccionario.
                    seccionesProcesadas.Add(linea, new List<string>());
                    // Actualiza la sección actual.
                    seccionString = linea;
                }
            }
        }

        // Almacena las secciones procesadas en el atributo SECCIONES.
        SECCIONES = seccionesProcesadas;
        // Retorna las secciones procesadas.
        return seccionesProcesadas;
>>>>>>> diego
    }
}
