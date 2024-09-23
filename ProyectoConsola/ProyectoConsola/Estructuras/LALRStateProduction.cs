using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    public class LALRStateProduction
    {
        public int _actualIndex { get; set; }
        public string _identifier { get; set; }
        public string _production { get; set; }
        public List<string> _lookahead {  get; set; }

        public LALRStateProduction(int index, string identifier, string productions, List<string> lookahead)
        {
            _identifier = identifier;
            _actualIndex = index;
            _production = productions;
            this._lookahead = lookahead;
        }
        public void AddLookahead(string looakahead)
        {
            this._lookahead.Add(looakahead);
        }
        public bool EqualsStateProduction(LALRStateProduction stateProduction)
        {
            if(_production.Equals(stateProduction._production) && _actualIndex.Equals(stateProduction._actualIndex))
            {
                return true;
            } 
            else return false;
        }
        public bool EqualsOldStateProduction(LALRStateProduction oldStateProduction)
        {
            if (_production.Equals(oldStateProduction._production) && (_actualIndex - 1).Equals(oldStateProduction._actualIndex))
            {
                return true;
            }
            else return false;
        }

    }


}
