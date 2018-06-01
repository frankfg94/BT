using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Linq;
using Discord.WebSocket;
using System.Diagnostics;

namespace BT
{

    public class Ping : ModuleBase<SocketCommandContext>
    {

        async Task<int> LoadDataAsync()
        {
            await Task.Delay(2000);
            return 42;
        }
        private readonly DiscordSocketClient _client;
        public Ping(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("a")]
        public async Task ObtainTheFileAsync()
        {
            await ReplyAsync("Je vous ai ouvert une fenêtre");
            var progressForm = new Form()
            {
                Width = 300,
                Height = 200,
                Text = "Charger un fichier... "
            };
            TextBox txtBox = new TextBox
            {
                Location = new Point(10, 50),
                Visible = true
            };
            progressForm.Controls.Add(txtBox);
            var progressFormTask = progressForm.ShowDialog();
            var data = await LoadDataAsync();
            progressForm.Close();
            MessageBox.Show(data.ToString());
        }

        [Command("userList")]
        public async Task DisplayUsers()
        {

            var users = Context.Guild.Users;
            foreach (var u in users)
                if (!u.IsBot && u.Id != 353243323592605699) 
                {
                    await u.ModifyAsync(x => x.Nickname = "[Test]" + u.Username);
                    await ReplyAsync(u.Username);
                }

            if (users.Count == 4)
            {
                await ReplyAsync("\nBien il semblerait que nous sommes tous présents, car nous sommes 4!");
            }
        }

        private Random n = new Random(Guid.NewGuid().GetHashCode());
        static bool responded = false;


        [Command("dice")]
        public async Task Dice([Remainder] int max = 6)
        {
            var EmbedBuilder = new EmbedBuilder();
            EmbedBuilder.WithColor(Discord.Color.Blue);
            EmbedBuilder.WithTitle("Vous avez lancé un dé");
            Random r = new Random();
            EmbedBuilder.AddField("Résultat : ", r.Next(0, max));
            await Context.Channel.SendMessageAsync("", false, EmbedBuilder);
        }

        public async Task AddQcmReactions(IUserMessage msg)
        {
            await msg.AddReactionAsync(new Emoji("🇦"));
            await msg.AddReactionAsync(new Emoji("🇧"));
            await msg.AddReactionAsync(new Emoji("🇨"));
            await msg.AddReactionAsync(new Emoji("🇩"));
        }

        public async Task GetAllParameters(object b)
        {
            var properties = b.GetType().GetProperties();
            foreach (var p in properties)
            {
                string name = p.Name;
                object value = p.GetValue(b);
                await ReplyAsync(name + " = " + value);
                Console.WriteLine(name + " = " + value);
            }
        }

        [Command("info", RunMode = RunMode.Async)]
        public async Task Info([Remainder]ulong id)
        {
            var user = await Context.Channel.GetUserAsync(id);
            await GetAllParameters(user);
        }

        //[Command("IA")]
        //public async Task IA()
        //{

        //}

        [Command("rep", RunMode = RunMode.Async)]
        public async Task Answer([Remainder] string letter)
        {
            await ReplyAsync("Vous avez choisi la réponse : " + letter + ")");
            if (responded)
            {
                if (letter == "a")
                {
                    await ReplyAsync("  Bonne réponse!!");
                    responded = true;
                }
                else if (letter == "b" || letter == "c" || letter == "d")
                {
                    await ReplyAsync("  Mauvaise réponse");
                }
                else
                {
                    await ReplyAsync("Merci d'entrer a, b, c ou d");
                }
            }
            else
            {
                await ReplyAsync("Merci de d'abord lancer un QCM");
            }
        }


        [Command("clean", RunMode = RunMode.Async)]
        public async Task Clean([Remainder] uint amount)
        {
            var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await ReplyAsync("Suppression de " + amount + " messages");
        }


        [Command("kill", RunMode = RunMode.Async)]
        public async Task Kill([Remainder] string name)
        {
            var users = Context.Guild.Users;
            foreach(var u in users)
            {
                if (u.Username.ToUpper() == name.ToUpper() || u.Nickname.ToUpper() == name.ToUpper())
                {
                    await u.ModifyAsync(x => x.Nickname = "[MORT]" + u.Username);
                    await ReplyAsync(":scream: " + u.Username + " vient de mourir!");
                }
            }

        }

