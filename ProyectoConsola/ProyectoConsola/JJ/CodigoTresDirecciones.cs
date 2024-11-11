using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProyectoConsola.Modelos;

namespace ProyectoConsola.JJ
{
    public class CodigoTresDirecciones
    {
        string stringDePrueba;
        Stack<string> pilaDePrueba = new();
        List<IdentificadorValorModel> listadoDeProducciones = new();
        string itemPrueba;
        RespuestaModel proyectoFinalizado = new(){ Codigo = 0, Mensaje = "Prueba no válido"};


        public CodigoTresDirecciones(){
            listadoDeProducciones = Ejemplo1.ListadoDeProducciones();

            using (StreamReader reader = new StreamReader("../../prueba1.txt"))
            {
                stringDePrueba = reader.ReadLine();
                string[] arregloDePrueba = stringDePrueba.Split(" ");
                pilaDePrueba = new(new Stack<string>(arregloDePrueba));
            }
        }

        public RespuestaModel ValidacionDeLaPruebaConGramatica(){
            if (string.IsNullOrEmpty(stringDePrueba))
            {
                Console.WriteLine("Archivo no valido");
            }
            
            string[] ArregloDepalabrasDePrueba = stringDePrueba.Split(" ");

            for (int i = 0; i < pilaDePrueba.Count; i++)
            {
                itemPrueba = pilaDePrueba.Pop();
                RespuestaModel respuesta = ValidarCaracterConGramarica("<program>");
            }

            return proyectoFinalizado;
        }

        private RespuestaModel ValidarCaracterConGramarica(string produccion){
            List<IdentificadorValorModel> vProduccion = listadoDeProducciones.Where(p => p.Id == produccion).ToList();
            
            foreach (IdentificadorValorModel rProduccion in vProduccion)
            {
                string[] valorProduccion = rProduccion.Valor!.Split(" ");
                
                foreach (string itemProduccion in valorProduccion)
                {
                    //terminal 
                    if (itemProduccion.StartsWith("'") && itemProduccion.EndsWith("'"))
                    {
                        if(itemProduccion.Equals("'"+itemPrueba+"'")){
                            if (pilaDePrueba.Count == 0)
                            {
                                proyectoFinalizado.Codigo = 1;
                                proyectoFinalizado.Mensaje = "Prueba válida";
                            }else{
                                itemPrueba = pilaDePrueba.Pop();
                                continue;
                            }
                        }else{
                            return new RespuestaModel(){ Codigo = 0, Mensaje = "Terminal no válido: " + itemPrueba };
                        };
                    }

                    //variable 
                    if (itemProduccion.StartsWith('<') && itemProduccion.EndsWith('>'))
                    {
                        RespuestaModel respuesta = ValidarCaracterConGramarica(itemProduccion); 
                        if(respuesta.Codigo == 1){
                            return new RespuestaModel(){ Codigo = 1, Mensaje = "Caracter válido" };
                        }
                        if (respuesta.Codigo == 0 || respuesta.Codigo == 3)
                        {
                            continue;
                        }
                    }

                    //es epsilon
                    if (itemProduccion.Equals("ε"))
                    {
                        return new RespuestaModel(){Codigo = 3};
                    }

                    //es keyword
                    if (Ejemplo1.ListadoDeTerminales().Contains(itemPrueba))
                    {
                        continue;
                    }
                    
                    //validar si es numero
                    int num;
                    bool resp = int.TryParse(itemPrueba, out num);
                    if (resp)
                    {
                        itemPrueba = pilaDePrueba.Pop();
                        return new RespuestaModel(){ Codigo = 3 };
                    }

                    //validar si es un identificador
                    RespuestaModel resp2 = ValidarIdentificador(itemPrueba);
                    if (resp2.Codigo == 1) 
                    {
                        itemPrueba = pilaDePrueba.Pop();
                        continue;
                    }else{
                        return resp2;
                    }
                    
                }

                if(proyectoFinalizado.Codigo == 1){
                    break;
                }
            }


            return new RespuestaModel(){ Codigo = 0 };
        }

        private RespuestaModel ValidarIdentificador(string identificador){
            bool respuesta = Regex.IsMatch(identificador, "[a-zA-Z0-9]");
            if (respuesta)
            {
                return new RespuestaModel(){ Codigo = 1 };
            }else{
                return new RespuestaModel(){ Codigo = 0};
            }
        }
        
    }
}