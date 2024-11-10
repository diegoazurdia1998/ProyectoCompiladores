using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ProyectoConsola.Modelos;

namespace ProyectoConsola.JJ
{
    public class Auxiliar
    {
        public string RealizarOperacion(string valor1, string operador, string valor2){
            if (string.IsNullOrEmpty(valor1) && string.IsNullOrEmpty(valor2)) return "noOperacion";

            switch (operador)
            {
                case "+":
                    return (int.Parse(valor1) + int.Parse(valor2)).ToString();
                break;
                case "*":
                    return (int.Parse(valor1) * int.Parse(valor2)).ToString();
                default:
                    return "noOperacion";
                break;   
            }
        }

        public Stack<string> ConvertirReduccionEnPila(string reduccion){
            Stack<string> pilaDeReduccion = new();
            string[] split = reduccion.Split(" ");
            foreach (var item in split)
            {
                pilaDeReduccion.Push(item);
            }
            return pilaDeReduccion;
        }   
    }
}