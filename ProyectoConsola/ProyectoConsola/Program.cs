// See https://aka.ms/new-console-template for more information
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
            //string filePath = Console.ReadLine();
            string filePath = "../../GRMAR.txt";

            if (filePath.ToLower().Equals("salir"))
                break;

            try
            {
                // Seccionar el archivo ingresado
                Dictionary<string, List<string>> seccionesProcesadas = fileManager.SeccionarArchivo(filePath);
                // Verificaar e identificar las secciones
                SectionsManager sectionManager = new(seccionesProcesadas);
                // Mostrar secciones
                //sectionManager.PrintSections();
                // Construir la tabla de Nullable, First y Follow
                NFFTableManager nFFTableManager = new(sectionManager);
                // Mostrar tabla de Nullable, First y Follow
                //nFFTableManager.PrintTables();
                // Calcular la tabla de estados y actions
                LALRTableManager lALRTableManager = new(sectionManager, nFFTableManager);
                // Mostrar tabla de estados y actions
                //Console.WriteLine("Exportar Excel?");
                //string op = Console.ReadLine();
                
                //if (op.Equals("t"))
                    lALRTableManager.ExportActionsToExcel("");
                //if (op.Equals("s"))
                    lALRTableManager.ExportStatesToExcel("");

                string filePath2 = "../../prueba1.txt";
                string leido = fileManager.ReadNewFile(filePath2);
                // if (string.IsNullOrEmpty(leido)){
                //     Console.WriteLine("");
                // }else{
                    bool respuesta = lALRTableManager.VerifyInputString(leido);
                    if (respuesta){
                        Console.WriteLine("La cadena es Aceptada");
                    }else{
                         Console.WriteLine("La cadena NO es aceptada");
                    }
                    
                // }

                // Console.WriteLine("Ingrese una cadena para validar");
                // string input = Console.ReadLine();
                // while (!input.Equals("s"))
                // {
                //     lALRTableManager.VerifyInputString(input);
                //     Console.WriteLine("Ingrese una cadena para validar");
                //     input = Console.ReadLine();
                // }
                // Console.WriteLine("Presione cualquier tecla para continuar");
                // filePath = Console.ReadLine();

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

