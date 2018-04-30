using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT
{
    public class Trap
    {
        public double activationProb = 0.5;
        double killProb = 0.5;
        bool used = false;

        void Desactivate()
        {
            used = true;
        }

        private Random r = new Random();
        public async Task UseOnPlayer(Player p)
        {

                if (!used)
                {
                    var random = r.NextDouble();
                    if (random > activationProb)
                    {       
                        if(!p.immuneToTraps)
                             await JDR.Kill(p);
                        Desactivate();
                    }
                }
            

        }
    }
}
