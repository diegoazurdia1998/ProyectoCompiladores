using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    public class LALRState
    {
        public int _actualIndex { get; set; }
        public string _identifier { get; set; }
        public string _productions { get; set; }
        public List<string> _lookahead {  get; set; }

        public LALRState(int index, string identifier, string productions, List<string> lookahead)
        {
            _identifier = identifier;
            _actualIndex = index;
            _productions = productions;
            this._lookahead = lookahead;
        }
        public void AddLookahead(string looakahead)
        {
            this._lookahead.Add(looakahead);
        }
      
    }


}