        [Command("killID", RunMode = RunMode.Async)]
        public async Task KillID( ulong id)
        {
            SocketGuildUser user = await Context.Channel.GetUserAsync(id) as SocketGuildUser;
            await user.ModifyAsync(x => x.Nickname = "[MORT]" + user.Username);
            await ReplyAsync(":scream: " + user + " vient de mourir!");
        }

        public async Task KillUser(SocketGuildUser user)
        {
               Console.WriteLine(user.Username);
               await user.ModifyAsync(x => x.Nickname = "[MORT]" + user.Username);
               await ReplyAsync(":scream: " + user.Username + " vient de mourir!");
        }

        [Command("killAll", RunMode = RunMode.Async)]
        public async Task KillAll()  
        {
            var users = Context.Guild.Users;
            string userString = null;
            foreach(var u in users)
            {
                Console.WriteLine(u);
                if (!u.IsBot && u.Id != 353243323592605699)
                {
                    await u.ModifyAsync(x => x.Nickname = "[!]" + u.Username); 
                }
            }
            await ReplyAsync(userString);
            await ReplyAsync("Bonjour, j'ai changé vos pseudos pour voir :eyeglasses: ");
        }

        [Command("resetN", RunMode = RunMode.Async)]
        public async Task ResetNames()  
        {
            var users = Context.Guild.Users;
            foreach (var u in users)
            {
               if( u.Id != 353243323592605699)
                    await u.ModifyAsync(x => x.Nickname = u.Username);
            }
            await ReplyAsync("Vos pseudos sont revenus à la normale ! :eyeglasses: ");
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task ShowHelp()
        {
            var eb = new EmbedBuilder();
            var eb2 = new EmbedBuilder();
            string desc = ">> Il faut mettre un point d'exclamation en préfixe pour exécuter une commande\n:diamond_shape_with_a_dot_inside: ping: Le bot affiche 'Hello World'(edited)\n:diamond_shape_with_a_dot_inside: qcm: le bot lance un qcm très basique de mathématiques\n: diamond_shape_with_a_dot_inside: qcm2: le bot lance un qcm visuel(edited)\n:diamond_shape_with_a_dot_inside: a: (si client discord.net téléchargé uniquement) ouvre une fenêtre windows\n:diamond_shape_with_a_dot_inside: clean[nb de messages] : permet de supprimer jusqu'à 100 messages dans la conversation actuelle\n:diamond_shape_with_a_dot_inside: dice[n] : Lance un dé possédant n faces(edited)\n:diamond_shape_with_a_dot_inside: rep[a | b | c | d] : Répond à la commande !qcm en sélectionner une des lettres a, b, c ou d\n:diamond_shape_with_a_dot_inside: eb: Envoie un message test d'Embed Builder(edited)\n:diamond_shape_with_a_dot_inside: note[string] : Attribue une note sur 10 à un objet, personne entré en tant que string\n:diamond_shape_with_a_dot_inside: clip1: lance une musique via youtube directement sur le bot\n: diamond_shape_with_a_dot_inside: clip2: lance une musique via un chemin de l'ordinateur directement sur le bot(edited)\n:diamond_shape_with_a_dot_inside: info[id] : Lance le maximum d'informations sur un utilisateur dont l'ID est indiqué en paramètre" +
                "\n:diamond_shape_with_a_dot_inside: !init : Lance un JDR dans un temple" +
                "\n:diamond_shape_with_a_dot_inside: !bigQcm : crée un qcm d'exemple 'a'" +
                "\n:diamond_shape_with_a_dot_inside: !preview[nom du qcm] : affiche les questions d'un qcm ";
            string desc2 = "\n:diamond_shape_with_a_dot_inside: !start[nom du qcm] : débute un Qcm " +
            "\n:diamond_shape_with_a_dot_inside: !add[nom du qcm] : ajoute une question au Qcm en fin de liste " +
             "\n:diamond_shape_with_a_dot_inside: !addMany[nom du qcm] : ajoute 5 questions au Qcm en fin de liste " +
            "\n:diamond_shape_with_a_dot_inside:  !del[nom du qcm] : supprime la dernière question du Qcm indiqué" +
            "\n:diamond_shape_with_a_dot_inside:  !deleteQCM[nom du qcm] : supprimer définitivement un QCM" +
            "\n:diamond_shape_with_a_dot_inside:  !dice[nom du qcm] : lance un dé à n faces" +
            "\n:diamond_shape_with_a_dot_inside:  !citation : lance lance une citation au hasard" +
            "\n:diamond_shape_with_a_dot_inside:  !mix[nom du qcm] : mélange les questions d'un qcm ";

            eb.WithDescription(desc);
            eb2.WithDescription(desc2);
            await Context.Channel.SendMessageAsync("",false,eb);
            await Context.Channel.SendMessageAsync("", false, eb2); 
        }


        public static List<Qcm> qcmList = new List<Qcm>();

        [Command("mix", RunMode = RunMode.Async)]
        public async Task MixQCM([Remainder] string QCMname)
        {
            bool QCMfound = false;
            foreach (var qcm in qcmList)
            {
                if (qcm.name.ToUpper() == QCMname.ToUpper().Trim())
                {
                    QCMfound = true;
                    qcm.MixQuestions();
                    qcm.MixQuestions();
                    qcm.MixQuestions();
                    qcm.MixQuestions();

                    await ReplyAsync("Le QCM : " + qcm.name + " a été mélangé avec succès");
                }
            }
            if (!QCMfound)
            {
                if (qcmList.Count > 0)
                {
                    await DisplayList();
                }
                else
                {
                    await ReplyAsync(" ! Aucun QCM n'a pour le moment été crée");
                }

            }
        }



        [Command("deleteQCM", RunMode = RunMode.Async)]
        public async Task DeleteQCM([Remainder] string QCMname)
        {
            bool QCMfound = false;
            foreach(var qcm in qcmList)
            {
                if (qcm.name.ToUpper() == QCMname.ToUpper().Trim())
                {
                    QCMfound = true;
                    qcm.Delete();
                    await ReplyAsync("QCM supprimé!");
                }
            }
            if (!QCMfound)
            {
                if (qcmList.Count > 0)
                {
                    await DisplayList();
                }
                else
                {
                    await ReplyAsync(" ! Aucun QCM n'a pour le moment été crée");
                }

            }
        }

        [Command("CreateAndStart", RunMode = RunMode.Async)]
        public async Task CreateAndStart([Remainder] string name = "")
        {
            if(name == "")
            {
                await ReplyAsync("Merci d'indiquer un nom pour le QCM à générer");
            }
            else
            {
                await BigQcm(name);
                await StartQCM(name);
            }
        }

        [Command("BigQcm", RunMode = RunMode.Async)]
        public async Task BigQcm(string name = "a")
        {
            ISocketMessageChannel channel = Context.Channel;
            await ReplyAsync("Lancement du méga QCM! :fire: ");
            Qcm bigQcm = new Qcm();
            qcmList.Add(bigQcm);
            bigQcm.name = name;
            // l'audio ne marche que si premiere question QUE
            bigQcm.AddQuestion(QType.text);
           bigQcm.AddQuestion(QType.text,false,Qcm.TextQuestion.DefaultContent);
            bigQcm.AddQuestion(QType.image); // Image multiple
            bigQcm.AddQuestion(QType.image,true); // Image simple
            try { await ReplyAsync("Questions ajoutées avec succès"); }
            catch { await ReplyAsync("Erreur lors de la création du QCM"); }
            //await Context.Guild.CreateTextChannelAsync(bigQcm.name);
            //await bigQcm.Preview(channel);
        }


        [Command("qcmIMG", RunMode = RunMode.Async)]
        public async Task QcmSampleSingleIMG(string name = "img")
        {
            ISocketMessageChannel channel = Context.Channel;
            await ReplyAsync("Lancement du QCM Image! :fire: ");
            Qcm bigQcm = new Qcm();
            qcmList.Add(bigQcm);
            bigQcm.name = name;
            bigQcm.AddQuestion(QType.image, true);
            bigQcm.AddQuestion(QType.image, true);
            bigQcm.AddQuestion(QType.image, true); 
            bigQcm.AddQuestion(QType.image, true); // Image simple
            try { await ReplyAsync("Questions ajoutées avec succès"); }
            catch { await ReplyAsync("Erreur lors de la création du QCM"); }
            //await Context.Guild.CreateTextChannelAsync(bigQcm.name);
            //await bigQcm.Preview(channel);
        }

        [Command("blindtest", RunMode = RunMode.Async)]
        public async Task BlindTest(string name = "blindtest")
        {
            try
            {
                ISocketMessageChannel channel = Context.Channel;
            await ReplyAsync("Génération du BlindTests! :fire: ");
            Qcm bigQcm = new Qcm();
            qcmList.Add(bigQcm);
            bigQcm.name = name;
            bigQcm.AddQuestion(QType.audio);
            bigQcm.AddQuestion(QType.audio);
            bigQcm.AddQuestion(QType.audio);
            bigQcm.AddQuestion(QType.audio);
            bigQcm.AddQuestion(QType.audio);
            bigQcm.AddQuestion(QType.audio);
            bigQcm.AddQuestion(QType.audio);

                await ReplyAsync("Questions ajoutées avec succès");
            }
            catch { await ReplyAsync("Erreur lors de la création du QCM"); }
            //await Context.Guild.CreateTextChannelAsync(bigQcm.name);
            //await bigQcm.Preview(channel);
           await StartQCM(name);
        }

        [Command("qcmM", RunMode = RunMode.Async)]
        public async Task MathSampleQcm(string name = "maths")
        {
            ISocketMessageChannel channel = Context.Channel;
            await ReplyAsync("Lancement du méga QCM! :fire: ");
            Qcm bigQcm = new Qcm();
            qcmList.Add(bigQcm);
            bigQcm.name = name;
            for(int i = 0; i < 3; i++)   bigQcm.AddQuestion(QType.text);
            await ReplyAsync("Questions ajoutées avec succès");
        }

        [Command("qcmT", RunMode = RunMode.Async)]
        public async Task BasicSampleQcm(string name = "text")
        {
            ISocketMessageChannel channel = Context.Channel;
            await ReplyAsync("Lancement du méga QCM! :fire: ");
            Qcm bigQcm = new Qcm();
            qcmList.Add(bigQcm);
            bigQcm.name = name;
            for (int i = 0; i < 10; i++) bigQcm.AddQuestion(QType.text, false, Qcm.TextQuestion.DefaultContent);
            await ReplyAsync("Questions ajoutées avec succès");
        }

        private async Task DisplayList()
        {
            string s = string.Empty;
            int i = 0;
            if (qcmList.Count() == 0)
            {
                s = "Aucun QCM n'existe pour le moment";
            }
            else
            {
                await ReplyAsync("Aucune correspondance n'a été trouvée\n Voici la liste des noms repertoriés");
                foreach (var qcm in qcmList)
                {
                    s = s + "\n" + i + ". " + qcm.name;
                    i++;
                }
            }

            await ReplyAsync(s);
        }


        [Command("preview", RunMode = RunMode.Async)]
        public async Task Preview([Remainder] string qcmName)
        {
            bool QCMfound = false;
            foreach (var qcm in qcmList)
            {
                if (qcm.name.ToUpper() == qcmName.ToUpper().Trim())
                {
                    await ReplyAsync("QCM sélectionné, prévisualisation...");
                    QCMfound = true;
                    await qcm.Preview(Context.Channel);
                }
            }
            if (!QCMfound)
            {
                if (qcmList.Count > 0)
                {
                    await DisplayList();
                }
                else
                {
                    await ReplyAsync(" ! Aucun QCM n'a pour le moment été crée");
                }
            }
        }

        [Command("add", RunMode = RunMode.Async)]
        public async Task AddQuestion([Remainder]string qcmName)
        {
            bool QCMfound = false;
            int type = QType.text;
            foreach (var qcm in qcmList)
            {
                if (qcm.name == qcmName.Trim())
                {
                    QCMfound = true;
                    qcm.AddQuestion(type);
                    string typeString = string.Empty;
                    if (type == QType.text) typeString = "Texte";
                    else if (type == QType.image) typeString = "Image";
                    else if (type == QType.audio) typeString = "Audio";
                    else  typeString = "Inconnu";
                    await ReplyAsync("Nouvelle question créee ! \n Elle de type :" + typeString);
                }
            }
            if (!QCMfound)
            {
                if (qcmList.Count > 0)
                {
                    await DisplayList();
                }
                else
                {
                    await ReplyAsync(" ! Aucun QCM n'a pour le moment été crée");
                }

            }
        }

        List<IUser> voter = new List<IUser>();
        public async Task ReactionParse(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel msg2, SocketReaction socketReaction)
        {
            await Console.Out.WriteLineAsync("\n----------------------------------------------------------------------\nRéaction détectée!! " + msg.Id);


            if (qcmList != null)
            {
                foreach (var qcm in qcmList)
                {
                    if (qcm.HasStarted)
                    {
                        if (qcm.questionsID.Contains(socketReaction.MessageId) && qcm.questionsID.Last() == msg.Id )
                        {
                            if (!socketReaction.User.Value.IsBot)
                            {
                                qcm.allAnswers.Add(socketReaction);
                                await StopAudioCmd();
                                await StartQCM(qcm.name);
                                i++;
                            }

                        }
                    }
                }
            }

                 if (!socketReaction.User.Value.IsBot) /*&& !voter.Contains(socketReaction.User.Value)*/
            {
                Console.WriteLine("Réaction non bot détectée");
                if(JDR.hasStarted )
                {
                    foreach(var p in JDR.allPlayers)
                    {
                        foreach (var ab in p.abilities)
                        {
                            if (msg.Id == ab.askMessage.Id)
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.WriteLine("Cliqué sur un message de capacité " + p.user.Username + " utilise la compétence : " + ab.illustration.Title);
                                Console.BackgroundColor = ConsoleColor.Black;
                                await p.UseAbility(Context,ab,p);
                            }
                        }
                    }
                }
                if (JDR.sacrificeMsgID.Contains(socketReaction.MessageId))
                {
                    Console.WriteLine("Détection sacrifice");
                    if (JDR.hasStarted)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        if (socketReaction.Emote.Name == "🇦")
                        {
                            JDR.allPlayers[0].sacrificeCount++;
                        }
                        else if (socketReaction.Emote.Name == "🇧")
                        {
                            JDR.allPlayers[1].sacrificeCount++;
                        }
                        else if (socketReaction.Emote.Name == "🇨")
                        {
                            JDR.allPlayers[2].sacrificeCount++;
                        }
                        else if (socketReaction.Emote.Name == "🇩")
                        {
                            JDR.allPlayers[3].sacrificeCount++;
                        }
                        else if (socketReaction.Emote.Name == "🇪")
                        {
                            JDR.allPlayers[4].sacrificeCount++;
                        }
                        else if (socketReaction.Emote.Name == "🇫")
                        {
                            JDR.allPlayers[5].sacrificeCount++;
                        }
                    }
                  
                }
                if (JDR.passageMsgsID.Contains(msg.Id) )
                {

                    if (JDR.hasStarted)
                    {
                        Console.WriteLine("Message de passage JDR");
                        //voter.Add(msg.Value.Author);
                        var structActuelle = (JDR.map.allStructures[JDR.currentStructureID] as Room);
                        Console.WriteLine("Choix du passage :   ");
                        if (socketReaction.Emote.Name == "🛡")
                        {
                            Console.Write("Safe");
                            structActuelle.VotePassage(PassageType.Safe);
                        }
                        else if (socketReaction.Emote.Name == "❗")
                        {
                            Console.Write("Risqué");
                            structActuelle.VotePassage(PassageType.Risky);
                        }
                        else if (socketReaction.Emote.Name == "💎")
                        {
                            Console.Write("Talisman");
                            structActuelle.VotePassage(PassageType.Talisman);
                        }
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync("Le JDR n'a pas été commencé " + msg.Id);
                    }
                }

                else
                {
                    Console.WriteLine(JDR.sacrificeMsgID.Last()+ " | " + msg.Id);
                }

            }
            else
            {
                await Console.Out.WriteLineAsync("Non reconnu");

            }







