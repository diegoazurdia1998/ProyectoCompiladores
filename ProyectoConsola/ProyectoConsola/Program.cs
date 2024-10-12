// See https://aka.ms/new-console-template for more information
using ProyectoConsola.FilesManagers;
using ProyectoConsola.Managers;
<<<<<<< Updated upstream
=======
using System.Drawing;
>>>>>>> Stashed changes

FileManager fileManager = new FileManager();
KeyWordsManager keywordsManager = new();

Dictionary<string, List<string>> seccionesProcesadas = fileManager.ProcesarArchivo("GRMAR.txt"); // \Proyecto\ProyectoConsola\ProyectoConsola\bin\Debug\net8.0\GRMAR.txt
<<<<<<< Updated upstream
string respuestaKeyWords = keywordsManager.ValidarKeywords(seccionesProcesadas);

=======

ProductionsManager productionsManager = new ProductionsManager(seccionesProcesadas["PRODUCTIONS"]);
bool productions = productionsManager.ValidateProductions();
>>>>>>> Stashed changes
//Imprimir secciones
foreach (var seccion in seccionesProcesadas)
{
    Console.WriteLine($"Sección: {seccion.Key}");
    foreach (var valor in seccion.Value)
    {
        Console.WriteLine($"  - {valor}");
    }
}
Console.WriteLine($" PRODUCTIONS - {productions}");
