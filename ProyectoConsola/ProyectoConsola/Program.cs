// See https://aka.ms/new-console-template for more information
using ProyectoConsola.Managers;
using System.Drawing;

class Program
{
    static void Main(string[] args)
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
                SectionsManager sm = new SectionsManager(seccionesProcesadas);
                // Mostrar secciones
                sm.PrintSections();
                // Construir la tabla de Nullable, First y Follow
                NFFTableManager nFFTableManager = new NFFTableManager(sm);
                // Mostrar tabla de Nullable, First y Follow
                nFFTableManager.PrintTables();
                // Calcular la tabla de estados y actions
                LALRTableManager lALRTableManager = new LALRTableManager(nFFTableManager, sm);                
                // Mostrar tabla de estados y actions
                //lALRTableManager.PrintStateTable();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}

