// See https://aka.ms/new-console-template for more information
using ProyectoConsola.Managers;
using ProyectoConsola.Parsing;
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
            //string filePath = Console.ReadLine();
            string filePath = "GRMAR3.txt";

            if (filePath.ToLower().Equals("salir"))
                break;

            try
            {
                // Seccionar el archivo ingresado
                Dictionary<string, List<string>> seccionesProcesadas = fileManager.SeccionarArchivo(filePath);
                // Verificaar e identificar las secciones
                SectionsManager sectionManager = new(seccionesProcesadas);
                sectionManager.ExportNonTerminalsToExcel("");
                // Construir la tabla de Nullable, First y Follow
                NFFTableManager nFFTableManager = new(sectionManager);
                // Calcular la tabla de estados y actions
                LALRTableManager lALRTableManager = new(sectionManager, nFFTableManager);
                LALRParser lALRParser = new(lALRTableManager, sectionManager);
                /*
                string input = Console.ReadLine();
                if (input != null && input.Length > 0)
                    lALRParser.Parse(input);
                else
                 */
                lALRParser.Parse("prueba1.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("\nPresione cualquier tecla para continuar");
                Console.ReadLine();
            }
        }
    }
}

