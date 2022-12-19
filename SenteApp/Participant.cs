using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenteApp
{
    class Participant
    {
        public int Id { get; set; }
        private List<Participant> children = new List<Participant>();
        private Participant _parent;
        public double Provision { get; set; }

        public Participant Parent {
            get { return _parent; }
            set { _parent = value; addChild(this, _parent); }
        }
        public int getLvl()
        {
            return deriveLvl(this);
        }
        private int deriveLvl(Participant p, int lvl = 0)
        {
            Participant parent = p.Parent;
            if (parent == null) // jesli nie ma rodzica (czyli jest zalozycielem) zwroci 0
            {
                return lvl;
            }

            return deriveLvl(parent, lvl + 1); // jesli posiadda rodzica, wywoluje sprawddzenie czy rodzic posiada rodzica
        }
        public void addChild(Participant child, Participant parent)
        {
            if(parent != null) { 
                parent.children.Add(child); // dodanie rodzicowi obecnego uczestnika jako dziecko
            }
            if(parent.Parent != null)
            {
                addChild(child, parent.Parent); // dodanie rodzicowi rodzica uczestnika jako dziecko
            }
        }
        public int countChildWithoutChildren()
        {
            int result = 0;
            foreach (Participant child in children)
            {
                if(child.children.Count == 0) // liczenie ile uczestnikow podd obecnym uczestnikiem nie posiada podwladnych
                {
                    result++;
                }
            }
            return result;
        }
    }
}
