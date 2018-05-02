using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace BT
{
    public class Trap : ModuleBase<SocketCommandContext>
    {
        private double activationProb = 0.5;
        private double killProb = 0.5;
        private bool canFallAndDie = false;
        private bool used = false;
        private EmbedBuilder illustration;
        private string name = "Sans nom";

        void Desactivate()
        {
            used = true;
        }

        public async Task ShowIllustration(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync(string.Empty,false,illustration);
        }

        public string GetName()
        {
            return name;
        }

        public Trap(double activationProba = 0.5, double killProba = 0.5, bool canFall = false)
        {
            // Fait bugger
            activationProb = activationProba;
            killProb = killProba;
            canFallAndDie = canFall;
            EmbedBuilder eb = new EmbedBuilder();
            if (killProba == 1)
            {
                name = "Marteau Pilon";
            }
            else
            {
                name = "Flèchette empoisonnée";
            }
            eb.Description = "Chance de s'activer : " + (activationProb * 100) + " %\n\n" + "Chance de tuer : " + (killProb * 100) + " %\n\n";
            eb.Title = ":boom: " + name + ":boom: ";
            illustration = eb;
        }

        private Random r = new Random();
        public async Task PlayerWalkOnMe(Player p,SocketCommandContext _context)
        {
                if (!used)
                {
                    var random = r.NextDouble();
                
                    if (random < activationProb)
                    {
                    if (!p.immuneToTraps || r.NextDouble() < killProb)
                        await JDR.Kill(p);
                    else
                    {
                        await _context.Channel.SendMessageAsync(p.user.Username + " a activé le piège mais a survécu!");
                        Desactivate();
                    }

                    }
                    else
                    {
                       await _context.Channel.SendMessageAsync(p.user.Username + " a esquivé le piège!!");
                     }
            }

        }
    }
}
