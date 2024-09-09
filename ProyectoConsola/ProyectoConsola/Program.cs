// See https://aka.ms/new-console-template for more information
using ProyectoConsola.FilesManagers;
using ProyectoConsola.Managers;
using System.Drawing;

FileManager fileManager = new FileManager();

Dictionary<string, List<string>> seccionesProcesadas = fileManager.ProcesarArchivo("GRMAR.txt"); // \Proyecto\ProyectoConsola\ProyectoConsola\bin\Debug\net8.0\GRMAR.txt

SectionsManager sm = new SectionsManager(seccionesProcesadas);
NFFTableManager nFFTableManager = new NFFTableManager(sm);
//Imprimir secciones
foreach (var seccion in seccionesProcesadas)
{
    Console.WriteLine($"Sección: {seccion.Key}");
    foreach (var valor in seccion.Value)
    {
        Console.WriteLine($"  - {valor}");
    }
}
