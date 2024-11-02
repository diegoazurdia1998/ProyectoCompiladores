using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    public class State
    {
        public int Index { get; set; }
        public List<Item> Items { get; set; }
        public State(int index, List<Item> items)
        {
            Index = index;
            Items = items;
        }
        public override bool Equals(object obj)
        {
            if (obj is State other)
            {
                bool b1 = Index.Equals(other.Index),
                    b2 = Items.Count.Equals(other.Items.Count);
                if (b2)
                {
                    for(int i = 0; i < Items.Count; i++)
                    {
                        if (!Items[i].Equals(other.Items[i]))
                        {
                            b2 = false;
                        }
                    }
                }
                return b1 && b2; ;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, Items);
        }
    }
}
