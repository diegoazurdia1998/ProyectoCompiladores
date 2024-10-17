using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    public class VerifyInputStackItem
    {
        private readonly int STATE = 0;
        private readonly int INPUT_SYMBOL = 1;
         public string _symbol {  get; set; }
        public object _value {  get; set; }
        public int _type { get; set; }
        public VerifyInputStackItem(string symbol)
        {
            _value = "";
            _symbol = symbol;
            if(int.TryParse(symbol, out int result))
            {
                _type = STATE;
            }
            else
            {
                _type = INPUT_SYMBOL;
            }
        }

        public void SetValue(object value)
        {
            _value = value;
        }
        public object GetValue()
        {
            return _value;
        }
    }
}