            // si oui, on examine si la reaction est sur le bon embed builder


            // si elle n'est pas la bonne réponse, on la supprime

            // si le smiley n'est pas le bon, message spécials


        }

        public async Task<Qcm> GetQcm(string name)
        {
            foreach (Qcm qcm in qcmList)
            {
                if (qcm.name.ToUpper() == name.ToUpper().Trim())
                {
                    Console.WriteLine("QCM détecté :" + qcm.name);
                    return qcm;
                }
            }
            await DisplayList();
            return null;
        }

        public async Task CheckAnswer(IUserMessage msg, Qcm.Question q)
        {
            await Task.Delay(5000);
            if(msg.Reactions.Count() > 4)
            {
                var reaction = msg.Reactions.Last();
                await ReplyAsync(reaction.Value.ToString());
            }
        }

        [Command("citation",RunMode = RunMode.Async)]
        public async Task DisplayCitation()
        {
            try
            {
                Console.WriteLine("ok");
                var lines = File.ReadAllLines("./citations.txt");
                Console.WriteLine("ok1");
                Random r = new Random();
                var randomLineNumber = r.Next(0, lines.Length - 1);
                EmbedBuilder e = new EmbedBuilder();
                e.WithTitle("Citation n°" + randomLineNumber);
                e.WithDescription(lines[randomLineNumber]);
                await Context.Channel.SendMessageAsync("", false, e);
                Console.WriteLine("ok2");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
            
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAudioCmd()
        {
            AudioService audioService = (AudioService)Program._services.GetService(typeof(AudioService));
            AudioModule am = new AudioModule(audioService, Context);
            await am.JoinCmd();
        }

        [Command("off", RunMode = RunMode.Async)]
        public async Task ShutDown()
        {
            await Context.Channel.SendMessageAsync("A la prochaine !");
            await Leave();
            Environment.Exit(0);
        }


        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopAudioCmd()
        {
            // await _client.StopAsync();
            
            if (Process.GetProcessesByName("ffmpeg") != null)
            {
                foreach (var process in Process.GetProcessesByName("ffmpeg"))
                {
                    process.Kill();
                }

            }
            //await _client.StopAsync();
            //await _client.StartAsync();
            //AudioService audioService = (AudioService)Program._services.GetService(typeof(AudioService));
            //AudioModule am = new AudioModule(audioService, Context);
            //am.StopCmd();
        }


        //[Command("next", RunMode = RunMode.Async)]
        //public async Task ChoosePassage()
        //{
        //    Room a = new Room();
        //    await a.ChoosePassage(Context);
        //}

            class QcmPlayer
        {
            public QcmPlayer(IUser _user)
            {
                user = _user;
            }
            public IUser user;
            public int score;
            public int maxScore;
        }

        AudioModule am;
        Stopwatch stopwatch = Stopwatch.StartNew();
        static int i = 1;
        [Command("start", RunMode = RunMode.Async)]
        public async Task StartQCM([Remainder] string qcmName)
        {
            Console.WriteLine();
            Qcm qcm = await GetQcm(qcmName);
            AudioService audioService = (AudioService)Program._services.GetService(typeof(AudioService));
            if(am == null)
                 am = new AudioModule(audioService, Context);
            IMessage msg = null;
            if (!qcm.HasStarted)
            {
                stopwatch.Restart();
                qcm.HasStarted = true;
                Console.WriteLine("Affichage Q1" );
                // Si ça buggue ç'est à cause du display Mode
                msg = await qcm.DisplayInDiscord(_client.GetChannel(414746672284041222) as ISocketMessageChannel, qcm.questions[0],Qcm.DisplayMode.QCM, am);
                qcm.questionsID.Add(msg.Id);
            }
            else //Problème réussir à bloquer l'affichage Q2 cad ignorer le dernier smiley
            { 
               ISocketMessageChannel channel = _client.GetChannel(414746672284041222) as ISocketMessageChannel;
                if (i < qcm.questions.Count)
                {
                    Console.WriteLine("On arrive à une question de type :" + qcm.questions[i].type);
                    msg = await qcm.DisplayInDiscord(_client.GetChannel(414746672284041222) as ISocketMessageChannel, qcm.questions[i], Qcm.DisplayMode.QCM);
                    qcm.questionsID.Add(msg.Id);
                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                }
                // Tableau des réponses
                else
                {
                    // Le tableau des réponses ne s'affiche que pour un affichage de type QCM
                    if(qcm.displayMode == Qcm.DisplayMode.QCM)
                    {
                        await channel.SendMessageAsync("Vous êtes arrivé au bout de ce QCM");
                        await channel.SendMessageAsync("Réponses enregistrées : " + qcm.allAnswers.Count);
                        EmbedBuilder embed = new EmbedBuilder();
                        int i = 0;
                        int score = 0;
                        int maxScore = qcm.allAnswers.Count;
                        Console.OutputEncoding = System.Text.Encoding.UTF8;
                        var usersIDThatAnswered = new List<ulong>();
                        var playerList = new List<QcmPlayer>();
                        foreach(var ans in qcm.allAnswers)
                        {
                            if (!usersIDThatAnswered.Contains(ans.User.Value.Id))
                            {
                                usersIDThatAnswered.Add(ans.User.Value.Id);
                                playerList.Add(new QcmPlayer(ans.User.Value));
                            }

                        }
                        foreach (var ans in qcm.allAnswers)
                        {

                            try
                            {
                                if(ans.Emote.Name != null)
                                {
                                    
                                    embed.AddField("Votre Réponse n°" + (i + 1) + " : " + ans.Emote.Name, " par " + ans.User.Value.Username);
                                    foreach(var player in playerList)
                                    {
                                        if (ans.Emote.Name == qcm.questions[i].answerLetter && ans.User.Value == player.user)
                                        {
                                            player.score++;
                                        }
                                        if(ans.User.Value == player.user)
                                        player.maxScore++;
                                    }
                                    embed.AddField("Bonne réponse ", qcm.questions[i].answer + ":" + qcm.questions[i].answerLetter);
                                    if (ans.Emote.Name == qcm.questions[i].answerLetter)
                                    {
                                        score++;    
                                    }
                                    else
                                    {
                                        Console.WriteLine(ans.Emote.Name  + " : " + qcm.questions[i].answerLetter);
                                    }
                                }

                                i++;
                                embed.AddField("", "");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        await channel.SendMessageAsync("", false, embed);
                        await channel.SendMessageAsync(" Score Final " + score + " / " +maxScore +"\n Durée : " +(stopwatch.ElapsedMilliseconds/1000)+"s");
                        foreach(var player in playerList)
                        {
                            await channel.SendMessageAsync(">> " + player.user.Username + " Score : "+ player.score +" / " + player.maxScore);
                        }
                    }
                }
            }
           // return msg;

        }

        [Command("addMany", RunMode = RunMode.Async)]
        public async Task AddQuestions([Remainder]string qcmName)
        {
            // problème étrange
            bool QCMfound = false;
            foreach (var qcm in qcmList)
            {
                if (qcm.name.ToUpper() == qcmName.ToUpper().Trim())
                {
                    int type = QType.text;
                    QCMfound = true;
                    string typeString = string.Empty;
                    if (type == QType.text) typeString = "Texte";
                    else if (type == QType.image) typeString = "Image";
                    else if (type == QType.audio) typeString = "Audio";
                    else typeString = "Inconnu";
                    qcm.AddQuestions();
                    await ReplyAsync("Questions rajoutées ! \n Type :" + typeString);
                }
            }
            if (!QCMfound)
            {
                if (qcmList.Count > 0)
                {
                    await DisplayList();
                }
                else
                {
                    await ReplyAsync(" :exclamation:  Aucun QCM n'a pour le moment été crée");
                }
            }
        }


        [Command("del", RunMode = RunMode.Async)]
        public async Task RemoveLastQuestion([Remainder] string qcmName)
        {
            bool QCMfound = false;
            foreach(var qcm in qcmList)
            {
                if(qcm.name == qcmName.Trim())
                {
                    await ReplyAsync("QCM sélectionné");
                    QCMfound = true;
                    qcm.DeleteQuestion(qcm.questions.Count-1);
                    await ReplyAsync("La question nommée : " + qcm.questions.Last().name+ " est désormais supprimée");
                }
            }
            if(!QCMfound)
            {
                if(qcmList.Count > 0)
                {
                    await DisplayList();
                }
                else
                {
                    await ReplyAsync(" ! Aucun QCM n'a pour le moment été crée");
                }

            }
        }

        [Command("leave",RunMode = RunMode.Async)]
        public async Task Leave()
        {
            AudioService audioService = (AudioService)Program._services.GetService(typeof(AudioService));
            AudioModule am = new AudioModule(audioService, Context);
            await am.LeaveCmd();
        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task Pin()    
        {
            var msg = await ReplyAsync("Hello World");
            await msg.AddReactionAsync(new Emoji("😨"));
            await Context.Client.SetGameAsync("House Party");
            // NE MARCHE PAS AVEC CO DE L'EFREI NI MON TEL !!!!!!!!!!!!
            try
            {
                AudioModule am = new AudioModule((AudioService)Program._services.GetService(typeof( AudioService)), Context);
                await am.Test();
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
                
        }   

        [Command("note")]
        public async Task Note(string s = "")
        {
            if (s == "")await  ReplyAsync("Merci d'indiquer la personne à noter :yes:");
            Random random = new Random();
            await Context.Channel.SendMessageAsync("Note : " + random.Next(0,11)+" / 10");
        }

    }

}
