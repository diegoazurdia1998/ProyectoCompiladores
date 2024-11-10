using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml.ConditionalFormatting;
using ProyectoConsola.Modelos;

namespace ProyectoConsola.JJ
{
    public class ParceoDeTabla
    {
        public Stack<IdentificadorValorModel> pila  = new(); 
        public List<AccionModel> listadoDeAcciones = new();
        List<ReduccionModel>  listadoDeReducciones = new();
        public ParceoDeTabla(){
            pila.Push(new IdentificadorValorModel() {Id = "0"});
        }

        public void Parsear(List<AccionModel> listadoAcciones, List<IdentificadorValorModel> archivoDePrueba, List<ReduccionModel> listadoReducciones){
            this.listadoDeAcciones = listadoAcciones; 
            this.listadoDeReducciones = listadoReducciones;
            IdentificadorValorModel itemInput = archivoDePrueba[0];
            archivoDePrueba.RemoveAt(0);
            Stack<IdentificadorValorModel> pilaTemporal = new();
            bool respuesta = false;
            IdentificadorValorModel itemPila = pila.Peek();
            
            while (archivoDePrueba.Count > 0)
            {
                respuesta = ProcesarCaracter(itemPila, itemInput);

                if (respuesta){
                    itemInput = archivoDePrueba[0];
                    archivoDePrueba.RemoveAt(0);
                    itemPila = pila.Peek();
                }else{
                    itemPila = pila.Peek();
                }
            }

            Console.WriteLine("Finalizado correctamente");
        }

        public bool ProcesarCaracter(IdentificadorValorModel itemPila, IdentificadorValorModel itemInput){
            foreach (var accion in listadoDeAcciones)
            {
                if (accion.Fila!.ToString().Equals(itemPila.Id) && ( accion.Columna!.Equals(itemInput.Id) || accion.Columna.Equals(itemInput.Valor))){
                    string queAccion = accion.Accion ?? "NA";
                    string queEstado = accion.AQueEstado!.ToString();

                    switch (queAccion)
                    {
                        case "Shift":
                            pila.Push(itemInput);
                            pila.Push(new IdentificadorValorModel(){ Id = queEstado});
                            return true;
                        break;
                        case "Goto":
                            pila.Push(new IdentificadorValorModel(){ Id = queEstado});
                            return false;
                        break;
                        case "Reduce":
                            RealizarReduce(queEstado);
                            RealizarGoto();
                            return false;
                        break;
                        case "OK":
                        break;
                        default: 
                            Console.Write("No se encontro accion");
                            return false;
                            break;
                    }
                    break;
                }
            }
            return false;
        }

        public void RealizarGoto(){
            IdentificadorValorModel item1 = pila.Pop();
            IdentificadorValorModel item2 = pila.Pop();

            pila.Push(item2);
            pila.Push(item1);

            ProcesarCaracter(item2, item1);
        }

        public void RealizarReduce(string reducePor){
            pila.Pop();
            IdentificadorValorModel itemPila = pila.Peek();
            Auxiliar auxiliar = new();                                        
            Stack<string> pilaDeReduccion = [];
            bool tieneOperacion = false;

            foreach (ReduccionModel reduccion in listadoDeReducciones)
            {   
                if (reducePor.Equals(reduccion.Estado)){
                    pilaDeReduccion = auxiliar.ConvertirReduccionEnPila(reduccion.Produccion!);
                    tieneOperacion = reduccion.TieneOperacion;
                    break;
                }
            }
            
            if (tieneOperacion){
                //copia de la pila original
                Stack<IdentificadorValorModel> pilaCopia = new(new Stack<IdentificadorValorModel>(pila));
                List<IdentificadorValorModel> ListadoDeLaOperacionArealizar = new();
                IdentificadorValorModel itemPilaCopia = pilaCopia.Pop();
                int contadorDePops = 0;
                string itemReduccion = pilaDeReduccion.Pop();

                while (pilaCopia.Count > 0)
                {
                    if (itemPilaCopia.Id == itemReduccion)
                    {
                        itemReduccion = pilaDeReduccion.Pop();
                        ListadoDeLaOperacionArealizar.Add(itemPilaCopia);
                    }
                    itemPilaCopia = pilaCopia.Pop();
                    contadorDePops++;

                    if (itemReduccion.Equals("=")){
                        string resultadoOperacion = auxiliar.RealizarOperacion(ListadoDeLaOperacionArealizar[0].Valor!, ListadoDeLaOperacionArealizar[1].Id!, ListadoDeLaOperacionArealizar[2].Valor!);
                        for (int i = 0; i < contadorDePops; i++)
                        {
                            pila.Pop();
                        }

                        if (resultadoOperacion.Equals("noOperacion")){
                            pila.Push(new IdentificadorValorModel(){ Id = pilaDeReduccion.Pop(), Valor = ListadoDeLaOperacionArealizar[1].Valor});
                        }else{
                            pila.Push(new IdentificadorValorModel(){ Id = pilaDeReduccion.Pop(), Valor = resultadoOperacion});
                        }
                        return;
                    }
                }
            }else{
                try
                {
                    List<IdentificadorValorModel> listaTemporalParaPushearAPila = new();
                    for (int i = 0; i <= pilaDeReduccion.Count ; i++)
                    {
                        string itemPilaReduce = pilaDeReduccion.Pop();
                        if (itemPilaReduce.Equals(itemPila.Id) || itemPilaReduce.Equals(itemPila.Valor))
                        {
                            listaTemporalParaPushearAPila.Add(pila.Pop());
                        }

                        if (itemPilaReduce == "=")
                        {
                            string ultimoItemPilaReduce = pilaDeReduccion.Pop();
                            pila.Push(new IdentificadorValorModel(){ Id = ultimoItemPilaReduce, Valor = listaTemporalParaPushearAPila[0].Valor!});
                        }
                    }
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Error con la produccion de los reduce, verifique que los terminales tengan espacio de los no terminales");
                    throw;
                }
                
            }
        }
    }
}