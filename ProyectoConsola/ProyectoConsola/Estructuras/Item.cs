using ProyectoConsola.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProyectoConsola.Enumeraciones;

namespace ProyectoConsola.Estructuras
{
    public class Item
    {
        public HashSet<string> Lookahead { get; set; }
        public Production ItemProduction { get; set; }
        public int ActionInt { get; set; }
        public LALRAction ItemAction { get; set; }
        public Item(HashSet<string> lookahead, string identifier, string production)
        {
            Lookahead = lookahead;
            ItemProduction = new Production(identifier, production, 0);
            ActionInt = -1;
            ItemAction = LALRAction.Default;
        }
        public void SetActionInt(int actionInt)
        {
            ActionInt = actionInt;
        }
        public void SetAction(LALRAction action)
        {
            ItemAction = action;
        }
        public override bool Equals(object obj)
        {
            if (obj is Item other)
            {
                bool b1 = Lookahead.SetEquals(other.Lookahead),
                    b2 = ItemProduction.Equals(other.ItemProduction),
                    b3 = ActionInt == other.ActionInt,
                    b4 = ItemAction == other.ItemAction;
                return b1 && b2 && b3 && b4;
            }
            return false;
        }
        public bool EqualsWithoutLookahead(object obj)
        {
            if (obj is Item other)
            {
                bool b2 = ItemProduction.Equals(other.ItemProduction),
                     b3 = ActionInt == other.ActionInt,
                     b4 = ItemAction == other.ItemAction;
                return b2 && b3 && b4;
            }
            return false;
        }
        public override int GetHashCode()
        {
            int hashCode = HashCode.Combine(ItemAction, ActionInt, ItemProduction.Left, ItemProduction.Right, ItemProduction.CurrentSymbolIndex);

            foreach (var item in Lookahead)
            {
                hashCode ^= item.GetHashCode(); // Combina los hashes de los elementos de Lookahead
            }

            return hashCode;
        }
        public Item Clone()
        {
            var clone = (Item)this.MemberwiseClone();
            clone.Lookahead = Lookahead;
            clone.ItemProduction = ItemProduction.Clone();
            clone.ActionInt = ActionInt;
            clone.ItemAction = ItemAction;
            return clone;
        }
    }
}
