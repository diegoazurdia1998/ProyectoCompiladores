using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    /// <summary>
    /// Objeto que se guarda en el stack para verificar una cadena de entrada
    /// </summary>
    public class InputStackItem
    {
        public object _symbol {  get; set; }
        public object _value {  get; set; }
        public int _type { get; set; }
        
        public InputStackItem(object symbol, int type)
        {
            _type = type;
            _symbol = symbol;
        }
        
        public InputStackItem(object symbol, int type, object value)
        {
            _type = type;
            _symbol = symbol;
            _value = value;
        }

        public int GetIntValueForSymbol()
        {
            if(int.TryParse(_symbol.ToString(), out int result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }
        
        public string GetStringValueForSymbol()
        {
            if(_symbol != null) 
                return _symbol.ToString();
            else
                return string.Empty;
        }
    }
}
