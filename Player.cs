using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Patterns.Contracts;
using static BT.Ability;

namespace BT
{
    // Comprend le type de personnage, qui définit un personnage fictif pour le joueur, exemple: Soldat Nazi
    public class Player 
    {
        protected bool dead = false;
        public int _type;
        public IUser user;
        public string discordRole;
        public RestTextChannel textChannel;
        public int goal;
        [Required]
        private SocketCommandContext _context;
        public List<Ability> abilities = new List<Ability>();
        private ulong _playerID { get; set; }
        private string _playerName { get; set; }
        public int votePower = 1;
        public bool immuneToTraps = false;
        public int sacrificeCount = 0;
        

        public Player(int type, IUser u, SocketCommandContext context) 
        {
            _type = type;
            user = u;
            _context = context;
            // En fonction du type, on assigne les bonnes compétences et capacités spéciales

            AssignGoalAndAbilities();
            //Console.WriteLine("Assigné : " + Enum.GetName(typeof(CharacterType), _type) + "\npour le joueur: " + u.Username + "\nobjectif: " + goal);
            JDR.allPlayers.Add(this);
        }

        public async Task ShowAbilityInTextChannel(Ability ability)
        {
            Console.WriteLine("entrée de ShowAbilityInTextChannel()");
            string description;
            string title;
            string url;
            string description2;
            switch (ability.identifier)
            {
                case Ability.ID.ActivateTrap:
                    title = "Poseur de piège";
                    description = "Vous pouvez activer un piège une fois par partie";
                    url = "http://nwamotherlode.com/wp-content/uploads/2016/01/trap-33819_640.png";
                    break;
                case Ability.ID.PrepareSurvival:
                    title = "Survivaliste";
                    description = "Vous pouvez vous préparer à survivre à un piège une fois par partie";
                    url = "";
                    break;
                case Ability.ID.PushInTrap:
                    title = "Assassinat";
                    description = "Vous pouvez pousser un aventurier dans une trappe";
                    url = "https://d30y9cdsu7xlg0.cloudfront.net/png/39793-200.png";
                    break;
                case Ability.ID.RevealTrap:
                    title = "Sixième sens";
                    description = "Vous pouvez détecter un piège";
                    url = "";
                    break;
                case Ability.ID.SacrificeSomeone:
                    title = "Sacrifice";
                    description = "Vous avez le pouvoir de sacrifier des aventuriers, en demandant un vote à chaque tour";
                    url = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b4/Codex_Magliabechiano_%28folio_70r%29.jpg/440px-Codex_Magliabechiano_%28folio_70r%29.jpg";
                    break;
                case Ability.ID.SecretConversation:
                    title = "Micro-oreillette";
                    description = "Vous pouvez communiquer en secret avec un coéquipier du même type";
                    url = "";
                    break;
                case Ability.ID.ShowFBICard:
                    title = "Gouvernement";
                    description = "Vous pouvez montrer votre carte FBI, afin d'augmenter vos votes";
                    url = "https://img00.deviantart.net/36bd/i/2015/105/f/5/punisher_fbi_card_franck_castle_by_moviecard-d80pkde.jpg";
                    break;
                case Ability.ID.ShootSomeone:
                    title = "Pistolet à poudre";
                    description = "Vous pouvez tuer un joueur à tout moment, avec 70% de précision";
                    url = "https://images.frankonia.fr/fsicache/server?type=image&width=720&height=720&effects=pad(CC,ffff)&quality=40&source=products/p113707_ha.jpg";
                    break;
                default:
                    title = "Erreur";
                    description = "Compétence non reconnue, vérifier l'identifiant de cette dernière";
                    url = "";
                    break;
            }
            switch (ability.usageMode)
            {
                case Ability.UsageType.ForEachRoom:
                    description2 = "Usage répété";
                    break;
                case Ability.UsageType.Single:
                    description2 = "Usage unique";
                    break;
                default:
                    description2 = "Erreur, type d'usage inconnu";
                    break;
            }
            var footer = new EmbedFooterBuilder
            {
                Text = description2
            };
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = title,
                Description = description,
                ImageUrl = url,
                Footer = footer
            };
            if (url == null || url == string.Empty)
            {
                embed.ImageUrl = "http://olivierdewyse.com/wp-content/uploads/2015/02/icones_ampoule.png";
            }
            ability.illustration = embed;
            await textChannel.SendMessageAsync("",false,ability.illustration);
            //await PrivCh(textChannel.Id);
            Console.WriteLine("sortie de ShowAbilityInTextChannel()");
        }

