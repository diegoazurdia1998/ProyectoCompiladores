using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoConsola.Modelos;

namespace ProyectoConsola.JJ
{
    public class Ejemplo1
    { 
        static public List<string> ListadoDeTerminales(){
            return ["PROGRAM", "VAR", "BEGIN", "END", "INTEGER"];
        }

        static public List<IdentificadorValorModel> ListadoDeProducciones(){
            return [
                new IdentificadorValorModel(){ Id = "<program>", Valor = "'PROGRAM' identifier ';' <declarations> <compound_statement> '.' ;" },
                new IdentificadorValorModel(){ Id = "<declarations>", Valor = "'VAR' identifier ':' 'INTEGER' ';' <declarations>" },
                new IdentificadorValorModel(){ Id = "<declarations>", Valor = "ε ;" },
                new IdentificadorValorModel(){ Id = "<compound_statement>", Valor = "'BEGIN' <statement_list> 'END' ;" },
                new IdentificadorValorModel(){ Id = "<statement_list>", Valor = "<assignment>" },
                new IdentificadorValorModel(){ Id = "<statement_list>", Valor = "<assignment> ';' <statement_list> ;" },
                new IdentificadorValorModel(){ Id = "<assignment>", Valor = "identifier ':=' <expression> ;" },
                new IdentificadorValorModel(){ Id = "<expression>", Valor = "<term> '+' <term>" },
                new IdentificadorValorModel(){ Id = "<expression>", Valor = "<term> ;" },
                new IdentificadorValorModel(){ Id = "<term>", Valor = "<factor> '*' <factor>" },
                new IdentificadorValorModel(){ Id = "<term>", Valor = "<factor> ;" },
                new IdentificadorValorModel(){ Id = "<factor>", Valor = "identifier" },
                new IdentificadorValorModel(){ Id = "<factor>", Valor = "number" },
            ];
        }



        static public List<AccionModel> ListadoDeAcciones(){
            return [
                new(){ Fila = "0", Columna = "num", Accion = "Shift", AQueEstado = "5" },
                new(){ Fila = "0", Columna = "(", Accion = "Shift", AQueEstado = "4" },
                new(){ Fila = "0", Columna = "<E>", Accion = "Goto", AQueEstado = "1" },
                new(){ Fila = "0", Columna = "<T>", Accion = "Goto", AQueEstado = "2" },
                new(){ Fila = "0", Columna = "<F>", Accion = "Goto", AQueEstado = "3" },

                new(){ Fila = "1", Columna = "+", Accion = "Shift", AQueEstado = "6" },
                new(){ Fila = "1", Columna = "$", Accion = "OK", AQueEstado = "" },

                new(){ Fila = "2", Columna = ")", Accion = "Reduce", AQueEstado = "2" },
                new(){ Fila = "2", Columna = "+", Accion = "Reduce", AQueEstado = "2" },
                new(){ Fila = "2", Columna = "*", Accion = "Shift", AQueEstado = "7" },
                new(){ Fila = "2", Columna = "$", Accion = "Reduce", AQueEstado = "2" },

                new(){ Fila = "3", Columna = ")", Accion = "Reduce", AQueEstado = "4" },
                new(){ Fila = "3", Columna = "+", Accion = "Reduce", AQueEstado = "4" },
                new(){ Fila = "3", Columna = "*", Accion = "Reduce", AQueEstado = "4" },
                new(){ Fila = "3", Columna = "$", Accion = "Reduce", AQueEstado = "4" },

                new(){ Fila = "4", Columna = "num", Accion = "Shift", AQueEstado = "5" },
                new(){ Fila = "4", Columna = "(", Accion = "Shift", AQueEstado = "4" },
                new(){ Fila = "4", Columna = "<E>", Accion = "Goto", AQueEstado = "8" },
                new(){ Fila = "4", Columna = "<T>", Accion = "Goto", AQueEstado = "2" },
                new(){ Fila = "4", Columna = "<F>", Accion = "Goto", AQueEstado = "3" },

                new(){ Fila = "5", Columna = ")", Accion = "Reduce", AQueEstado = "6" },
                new(){ Fila = "5", Columna = "+", Accion = "Reduce", AQueEstado = "6" },
                new(){ Fila = "5", Columna = "*", Accion = "Reduce", AQueEstado = "6" },
                new(){ Fila = "5", Columna = "$", Accion = "Reduce", AQueEstado = "6" },

                new(){ Fila = "6", Columna = "num", Accion = "Shift", AQueEstado = "5" },
                new(){ Fila = "6", Columna = "(", Accion = "Shift", AQueEstado = "4" },
                new(){ Fila = "6", Columna = "<T>", Accion = "Goto", AQueEstado = "9" },
                new(){ Fila = "6", Columna = "<F>", Accion = "Goto", AQueEstado = "3" },

                new(){ Fila = "7", Columna = "num", Accion = "Shift", AQueEstado = "5" },
                new(){ Fila = "7", Columna = "(", Accion = "Shift", AQueEstado = "4" },
                new(){ Fila = "7", Columna = "<F>", Accion = "Goto", AQueEstado = "10" },
                
                new(){ Fila = "8", Columna = ")", Accion = "Shift", AQueEstado = "11" },
                new(){ Fila = "8", Columna = "+", Accion = "Shift", AQueEstado = "6" },
                
                new(){ Fila = "9", Columna = ")", Accion = "Reduce", AQueEstado = "1" },
                new(){ Fila = "9", Columna = "+", Accion = "Reduce", AQueEstado = "1" },
                new(){ Fila = "9", Columna = "*", Accion = "Shift", AQueEstado = "7" },
                new(){ Fila = "9", Columna = "$", Accion = "Reduce", AQueEstado = "1" },
                
                new(){ Fila = "10", Columna = ")", Accion = "Reduce", AQueEstado = "3" },
                new(){ Fila = "10", Columna = "+", Accion = "Reduce", AQueEstado = "3" },
                new(){ Fila = "10", Columna = "*", Accion = "Reduce", AQueEstado = "3" },
                new(){ Fila = "10", Columna = "$", Accion = "Reduce", AQueEstado = "3" },

                new(){ Fila = "11", Columna = "+", Accion = "Reduce", AQueEstado = "5" },
                new(){ Fila = "11", Columna = "*", Accion = "Reduce", AQueEstado = "5" },
                new(){ Fila = "11", Columna = "$", Accion = "Reduce", AQueEstado = "5" },

            ];
        }

