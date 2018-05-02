using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;    
using Microsoft.Extensions.DependencyInjection;

namespace BT   
{

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public class Program
    {


    [STAThread] // semble ne rien faire
        static void Main(string[] args)
        {
            Console.WriteLine("Starting ...\n");
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient _client;
        private CommandService _commands;
        static public IServiceProvider _services;

        string[] lines = File.ReadAllLines("token.txt");
        private string botToken;

        public static AudioService audioService = new AudioService();
        public async Task RunBotAsync()
        {
            //_services = new ServiceCollection().AddSingleton(new AudioService());
            _client = new DiscordSocketClient();
            _commands = new CommandService();            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(new AudioService())
                .BuildServiceProvider();
            _client.Log += Log;
            await RegistercommandAsync();
            
            _client.UserJoined += AnnounceJoinedUser; //Check if userjoined
            _client.ReactionRemoved += ReactionRemoved;
            Ping p = new Ping(_client);
            _client.ReactionAdded += p.ReactionParse;

            botToken = lines[0];
            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            await Task.Delay(-1);
            // event subscription  
        }

       
        public async Task VoiceUpdate(SocketUser user, SocketVoiceState state, SocketVoiceState state2) //welcomes New Players
        {
            var channel = _client.GetChannel(370666551306616845) as SocketTextChannel; //gets channel to send message in
            await channel.SendMessageAsync("Audio mis à jour pour " + user + "\nmuté :"+ state.IsMuted + "\nChaine :"+state.VoiceChannel);
        }


        public async Task ReactionRemoved(Cacheable<IUserMessage,ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await Console.Out.WriteLineAsync("Réaction supprimée");
        }

        public async Task AnnounceJoinedUser(SocketGuildUser user) //welcomes New Players
        {
            var channel = _client.GetChannel(370666551306616845) as SocketTextChannel; //gets channel to send message in
            await channel.SendMessageAsync("Bienvenue! " + user.Mention + " sur le serveur!"); //Welcomes the new useu
            if(user.Username.Contains("anon"))
                {
                await channel.SendMessageAsync("Vous êtes Jeremy Martin il me semble, le spécialiste de JV.COM"); //Welcomes the new user
                await user.ModifyAsync(x => 
                {
                     x.Nickname = "[JV]" + x.Nickname;
                }
                );
            }
            else if (user.Username.Contains("Mastermanga") )
            {
                await channel.SendMessageAsync("Ah, Yoann Torrado, je vous attendais! "); //Welcomes the new user
                await user.ModifyAsync(x =>
                {
                    x.Nickname = ">>" + user.Username +"<<";
                }
                );
            }
        }

        private Task Log(LogMessage arg)            
        {   
            Console.WriteLine(arg);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Fonction qui détecte chaque message reçu dans discord et lance l'événement HandleCommandAsync pour le traiter
        /// </summary>
        /// <returns></returns>
        public async Task RegistercommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /// <summary>
        ///Traitement du message par l'API de Discord
        /// </summary>
        /// <param name="arg">Désigne le contenu texte du message lui-même</param>
        /// <returns></returns>
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message is null || message.Author.IsBot) return;
            int argPos = 0;

            if (message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);

            }

        }
    }
}
