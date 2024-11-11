// See https://aka.ms/new-console-template for more information
using ProyectoConsola.JJ;
using ProyectoConsola.Managers;
using ProyectoConsola.Modelos;
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
        //CodigoTresDirecciones codigoTresDirecciones = new();
        //RespuestaModel respuesta = codigoTresDirecciones.ValidacionDeLaPruebaConGramatica();
        //Console.WriteLine(respuesta.Mensaje);
        //Environment.Exit(0);

        ParceoDeTabla parceoDeTabla = new();
        parceoDeTabla.Parsear(Ejemplo1.ListadoDeAcciones(), Ejemplo1.ListadoPrueba(), Ejemplo1.ListadoDeReducciones());

        FileManager fileManager = new FileManager();




        while (true)
        {
            Console.WriteLine("Ingrese la ruta del archivo (o 'salir' para finalizar):");
            //string filePath = Console.ReadLine();
            string filePath = "../../gramaticaReducida.txt";

            if (filePath.ToLower().Equals("salir"))
                break;

            try
            {
                // Seccionar el archivo ingresado
                Dictionary<string, List<string>> seccionesProcesadas = fileManager.SeccionarArchivo(filePath);
                // Verificaar e identificar las secciones
                SectionsManager sectionManager = new(seccionesProcesadas);
                //sectionManager.ExportNonTerminalsToExcel("");
                // Construir la tabla de Nullable, First y Follow
                NFFTableManager nFFTableManager = new(sectionManager);
                
                //leer el archivo de prueba
                LeerArchivoDePrueba leerArchivoDePrueba = new();
                List<string> archivoDePrueba = leerArchivoDePrueba.Leer("../../prueba1.txt");
                
                // Calcular la tabla de estados y actions
                LALRTableManager lALRTableManager = new(sectionManager, nFFTableManager, archivoDePrueba);

                
                
                //LALRParser lALRParser = new(lALRTableManager, sectionManager);
                /*
                string input = Console.ReadLine();
                if (input != null && input.Length > 0)
                    lALRParser.Parse(input);
                else
                 */
                //lALRParser.Parse("../../prueba1.txt");
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;
                Console.WriteLine("Error: " + mensaje);
                Console.WriteLine("\nPresione cualquier tecla para continuar");
                Console.ReadLine();
            }
        }
    }
}