        //No Aceptado, se elimino el ultimo parentesis
        // static public List<IdentificadorValorModel> ListadoPrueba(){
        //     return [
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "5" },
        //         new IdentificadorValorModel(){ Id = "+" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "7" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "4" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "2" },
        //         new IdentificadorValorModel(){ Id = "$" },
        //     ];
        // }

        //No Aceptado se eliminio el primer valor de 5
        // static public List<IdentificadorValorModel> ListadoPrueba(){
        //     return [
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "+" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "7" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "4" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "2" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "$" },
        //     ];
        // }

        static public List<IdentificadorValorModel> ListadoPrueba(){
            return [
                new IdentificadorValorModel(){ Id = "num", Valor = "2" },
                new IdentificadorValorModel(){ Id = "+" },
                new IdentificadorValorModel(){ Id = "num", Valor = "3" },
                new IdentificadorValorModel(){ Id = "*" },
                new IdentificadorValorModel(){ Id = "num", Valor = "4" },
            ];
        }

        //Aceptado
        // static public List<IdentificadorValorModel> ListadoPrueba(){
        //     return [
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "5" },
        //         new IdentificadorValorModel(){ Id = "+" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "7" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "4" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "2" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "+" },
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "2" },
        //         new IdentificadorValorModel(){ Id = "+" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "1" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "$" },
        //     ];
        // }

        //Aceptado
        // static public List<IdentificadorValorModel> ListadoPrueba(){
        //     return [
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "5" },
        //         new IdentificadorValorModel(){ Id = "+" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "7" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "(" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "4" },
        //         new IdentificadorValorModel(){ Id = "*" },
        //         new IdentificadorValorModel(){ Id = "num", Valor = "2" },
        //         new IdentificadorValorModel(){ Id = ")" },
        //         new IdentificadorValorModel(){ Id = "$" },
        //     ];
        // }

        static public List<ReduccionModel> ListadoDeReducciones(){
            return [
               new ReduccionModel(){ Estado =  "0", Produccion = "S' = <E> $", TieneOperacion = false },
               new ReduccionModel(){ Estado =  "1", Produccion = "<E> = <E> + <T>", TieneOperacion = true },
               new ReduccionModel(){ Estado =  "2", Produccion = "<E> = <T>", TieneOperacion = false },
               new ReduccionModel(){ Estado =  "3", Produccion = "<T> = <T> * <F>", TieneOperacion = true },
               new ReduccionModel(){ Estado =  "4", Produccion = "<T> = <F>", TieneOperacion = false },
               new ReduccionModel(){ Estado =  "5", Produccion = "<F> = ( <E> )", TieneOperacion = true },
               new ReduccionModel(){ Estado =  "6", Produccion = "<F> = num", TieneOperacion = false },
            ];
        }
    }
}