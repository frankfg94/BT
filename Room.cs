using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Patterns.Model;
using Discord.WebSocket;
using Discord.Commands;
using Discord;

namespace BT
{
     class Structure : ModuleBase<SocketCommandContext>
    {
        protected string name;
        // En mettant id en protected, cela permet d'y accèder dans les classes filles
        public static int id = 0;
        protected EmbedBuilder illustration;

        public async Task ShowIllustration(SocketCommandContext context)
        {
            Console.WriteLine("Enter2");
            await context.Channel.SendMessageAsync(string.Empty,false,illustration);
        }   
    }

    class Passages : Passage
    {

        public List<Passage>passages = new List<Passage>();
        SafePassage p1 = new SafePassage();
        RiskyPassage p2 = new RiskyPassage();
        TalismanPassage p3 = new TalismanPassage();
        public Passages()
        {
            passages.Add(p1);
            passages.Add(p2);
            passages.Add(p3);
            Console.WriteLine(">>Création d'un passage multiple, longueur :" + passages.Count + "(ID:"+id+")");

            // Génération de l'illustration
            EmbedBuilder eb = new EmbedBuilder
            {
                Color = Color.DarkGrey,
                Title = "Choix du passage",
                ImageUrl = "https://c1.staticflickr.com/1/40/74396226_5f6a544e22.jpg"
            };
            eb.AddField("Passage classique", "A priori sans risques sauf si piégé par un inconnu");
            eb.AddField("Passage risqué", "Danger de mort, attention!");
            eb.AddField("Passage Talisman", "Danger de mort, attention!");
            illustration = eb;
            id++;
        }
    }

        
    class Room : Structure
    {
        bool containsTalisman = false;
        bool isLastRoom = false;
        bool hasPistol = false;
        private static int roomID = 0;
        public Room()
        {
            Console.WriteLine(">Création d'une salle" + "                              (ID:" + id + ")");
            id++;
            roomID++;
            EmbedBuilder eb = new EmbedBuilder
            {
                Color = Color.DarkGrey,
                Title = "Vous êtes arrivés dans la salle n°" + roomID,
                ImageUrl = "https://i.pinimg.com/originals/29/ef/19/29ef1970df77df39040233e74447808d.jpg"
            };
            illustration = eb;
        }

        public async Task ProposeSacrifice(SocketCommandContext context)
        {
            int i = 1;
            EmbedBuilder eb = new EmbedBuilder();
            eb.Title = "Voulez vous sacrifier un joueur?";
            foreach (var p in JDR.allPlayers)
            {
                eb.Description = eb.Description + "\n" + i + ")     "+ p.user.Username;
                await context.Channel.SendMessageAsync("",false,eb);
                i++;
            }
           
        }

        public async Task ChoosePassage(SocketCommandContext context)
        {
            Console.WriteLine("Debut");
            EmbedBuilder eb = new EmbedBuilder
            {
                Color = Color.DarkGrey,
                Title = "Choix du passage",
                ImageUrl = "https://c1.staticflickr.com/1/40/74396226_5f6a544e22.jpg"
            };
            eb.AddField("Passage classique", "A priori sans risques sauf si piégé par un inconnu");
            eb.AddField("Passage risqué", "Danger de mort, attention!");
            eb.AddField("Passage Talisman", "Danger de mort, attention!");

                var msg = await context.Channel.SendMessageAsync(string.Empty, false, eb);
            //var msg = await p.StartQCM(passageQCM.name);
            await msg.AddReactionAsync(new Emoji("🛡")); 
            await msg.AddReactionAsync(new Emoji("❗"));
            await msg.AddReactionAsync(new Emoji("💎"));
            JDR.passageMsgs.Add(msg);
            int time = 20000;
            await context.Channel.SendMessageAsync(":timer: Fermeture de la salle dans " + time/1000 + "s");
            await Task.Delay(time-4000);
            AudioModule am = new AudioModule((AudioService)Program._services.GetService(typeof(AudioService)), context);
            try { await am.DoorCloseSFX(); }
            catch(Exception ex) { Console.WriteLine(ex); }
            await context.Channel.SendMessageAsync("En route vers le passage !");
        }
    }

    class Passage : Structure
    {
        
    }

    class RiskyPassage : Passage
    {
        
    }

    class SafePassage : Passage
    {

    }

    class TalismanPassage : Passage
    {
        bool talismanStolen = false;
    }
}
