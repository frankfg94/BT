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
        public static SocketCommandContext _context;
        static Map map;
        private RestVoiceChannel voiceChannel;
        private RestTextChannel textChannel;
        public int currentStructureID = 0;
        public static List<Player> allPlayers = new List<Player>();
        public static List<RestUserMessage> passageMsgs = new List<RestUserMessage>();
        public static int remainingPlayers = 0;
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
            var voiceUsers = GetVoiceConnectedUsers();
            List<CharacterType> ct = new List<CharacterType>(new CharacterType[] { CharacterType.CaraLoft, CharacterType.FBI_Inspector, CharacterType.MayaSucessor, CharacterType.NaziSoldier, CharacterType.SerialKiller, CharacterType.Tourist, CharacterType.Tourist });
            var ctRandomized = ct.OrderBy(a => Guid.NewGuid()).ToList();
            int ID = 0;
            foreach (var u in voiceUsers)
            {
                Player p = new Player(ctRandomized[ID], u);
                ID++;
                PlayerDisplay display = new PlayerDisplay(Context);
                Console.WriteLine("Alpha");
                //await display.AddRoleToUserTest("Touriste");
                await display.ShowGoalAsync();
                display.AddRoleToPlayer(p);
                await display.ShowAbilitiesAsync();
                await display.RemoveRolesJDR();
                //await display.AddRoleToPlayer(p);  // L'ajout de rôles fait tout bugger
            }
        }

        private async Task CreateJDRChannels()
        {
            await Context.Channel.SendMessageAsync("Tentative de création");
            await DeleteOldChannels("lobby", "en attente");
            textChannel = await Context.Guild.CreateTextChannelAsync("Lobby");
            voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Joueurs en attente");
        }

        [Command("startJDR", RunMode = RunMode.Async)]
        public async Task StartGameAsync()
        {
            Console.WriteLine("Nombre de structures "+ Structure.id);
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

            foreach (var s in map.allStructures)
            {
                currentStructureID++;
                Console.WriteLine("structure actuelle" + currentStructureID);
                Thread.Sleep(5000);
                await map.MoveToNextStructure(Context);
            }

            // Mettre le bon identifiant
            if (currentStructureID == Structure.id)
            {
                string outroTxt = "Outro"; 
                EmbedBuilder eb = new EmbedBuilder
                {
                    Title = "Temple Maya - " + allPlayers.Count + " joueurs",
                    Description = outroTxt,
                    ImageUrl = "http://moonshineink.com/sites/default/files/so_a_offroad_multiplejeeps.tiff_web.jpg"
                };
                await ReplyAsync("", false, eb);
            }
        }
        [Command("init", RunMode = RunMode.Async)]
        public async Task InitAsync()
        {
            var bot = Context.User;

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
                        await u.ModifyAsync(x => x.ChannelId = voiceChannel.Id);
                        allUsers += " \n" + u.Username;
                        count++;
                    }
                }
                await ReplyAsync(allUsers);
                await ShowRolesDiscordAsync();


                // on affiche le nombre d'utilisateurs sur le nom de la chaîne
                if (count > 1)
                    await textChannel.ModifyAsync(x => x.Name = textChannel.Name + " " + count);
                await Context.Channel.SendMessageAsync("Initialisation effectuée avec succès!");

                // On commence un thread séparé afin de pouvoir lancer la musique en parallèle
                Thread t = new Thread(async () => await StartIntroMusic());
                t.Start();
                await StartCountdown1MIN();
                await ShowMap();
                map = new Map(5);
                map.GetMiniMap();
                await StartGameAsync();
                await ReplyAsync(map.GetMiniMap());
                ts.Dispose();

            }
            else
            {
                await ReplyAsync(":warning: Au moins un joueur doit être connecté sur channel vocal pour débuter");
            }
        }

    }
}
