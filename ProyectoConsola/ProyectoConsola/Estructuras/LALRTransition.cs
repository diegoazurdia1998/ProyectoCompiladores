using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    public class LALRTransition
    {
        //SHIFT / GOTO
        public int FromStateId { get; set; }
        public int ToStateId { get; set; }
        public string Symbol { get; set; }
    }
}
