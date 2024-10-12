// See https://aka.ms/new-console-template for more information
<<<<<<< HEAD
using ProyectoConsola.FilesManagers;
using ProyectoConsola.Managers;

FileManager fileManager = new FileManager();
KeyWordsManager keywordsManager = new();

Dictionary<string, List<string>> seccionesProcesadas = fileManager.ProcesarArchivo("GRMAR.txt"); // \Proyecto\ProyectoConsola\ProyectoConsola\bin\Debug\net8.0\GRMAR.txt
string respuestaKeyWords = keywordsManager.ValidarKeywords(seccionesProcesadas);

//Imprimir secciones
foreach (var seccion in seccionesProcesadas)
{
    Console.WriteLine($"Sección: {seccion.Key}");
    foreach (var valor in seccion.Value)
    {
        Console.WriteLine($"  - {valor}");
    }
}
=======
using ProyectoConsola.Managers;
using System.Drawing;

/// <summary>
/// Clase principal del programa.
/// </summary>
class Program
{
    /// <summary>
    /// Punto de entrada principal para la aplicación.
    /// </summary>
    static void Main()
    {
        FileManager fileManager = new FileManager();

        while (true)
        {
            Console.WriteLine("Ingrese la ruta del archivo (o 'salir' para finalizar):");
            string filePath = Console.ReadLine();

            if (filePath.ToLower().Equals("salir"))
                break;

            try
            {
                // Seccionar el archivo ingresado
                Dictionary<string, List<string>> seccionesProcesadas = fileManager.SeccionarArchivo(filePath);
                // Verificaar e identificar las secciones
                SectionsManager sectionManager = new SectionsManager(seccionesProcesadas);
                // Mostrar secciones
                sectionManager.PrintSections();
                // Construir la tabla de Nullable, First y Follow
                //NFFTableManager nFFTableManager = new NFFTableManager(sm);
                // Mostrar tabla de Nullable, First y Follow
                //nFFTableManager.PrintTables();
                // Calcular la tabla de estados y actions
                LALRTableManager lALRTableManager = new LALRTableManager(sectionManager);
                // Mostrar tabla de estados y actions
                //lALRTableManager.PrintStateTable();
                Console.WriteLine("Mama");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}

<<<<<<< HEAD
LALRTableManager lALRTableManager = new LALRTableManager(nFFTableManager, sm);
//lALRTableManager.PrintStateTable();
>>>>>>> diego
=======
>>>>>>> diego
