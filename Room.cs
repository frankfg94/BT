using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Patterns.Model;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Discord.Rest;

namespace BT
{
      public class Structure : ModuleBase<SocketCommandContext>
    {
        protected string name;
        // En mettant id en protected, cela permet d'y accèder dans les classes filles
        public  int id = 0;
        public EmbedBuilder illustration;
        protected Trap piege;
        public async Task ShowIllustration(SocketCommandContext context)
        {
            Console.WriteLine("Enter2");
            var msg = await context.Channel.SendMessageAsync(string.Empty, false, illustration);
        }
        public Structure()
        {
            id++;
        }
    }

    public enum PassageType
    {
        Safe,
        Risky,
        Talisman
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
        }
        private int selected = -1;

       


    }

        
    class Room : Structure
    {
        int voteSafe = 0;
        int voteRisky = 0;
        int voteTalis = 0;
        private bool hasTalisman = false;
        bool isLastRoom = false;
        public bool hasPistol = false;
        public EmbedBuilder illustrationPassage;
        private int roomID = 0;

        public bool SetTalisman(bool value)
        {
            if(value == true)
            {
                illustration.Title = ":gem: Salle au talisman";
            }
            return value;
        }

        public Room()
        {
            Console.WriteLine(">Création d'une salle" + "                              (ID:" + id + ")");
            id++;
            Map.numberOfRoom++;
            EmbedBuilder eb = new EmbedBuilder
            {
                Color = Color.DarkGrey,
                Title = "Vous êtes arrivés dans la salle n°" + Map.numberOfRoom,
                ImageUrl = "https://i.pinimg.com/originals/29/ef/19/29ef1970df77df39040233e74447808d.jpg"
            };
            illustration = eb;

            // Génération de l'illustration
            EmbedBuilder eb2 = new EmbedBuilder
            {
                Color = Color.DarkGrey,
                Title = "Choix du passage",
                ImageUrl = "https://c1.staticflickr.com/1/40/74396226_5f6a544e22.jpg"
            };
            eb2.AddField("Passage classique", "A priori sans risques sauf si piégé par un inconnu");
            eb2.AddField("Passage risqué", "Danger de mort, attention!");
            eb2.AddField("Passage Talisman", "Danger de mort, attention!");
            illustrationPassage = eb2;
            id++;
        }

        public void VotePassage(PassageType type)
        {
            Console.WriteLine("Enter VotePassage()");
            if (type == PassageType.Safe)
            {
                voteSafe++;
            }
            else if (type == PassageType.Risky)
            {
                voteRisky++;
            }
            else if (type == PassageType.Talisman)
            {
                voteTalis++;
                Console.WriteLine("Incrémentation talisman");
            }
        }

        public void SelectPassageWithVotes(int CurrentStructid)
        {
            Console.WriteLine("Structure actuelle: " + CurrentStructid + "\n");
            Console.Write("Analyse des votes...");
            if (voteSafe > voteRisky && voteSafe > voteTalis)
                (JDR.map.allStructures[CurrentStructid] as Passages).passages[0].isSelected = true;
            if (voteRisky > voteSafe && voteRisky > voteTalis)
                (JDR.map.allStructures[CurrentStructid] as Passages).passages[1].isSelected = true;
            if (voteTalis > voteSafe && voteTalis > voteRisky)
                (JDR.map.allStructures[CurrentStructid] as Passages).passages[2].isSelected = true;

            if (voteTalis == voteSafe && voteSafe == voteRisky && voteTalis == voteRisky && voteTalis == 0)
            {
                Console.WriteLine("Aucun vote n'a été effectué");
            }
            
            Console.Write("     Terminée");
            Console.WriteLine("\nS:" + voteSafe + "\nR:"+ voteRisky + "\nT:"+ voteTalis);
        }


        public async Task<RestUserMessage> ShowIllustrationPassage(SocketCommandContext context)
        {
            Console.WriteLine("Enter2");
            var msg = await context.Channel.SendMessageAsync(string.Empty, false, illustrationPassage);
            if (GetType() == typeof(Passages))
            {
                JDR.lastPassageMsgID = msg.Id;
            }
            return msg;
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

        public async Task ChoosePassage(SocketCommandContext context, int id, Map m)
        {
            var msg = await (m.allStructures[id] as Room).ShowIllustrationPassage(context);
            Console.WriteLine("Choix du passage");
            if(msg != null)
            {
                //var msg = await p.StartQCM(passageQCM.name);
                await msg.AddReactionAsync(new Emoji("🛡"));
                await msg.AddReactionAsync(new Emoji("❗"));
                await msg.AddReactionAsync(new Emoji("💎"));
                JDR.passageMsgs.Add(msg);
                int time = 20000;
                await context.Channel.SendMessageAsync(":timer: Fermeture de la salle dans " + time / 1000 + "s");
                await Task.Delay(time - 4000);

                // id + 1 car on regarde les passages à venir
                SelectPassageWithVotes(id+1);
                var pass = (m.allStructures[id+1] as Passages);
                foreach (var passage in pass.passages)  
                    {
                       if(passage.isSelected)
                       {
                        await context.Channel.SendMessageAsync("Passage sélectionné !  :" + passage.GetName());
                        if(passage.GetName() == "passage Talisman")
                            if(JDR.map.allStructures[id + 2] != null)
                            (JDR.map.allStructures[id + 2] as Room).illustration.Title = ":gem: Salle au Talisman";
                        if (JDR.map.allStructures[id + 1] != null)
                            JDR.map.allStructures[id + 1] = passage;
                       }

                }
                AudioModule am = new AudioModule((AudioService)Program._services.GetService(typeof(AudioService)), context);
                try { await am.DoorCloseSFX(); }
                catch (Exception ex) { Console.WriteLine(ex); }
                await context.Channel.SendMessageAsync("En route vers le passage !");
            }
            else
            {
                Console.WriteLine("Message nul");
            }
        }
    }

    class Passage : Structure
    {
        List<Player> playerOrder;
        protected int votes;
        public bool isSelected = false;
        protected string name;
        public string GetName()
        {
            return name;
        }

    }

    class RiskyPassage : Passage
    {
        public RiskyPassage()
        {
            illustration = new EmbedBuilder();
            illustration.Title = "Passage risqué";
            illustration.ImageUrl = "http://www.kinyu-z.net/data/wallpapers/65/929659.jpg";
            name = "passage Risqué"; 
        }
    }

    class SafePassage : Passage
    {
        public SafePassage()
        {
            illustration = new EmbedBuilder();
            illustration.Title = "Passage classique";
            illustration.ImageUrl = "https://media-cdn.tripadvisor.com/media/photo-s/05/4e/4f/9d/ramanathaswamy-temple.jpg";
            name = "passage Classique";
        }

    }

    class TalismanPassage : Passage
    {
        bool talismanStolen = false;
        public TalismanPassage()
        {
            illustration = new EmbedBuilder();
            illustration.Title = ":skull: Passage Talisman";
            illustration.ImageUrl = "https://s-media-cache-ak0.pinimg.com/originals/10/5e/3c/105e3c611b5468c5c3c6f6b7c09e322d.png";
            name = "passage Talisman";
        }
    }
}
