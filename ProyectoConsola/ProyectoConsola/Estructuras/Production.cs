using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoConsola.Estructuras
{
    public class Production
    {
        public string Left { get; set; }
        public List<string> Right { get; set; }
        public int CurrentSymbolIndex { get; set; }

        public Production(string identifer, string production, int index)
        {
            Left = identifer;
            Right = production.Split(' ').ToList();
            CurrentSymbolIndex = index;
        }
        public int RightCount()
        {
            return Right.Count;
        }
        public bool IsAtEnd()
        {
            return CurrentSymbolIndex >= Right.Count; // Verifica si se ha llegado al final
        }
        public string CurrentSymbol()
        {
            return IsAtEnd() ? null : Right[CurrentSymbolIndex]; // Devuelve el símbolo actual
        }
        public Production IncreaseIndex()
        {
            CurrentSymbolIndex++;
            return this;
        }
        public override bool Equals(object obj)
        {
            if (obj is Production other)
            {
                bool b1 = Left.Equals(other.Left),
                    b2 = Right.SequenceEqual(other.Right),
                    b3 = CurrentSymbolIndex == other.CurrentSymbolIndex;
                return b1 && b2 && b3;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Right, CurrentSymbolIndex);
        }
        public Production Clone()
        {
            var clone = (Production)this.MemberwiseClone();
            clone.Left = Left;
            clone.Right = Right;
            clone.CurrentSymbolIndex = CurrentSymbolIndex;
            return clone;
        }
    }
}
