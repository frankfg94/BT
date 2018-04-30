using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Extensibility;

namespace BT
{


    public class JDR : ModuleBase<SocketCommandContext>
    {
        //private readonly SocketCommandContext context;
        //public JDR(SocketCommandContext _context)
        //{
        //    Context = _context;   
        //}
        ~JDR()
        {
            allPlayers = null;
            Console.WriteLine("Destruction du JDR");
        }
        public static SocketCommandContext _context;
        public static Map map;
        private RestVoiceChannel mainVoiceChannel;
        private RestTextChannel maintTextChannel;
        public static int currentStructureID = 0;
        public static int currentRoomID = 0;
        public static int RoomID = 0;
        public static List<Player> deadPlayers = new List<Player>();
        public static List<Player> allPlayers = new List<Player>();
        public static List<RestUserMessage> passageMsgs = new List<RestUserMessage>();
        public static int remainingPlayers = 0;
        public static ulong lastPassageMsgID;
        public List<IUser> GetVoiceConnectedUsers()
        {
            var users = Context.Guild.Users;
            List<IUser> usersConnect = new List<IUser>();
            foreach (var u in users)
            {
                // Plus tard rajouter seulement si le channel audio 
                if (u.VoiceChannel != null)
                    usersConnect.Add(u);
            }
            return usersConnect;
        }

        async Task StartCountdown1MIN()
        {
            // apparemment très mauvaise utiliser system.Threading pour de l'async, utiliser Task.Delay() à la place
            await ReplyAsync(":fire: Démarrage :fire:");
            System.Threading.Thread.Sleep(15000);
            await ReplyAsync(":fire: 45 secondes avant début de la partie:fire:");
            System.Threading.Thread.Sleep(15000);
            await ReplyAsync(":fire: 30 secondes avant début de la partie:fire:");
            System.Threading.Thread.Sleep(20000);
            await ReplyAsync(":fire: Début dans 10 secondes :fire:");
            System.Threading.Thread.Sleep(5000);
            await ReplyAsync("5");
            System.Threading.Thread.Sleep(1000);
            await ReplyAsync("4");
            System.Threading.Thread.Sleep(1000);
            await ReplyAsync("3");
            System.Threading.Thread.Sleep(1000);
            await ReplyAsync("2");
            System.Threading.Thread.Sleep(1000);
            await ReplyAsync("1");
        }

        // Permet d'éviter plusieurs fois les mêmes chaines à chaque fois qu'on lance un JDR exemple : Joueurs en attente, Joueurs en attente, Joueurs en attente (3 channels vocals
        // alors qu'on en a besoin que d'1 seul)
        public async Task DeleteOldChannels(string oldNameText= "lobby", string oldNameVoice = "en attente")
        {

            var allTextChannels = Context.Guild.TextChannels;
            var allVoiceChannels = Context.Guild.VoiceChannels;
            foreach (var channel in allTextChannels)
            {
                if(channel.Name.Contains(oldNameText.ToLower()))
                {
                    await channel.DeleteAsync();
                }
            }
            foreach (var channel in allVoiceChannels)
            {
                if (channel.Name.Contains(oldNameVoice))
                {
                    await channel.DeleteAsync();
                }
            }
        }

        private EmbedBuilder ShowInstructions(Player p)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            return embedBuilder;
        }