        [Command("priv", RunMode = RunMode.Async)]
        public async Task PrivCh(ulong id)
        {
            var ch = _context.Guild.GetChannel(id) as ITextChannel;
            Console.WriteLine("OK");
            OverwritePermissions e = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny);
            Console.WriteLine("OK pour e");
            foreach(var u in _context.Guild.Users)
            {
                if(!u.IsBot || u.Id != 353243323592605699)
                {
                    await ch.AddPermissionOverwriteAsync(u, e);
                }
            }
            Console.WriteLine("OK");
        }

        public async Task UseAbility(SocketCommandContext context, Ability ab, Player target)
        {
            if (JDR.hasStarted)
            {
                if(ab.isAvailable)
                {
                    // On regarde si la capacité spéciale est utilisée sur le joueur même

                        switch (ab.identifier)
                        {
                            case Ability.ID.ShowFBICard:
                                target.votePower = 2;
                                await JDR._context.Channel.SendMessageAsync(context.Message.Author.Mention + "A sorti sa carte FBI!\nSon vote compte désormais pour double");
                                break;
                            case Ability.ID.PrepareSurvival:
                                target.immuneToTraps = true;
                                await JDR._context.Channel.SendMessageAsync(context.Message.Author.Mention + "s'est immunisé au prochain piège !");
                                break;
                            case Ability.ID.RevealTrap:
                                break;

                        // Dans le cas où l'on utilise la capacité sur un autre joueur

                            case Ability.ID.ShootSomeone:
                                await context.Channel.SendMessageAsync(context.Message.Author.Mention + "a tué " + target.user.Username);
                                await JDR.Kill(target);
                                break;
                            default:
                                Console.WriteLine("erreur d'ID d'ability fonction Use()");
                                break;
                    }
                    // Si l'usage est unique, on ne peut utiliser qu'une fois la compétence
                    if (ab.usageMode == UsageType.Single)
                    {
                        ab.isAvailable = false;
                    }
                }
           

            }


        }

        private void AssignGoalAndAbilities()
        {
            Console.WriteLine("Entrée AssignGoalAndAbilities() ");
            switch (_type)
            {
                case CharacterType.CaraLoft:
                    goal = Goal.FindTalismans;
                    var ability = new Ability(Ability.ID.PrepareSurvival, Ability.UsageType.Single,this);                       
                    abilities.Add(ability);
                    break;
                case CharacterType.FBI_Inspector:
                    goal = Goal.Kill.SerialKiller;
                    var ability2 = new Ability(Ability.ID.ShowFBICard, Ability.UsageType.Single,this);
                    abilities.Add(ability2);
                    break;
                case CharacterType.MayaSpirit:
                    var abilityS = new Ability(Ability.ID.ActivateTrap, Ability.UsageType.ForEachRoom);
                    abilities.Add(abilityS);
                    goal = Goal.Protect.Maya;
                    break;
                case CharacterType.MayaSucessor:
                    var ability3 = new Ability(Ability.ID.ActivateTrap, Ability.UsageType.Single);
                    abilities.Add(ability3);
                    goal = Goal.Protect.Talismans;
                    break;
                case CharacterType.NaziSoldier:
                    var ability4 = new Ability(Ability.ID.SecretConversation, Ability.UsageType.Single);
                    abilities.Add(ability4);
                    goal = Goal.Kill.CaraLoft;
                    break;
                case CharacterType.SerialKiller:
                    var ability5 = new Ability(Ability.ID.PushInTrap, Ability.UsageType.ForEachRoom);
                    abilities.Add(ability5);
                    goal = Goal.Kill.Tourist;
                    break;
                case CharacterType.Tourist:
                    var ability6 = new Ability(Ability.ID.RevealTrap, Ability.UsageType.Single,this);
                    abilities.Add(ability6);
                    goal = Goal.ExitTemple;
                    break;
                default:
                    Console.WriteLine("Cas non pris en charge lors de l'attribution des compétences pour la création du joueur : " + user.Username);
                    break;

            }
            Console.WriteLine("Sortie AssignGoalAndAbilities() ");

        }


    }


    public class PlayerDisplay : ModuleBase<SocketCommandContext>
    {
        SocketCommandContext _context;
        public PlayerDisplay(SocketCommandContext context)
        {
            _context = context;
        }

        SocketRole SelectRole(string roleName)
        {
            return _context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
        }


        public void AddRoleToPlayer(Player p)
        {
            Console.WriteLine("entrée dans AddRoleToPlayer()");
            IUser user = p.user;
            string role = null;
            List<string> roleString = new List<string>(new string[] { "Cara Loft", "Inspecteur FBI", "Esprit Maya", "Héritière Maya", "Soldat Nazi", "Tueur en série", "Touriste" });
            if (p._type == CharacterType.CaraLoft)
            {
                role = roleString[0];
            }
            else if (p._type == CharacterType.FBI_Inspector)
            {
                role = (roleString[1]);
            }
            else if (p._type == CharacterType.MayaSpirit)
            {
                role = (roleString[2]);
            }
            else if (p._type == CharacterType.MayaSucessor)
            {
                role = (roleString[3]);
            }
            else if (p._type == CharacterType.NaziSoldier)
            {
                role = (roleString[4]);
            }
            else if (p._type == CharacterType.SerialKiller)
            {
                role = (roleString[5]);
            }
            else if (p._type == CharacterType.Tourist)
            {
                role = (roleString[6]);
            }
            else if (p._type == CharacterType.Unassigned)
            {
                Console.WriteLine(p.user.Username + "n'a pas de role car réglé sur non assigné");
            }
            // await (user as SocketGuildUser).AddRoleAsync(role);
            p.discordRole = role;
            Console.WriteLine("sortie de AddRoleToPlayer()");
        }


        public async Task AddRoleToUserTest(string name )
        {
            Console.WriteLine("YE");
            SocketRole role = null;
            role = SelectRole(name);
            Console.WriteLine("Utilisateur" + name);
            SocketGuildUser user = _context.Guild.GetUser(322421524319567882);
            try
            {
                await user.AddRoleAsync(role);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task RemoveRolesJDR()
        {
            foreach(var p in JDR.allPlayers)
            {
                var u = p.user as SocketGuildUser;
                foreach (var role in u.Roles)
                {
                    if(role.Name == "Cara Loft"  || role.Name == "Inspecteur FBI" || role.Name == "Esprit Maya" || role.Name == "Héritière Maya" || role.Name == "Soldat Nazi" || role.Name == "Tueur en série" || role.Name == "Touriste")
                    {
                        await role.DeleteAsync();
                    }
                }
            }
        }

        public ulong GetChannelID(string name)
        {
            var txtCh = _context.Guild.TextChannels;
            foreach (var ch in txtCh)
            {
                if (ch.Name == name)
                {
                    return ch.Id;
                }
            }
            Console.WriteLine("Impossible d'obtenir la chaîne intitulée: " + name);
            return 0;
        }

        public async Task ShowAbilitiesAsync()
        {

            var playerList = JDR.allPlayers;
            foreach (var u in playerList)
            {
                foreach (var ab in u.abilities)
                {
                   await u.ShowAbilityInTextChannel(ab);
                }
                //   var perm =  ch.GetPermissionOverwrite(SelectRole("@everyone"));
                //      perm = perm.Value.Modify(null,null,null,PermValue.Deny);
                //await ch.AddPermissionOverwriteAsync(SelectRole("@everyone"), perm.Value);
            }

        }

        

        public async Task ShowGoalAsync()
        {
            Console.WriteLine("Entrée de ShowGoalAsync()");
            string description ;
            string title ;
            string url;
            Console.WriteLine("Nombre de joueurs: "  + JDR.allPlayers.Count);
            var playerList = JDR.allPlayers;
            foreach(var u in playerList)
            {
                switch (u.goal)
                {
                    case Goal.ExitTemple:
                        title = "Sauve qui peut";
                        description = "Vous devez vous enfuir à tout prix de ce temple";
                        url = "";
                        break;
                    case Goal.FindTalismans:
                        title = "Vol de Talismans";
                        description = "Votre objectif est de récupérer le plus de Talismans possible";
                        url = "";
                        break;
                    case Goal.Kill.CaraLoft:
                        title = "Assassinat";
                        description = "Vous devez tuez l'aventurière, Cara Loft";
                        url = "";
                        break;
                    case Goal.Kill.SerialKiller:
                        title = "Tueur tué";
                        description = "Nos informateurs ont rapporté qu'un psychopathe fait partie de l'expédition, éliminez-le à tout prix et remportez la prime";
                        url = "";
                        break;
                    case Goal.Kill.Tourist:
                        title = "Pas d'incruste";
                        description = "Vous devez tuer le maximum de touristes";
                        url = "";
                        break;
                    case Goal.Protect.Talismans:
                        title = "Temple sacré";
                        description = "Vous devez protéger les Talismans";
                        url = "";
                        break;
                    default:
                        title = "Non assigné";
                        description = "Erreur, l'objectif n'a pas encore de description";
                        url = "";
                        break;
                }
                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = title,
                    Description = description,
                    ImageUrl = url
                    
                };
                if(url == null ||url == string.Empty)
                {
                    embed.ImageUrl = "http://www.astucesdegolf.com/wp-content/uploads/2015/01/objectif.jpg";
                }

                // Ne marche pas,tout comme ce qui est Reply Async

                    
                    u.textChannel= await _context.Guild.CreateTextChannelAsync(u.user.Username);

                await u.textChannel.SendMessageAsync(string.Empty, false, embed);
                //   var perm =  ch.GetPermissionOverwrite(SelectRole("@everyone"));
                //      perm = perm.Value.Modify(null,null,null,PermValue.Deny);
                //await ch.AddPermissionOverwriteAsync(SelectRole("@everyone"), perm.Value);
            }
            Console.WriteLine("Sortie de ShowGoalAsync()");

        }


    }




    public class Ability 
    {
        // A mettre en protected plus tard
        public EmbedBuilder illustration;
        public bool isAvailable = true;
        public Player _target;
        public IUserMessage askMessage;
        public Ability(int NameID, int usageType = UsageType.Single, Player target = null)
        {
            identifier = NameID;
            usageMode = usageType;
            DefineEffectsOnTarget(target);
        }

       

        private void DefineEffectsOnTarget(Player character)
        {

        }
        public int identifier;
        public int usageMode;
        public class ID
        {
            public const int ActivateTrap = 0;
            public const int PushInTrap = 1;
            public const int SecretConversation = 2;
            public const int RevealTrap = 3;
            public const int PrepareSurvival = 4;
            public const int ShowFBICard = 5;
            public const int SacrificeSomeone = 6;
            public const int ShootSomeone = 7;
        }
        public class UsageType
        {
            public const int Single = 0;
            public const int ForEachRoom = 1;
        }
    }

    public static class CharacterType
    {
        public static Player player;
        public const int MayaSpirit = 0;
        public const int MayaSucessor = 1;
        public const int FBI_Inspector = 2;
        public const int SerialKiller = 3;
        public const int Tourist = 4;
        public const int NaziSoldier = 5;
        public const int CaraLoft = 6;
        public const int Unassigned = 7;
    }

    public  class Goal
    {
        public bool isReached = false;
        public const int KillEveryone = 0;
        public const int FindTalismans = 1;
        public const int ExitTemple = 11;
        public static class Protect
        {
            public const int Talismans = 2;
            public const int Maya = 3;
        }

        public static class Kill
        {
            public const int MayaSpirit = 4;
            public const int MayaSucessor = 5;
            public const int FBI_Inspector = 6;
            public const int SerialKiller = 7;
            public const int Tourist = 8;
            public const int NaziSoldier = 9;
            public const int CaraLoft = 10;
        }
        
    }
}