        [Command("map",RunMode = RunMode.Async)]
        private async Task ShowMap(bool quick = true)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Title = "Carte : Temple Maya   ",
                ImageUrl = "https://pre00.deviantart.net/3bb2/th/pre/i/2016/034/2/7/temple_sacrifice_by_jonathanguzi-d9qesxl.jpg",
                Color = Color.DarkGreen
            };
            EmbedFooterBuilder e = new EmbedFooterBuilder
            {
                Text = allPlayers.Count.ToString() + " joueurs"
            };
            embedBuilder.Footer = e;
            await Context.Channel.SendMessageAsync(string.Empty,false,embedBuilder);
        }

        [Command("async",RunMode = RunMode.Async)]
        private async Task StartIntroMusic()
        {
            try
            {
                AudioModule am = new AudioModule((AudioService)Program._services.GetService(typeof(AudioService)), Context);
                await am.Music2();
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }

        public bool CheckIfVoiceConnectedUsers()
        {
            var users = Context.Guild.Users;
            foreach (var u in users)
            {
                if (u.VoiceChannel != null)
                {
                    return true;
                }
            }
            return false;
        }

        [Command("genMap", RunMode = RunMode.Async)]
        public async Task CreateMap(int nbSalles)
        {
            _context = Context;
            map = new Map(nbSalles);
            await ReplyAsync(map.GetMiniMap());
        }

        [Command("sacr", RunMode = RunMode.Async)]
        public async Task Sacr()
        {
            Room r = new Room();
            await r.ProposeSacrifice(Context);
        }

        public async Task ShowRolesDiscordAsync()
        {
            Console.WriteLine("Entrée ShowRolesDiscordAsync()");
            var voiceUsers = GetVoiceConnectedUsers();
            Console.WriteLine("1");

            List<int> ct = new List<int>(new int[] { CharacterType.CaraLoft, CharacterType.FBI_Inspector, CharacterType.MayaSucessor, CharacterType.NaziSoldier, CharacterType.SerialKiller, CharacterType.Tourist, CharacterType.Tourist });
            var ctRandomized = ct.OrderBy(a => Guid.NewGuid()).ToList();
            int ID = 0;
            Console.WriteLine("2");
            foreach (var u in voiceUsers)
            {
                Player p = new Player(ctRandomized[ID], u);
                Console.WriteLine("Alpha");
                ID++;
                PlayerDisplay display = new PlayerDisplay(Context);
                await display.ShowGoalAsync();
                display.AddRoleToPlayer(p);
                await display.ShowAbilitiesAsync();

            }
            Console.WriteLine("Sortie ShowRolesDiscordAsync()");
        }

        private async Task CreateJDRChannels()
        {
            await Context.Channel.SendMessageAsync("Tentative de création");
            await DeleteOldChannels("lobby", "en attente");
            maintTextChannel = await Context.Guild.CreateTextChannelAsync("Lobby");
            mainVoiceChannel = await Context.Guild.CreateVoiceChannelAsync("Joueurs en attente");
        }

        public static async Task Kill(Player p)
        {
            deadPlayers.Add(p);
            if(allPlayers.Contains(p))
            {
                allPlayers.Remove(p);
            }
           await (p.user as SocketGuildUser).ModifyAsync(x => x.Nickname = "[MORT]" + p.user.Username);
        }



        Random r = new Random();
        [Command("startJDR", RunMode = RunMode.Async)]
        public async Task StartGameAsync()
        {


            await mainVoiceChannel.ModifyAsync(x=>x.Name="Joueurs du JDR");
            if(allPlayers.Count > 0)
            {
                // Calcul d'aléatoire de début de partie pour le pistolet
                if (map != null)
                    map.PistolPossible(1);
                else
                    Console.WriteLine("Aucune map crée");

                // Placement du talisman dans la première salle
                var random = r.NextDouble();
                if (random <= 1)
                {
                    map.allStructures.OfType<Room>().First().SetTalisman(true);
                    Console.WriteLine("La structure " + map.allStructures.OfType<Room>().First().id + " possède le talisman");
                }


                // On renomme la dernière salle en salle finale
                var lastRoom = map.allStructures.OfType<Room>().Last();
                lastRoom.illustration.Title = lastRoom.illustration.Title + " (Salle finale)";

                // Entrée du batiment
                Console.WriteLine("Nombre de structures " + map.allStructures.Count);
                if (currentStructureID == 0)
                {
                    string introTxt = "Intro";
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Title = "Temple Maya - " + allPlayers.Count + " joueurs",
                        Description = introTxt,
                        ImageUrl = "https://s-media-cache-ak0.pinimg.com/originals/f0/73/06/f07306ec3a8f66709791aaadf8cc77c1.jpg"
                    };
                    await ReplyAsync("", false, eb);
                }

                // Parcours des salles
                for (int id = 0; id < map.allStructures.Count; id++)
                {
                    Thread.Sleep(10000);
                    await _context.Channel.SendMessageAsync("On passe à la structure suivante / NOUVELLE ITERATION");
                    Console.WriteLine("Structure actuelle" + id);
                    if (map.allStructures[id].GetType() == typeof(Room) && id != map.allStructures.Count - 1)
                    {
                        
                        await ProposeAbilityUse();
                        Console.WriteLine("Sortie de ProposeAbilityUse()");
                        Thread.Sleep(10000);
                        Console.WriteLine("Pièce de type salle");
                        await (map.allStructures[id] as Room).ShowIllustration(Context);
                        // On regarde et on assigne un pistolet
                        await CheckPistol(map.allStructures[id] as Room);
                        await ProposeSacrifice();
                         Thread.Sleep(10000);
                        await (map.allStructures[id] as Room).ChoosePassage(Context, currentStructureID, map);
                        currentRoomID++;
                    }
                    else // Sinon c'est passage
                    {
                        await map.allStructures[currentStructureID].ShowIllustration(Context);
                    }
                    currentStructureID++;

                }

                // Sortie du batiment
                if (currentStructureID == map.allStructures.Count)
                {
                    string outroTxt = "Outro";
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Title = "Temple Maya - " + allPlayers.Count + " joueurs",
                        Description = outroTxt,
                        ImageUrl = "http://moonshineink.com/sites/default/files/so_a_offroad_multiplejeeps.tiff_web.jpg"
                    };
                    await ReplyAsync("Tableau récapitulatif", false, eb);
                }
            }
            else
            {
                await ReplyAsync(":warning: Un moins un joueur doit être inscrit pour commencer la partie");
            }
        }

        private async Task CheckPistol(Room room)
        {
            if(room.hasPistol)
            {
                var randPlay = allPlayers.OrderBy(x => Guid.NewGuid()).ToList();
                await ReplyAsync( randPlay[0].user.Mention + "a trouvé un pistolet à poudre !");
            }
        }

        public async Task ProposeAbilityUse()
        {
            await ReplyAsync(":information_source: Les aventuriers qui le souhaitent peuvent activer une compétence avant d'entrer dans la prochaine salle");
            foreach(var p in allPlayers)
            {
                if(p.textChannel!=null)
                {
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Title = "Salle " + JDR.currentRoomID + 1 + " : Utiliser compétence ?";
                    var msg = await p.textChannel.SendMessageAsync("", false, eb);
                    await msg.AddReactionAsync(new Emoji("✔"));
                }

            }
        }

        public async Task ProposeSacrifice()
        {
            Console.WriteLine("Entrée Message sacrifice ");
            EmbedBuilder b = new EmbedBuilder();
            b.Title = ("Souhaitez vous sacrifier un des aventuriers?");
            for (int i = 0; i < allPlayers.Count; i++)
                b.Description += "\n" + allPlayers[i].user.Username + "\n";
            Console.WriteLine("1 ");
            var msg = await ReplyAsync("", false, b);
            Console.WriteLine("2 ");
            Console.WriteLine("nb joueurs: " + allPlayers.Count);
            for (int i = 1; i < allPlayers.Count + 1; i++)
            {
                if (i == 1)
                    await msg.AddReactionAsync(new Emoji("🇦"));
                else if (i == 2)
                    await msg.AddReactionAsync(new Emoji("🇧"));
                else if (i == 3)
                    await msg.AddReactionAsync(new Emoji("🇨"));
                else if (i == 4)
                    await msg.AddReactionAsync(new Emoji("🇩"));
                // Pas sûr ques les smileys soient reconnus à partir de i == 5
                else if (i == 5)
                    await msg.AddReactionAsync(new Emoji("🇪"));
                else if (i == 6)
                    await msg.AddReactionAsync(new Emoji("🇫"));
            }
            Console.WriteLine("Fin Message sacrifice ");
        }
        [Command("init", RunMode = RunMode.Async)]
        public async Task InitAsync()
        {
            var bot = Context.User;
            //Player p = new Player(CharacterType.NaziSoldier, await Context.Channel.GetUserAsync(322421524319567882));
            try
            {
                foreach (var user in Context.Guild.Users)
                {
                    await DeleteOldChannels(user.Username);
                }
                await DeleteOldChannels(Context.User.Username, "JDR");
            }
            catch { }    


            int count = 0;
            var ok =  CheckIfVoiceConnectedUsers();
            if(ok)
            {
                    var ts = Context.Channel.EnterTypingState();
                await CreateJDRChannels();
                var users = Context.Guild.Users;
                string allUsers = string.Empty;
                foreach (var u in users)
                {

                    // l'identifiant auquel il ne faut pas toucher est celui de François, le fait que je sois admin bloque les commandes et donc l'exécution du code
                    // On filtre également tous les bots, qui ne sont pas considérés comme des utilisateurs
                    if (!u.IsBot && u.Id != 353243323592605699)
                    {   
                        // On bouge tous les membres du serveur, déjà connectés à un channel vocal, sur une même voice channel
                        await u.ModifyAsync(x => x.ChannelId = mainVoiceChannel.Id);
                        allUsers += " \n" + u.Username;
                        count++;
                    }
                }
                await ReplyAsync(allUsers);
                await ShowRolesDiscordAsync();


                // on affiche le nombre d'utilisateurs sur le nom de la chaîne
                if (count > 1)
                    await maintTextChannel.ModifyAsync(x => x.Name = maintTextChannel.Name + " " + count);
                await Context.Channel.SendMessageAsync("Initialisation effectuée avec succès!");

                // On commence un thread séparé afin de pouvoir lancer la musique en parallèle
                Thread t = new Thread(async () => await StartIntroMusic());
                t.Start();
                await StartCountdown1MIN();
                ts.Dispose();
                await ShowMap();
                await CreateMap(5);
                await StartGameAsync();
                await ReplyAsync(map.GetMiniMap());

            }
            else
            {
                await ReplyAsync(":warning: Au moins un joueur doit être connecté sur channel vocal pour débuter");
            }
        }

    }
}
