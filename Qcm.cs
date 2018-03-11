using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BT
{
    public static class QType
    {
        public const int text = 0;
        public const int image = 1;
        public const int audio = 2;
        public const int random = -1;
    }

    public enum ImageQuestion
    {
        CorrespondingImageMultiple = 0,
        RecognizePictureSingle = 1,
        None = -1,
    };


    public class Qcm : ModuleBase<SocketCommandContext>
    {
        public List<SocketReaction> allAnswers = new List<SocketReaction>();
        public string name = "defaut";
        public bool HasStarted = false;

        public enum CalculType
        {
            Addition = 0,
            Soustraction = 1,
            Multiplication = 2,
            // Division = 3
        }

        public class Question : ModuleBase<SocketCommandContext>
        {
            public int type;
            public object content;
            public string name = "Sans nom";
            public string answer = "🇦";
            public bool answered = false;
            public ImageQuestion imageQuestion;
            public Question(object _answer, int _type = QType.text, object _content = null, string Qname = "Sans nom", ImageQuestion imageQuestion = ImageQuestion.None)
            {
                type = _type;
                content = _content;
                name = Qname;
                if (type != QType.image)
                {
                    imageQuestion = ImageQuestion.None;
                }
            }

            public bool Answered()
            {
                if (answer == null)
                    return false;
                else
                    return true;
            }

            public void Delete()
            {

            }
        }

        public List<Question> questions = new List<Question>();

        public List<ulong> questionsID = new List<ulong>();
        /// <summary>
        /// Ajoute une question de type texte, image ou audio à la position indiquée, si aucune n'est précisée, la question sera rajoutée à la fin du Qcm
        /// </summary>
        public Question AddQuestion(int type, bool SingleImage = false,TextQuestion tq = TextQuestion.MathContent,int pos = -1)
        {
            Question qToAdd = null;
            if (type == QType.text)
            {
                if(tq == TextQuestion.MathContent)
                {
                    CalculType[] valeurs = { CalculType.Addition, CalculType.Multiplication, CalculType.Soustraction };
                    CalculType CalculTypeAleatoire = valeurs[r.Next(valeurs.Length)];
                    qToAdd = new Question(null, type, "Question texte Maths")
                    {
                         content = GenerateMathContent(CalculTypeAleatoire)
                    };
                }
                else if (tq == TextQuestion.DefaultContent)
                {
                    qToAdd = new Question(null, type, "Question texte par défaut")
                    {
                        content = GenerateDefaultContent(qToAdd)
                    };
                }

            }
            else if (type == QType.image)
            {
                qToAdd = new Question(null, type, "Question visuelle");
                if (SingleImage)
                {
                    qToAdd.content = GeneratePictureContent(qToAdd);
                }
                else
                {
                    qToAdd.content = GeneratePictureContent(qToAdd, ImageQuestion.CorrespondingImageMultiple);
                }
            }
            else
            {
                qToAdd = new Question(null, type, "Autre");
            }

            if (pos < 0)
            {
                questions.Add(qToAdd);
                // Console.WriteLine(qToAdd.name + "|" + qToAdd.type + "|" + qToAdd.content);
            }
            else
            {
                questions.Insert(pos, qToAdd);
            }
            return qToAdd;
        }

        List<EmbedBuilder> ebListSimple = new List<EmbedBuilder>();
        List<EmbedBuilder> ebListMultiple = new List<EmbedBuilder>();
        List<string> recognizeThingsPictures = new List<string>(new string[] { "Tank", "JeanneA", "JeuneFillePerle", "SpireDublin" });
        Random r = new Random();
        private List<EmbedBuilder> GeneratePictureContent(Question q, ImageQuestion type = ImageQuestion.RecognizePictureSingle)
        {
            EmbedBuilder imgEb1 = new EmbedBuilder();
            if (type == ImageQuestion.RecognizePictureSingle)
            {
                Console.WriteLine("Création d'une image avec 4 réponses a) b) c) d)");
                int randomNumber = r.Next(0, recognizeThingsPictures.Count());
                imgEb1.WithTitle("Que représente cette image");
                string url = null;
                string name = recognizeThingsPictures[randomNumber];
                switch (name)
                {
                    case "Tank":
                        url = "https://www.industrie-techno.com/mediatheque/5/4/4/000012445_imageArticlePrincipaleLarge.jpg";
                        q.answer = "Un char d'assaut Polonais Futuriste";
                        imgEb1.WithDescription(" a) Un prototype Polonais en construction \n b) Un char apparu dans le film Bladde Runner 2049  \n c) Un Fan-Art \n d) Un nouveau char américain");
                        break;
                    case "JeanneA":
                        url = "http://www.musee-orsay.fr/typo3temp/zoom/tmp_828c5c0a84bf57e06bc87366dc30b237.gif";
                        q.answer = "Jeanne D'Arc";
                        imgEb1.WithDescription(" a) Une jeune paysanne priant pour son mari \nb) Jeanne D'Arc\n c) Fernande Olivier \n d) Camille Claudel");
                        break;
                    case "JeuneFillePerle":
                        url = "https://cdn.radiofrance.fr/s3/cruiser-production/2016/05/5791f11c-4124-45b9-b493-2bd99aace836/738_meisje_met_de_parel.jpg";
                        imgEb1.WithTitle("Quel est l'auteur de ce tableau intitulé 'La jeune fille à la Perle'?");
                        q.answer = "Vermeer";
                        imgEb1.WithDescription(" a) Monet \n b) Vermeer \n c) Peeter de Hooch \n d) Van Gogh");
                        break;
                    case "SpireDublin":
                        imgEb1.WithTitle("Quel représente ce monument");
                        url = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1b/E4324-Spire-of-Dublin.jpg/390px-E4324-Spire-of-Dublin.jpg";
                        imgEb1.WithDescription(" a) La Spire de Dub lin \n b) L'épine de Berlin \n c) Le pic de Berlin \n d) La Pointe de Dublin");
                        q.answer = "La Spire de Dublin";
                        break;
                    default:
                        url = "https://blog.sqlauthority.com/i/a/errorstop.png";
                        q.answer = "Erreur, image non trouvée dans liste";
                        break;
                }
                imgEb1.WithImageUrl(url);
                ebListSimple.Add(imgEb1);
                q.imageQuestion = type;
                return ebListSimple;
            }
            else if (type == ImageQuestion.CorrespondingImageMultiple)
            {
                Console.WriteLine("Création de plusieurs images qui doivent correspondre à une question");
                List<string> correctImage = new List<string>(new string[] { "TableauVermeer" });
                EmbedBuilder imgEb2 = new EmbedBuilder();
                EmbedBuilder imgEb3 = new EmbedBuilder();
                EmbedBuilder imgEb4 = new EmbedBuilder();

                // r : nombre aléatoire de type Random(), R.Next(a,b) : prendre un nombre aléatoire entre a et b
                int randomNumber = r.Next(0, correctImage.Count());
                string name = correctImage[randomNumber];
                string url1, url2, url3, url4 = null;
                switch (name)
                {
                    case "TableauVermeer":
                        url1 = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7c/VermeerMilkmaid.jpg/230px-VermeerMilkmaid.jpg";
                        url2 = "http://www.rivagedeboheme.fr/medias/images/vermeer-la-jeune-fille-a-la-perle-1665-1667.jpg";
                        url3 = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/46/Cropped_version_of_Jan_Vermeer_van_Delft_002.jpg/260px-Cropped_version_of_Jan_Vermeer_van_Delft_002.jpg";
                        url4 = "https://upload.wikimedia.org/wikipedia/commons/7/78/Pieter_de_Hooch_-_The_Golf_Players_-_c.1658.jpg";
                        ebListMultiple.Add(imgEb1.WithImageUrl(url1));
                        ebListMultiple.Add(imgEb2.WithImageUrl(url2));
                        ebListMultiple.Add(imgEb3.WithImageUrl(url3));
                        ebListMultiple.Add(imgEb4.WithImageUrl(url4));
                        q.name = "Lequel fs tableaux n'a pas été peint par Vermeer?";
                        q.answer = "Vermeer";
                        break;
                    default:
                        url1 = "https://blog.sqlauthority.com/i/a/errorstop.png";
                        ebListMultiple.Add(imgEb1.WithImageUrl(url1));
                        q.name = "Erreur, image non trouvée dans la liste";
                        break;
                }
                q.imageQuestion = type;
                List<EmbedBuilder> MagicList = new List<EmbedBuilder> { imgEb1, imgEb2, imgEb3, imgEb4 };
                return MagicList;
            }
            return ebListMultiple;

        }

        int result;
        Random n = new Random();

        // créer un système d'héritage

        private EmbedBuilder ConvertToEmbed(List<Object> data)
        {
            EmbedBuilder eb = new EmbedBuilder();
            string[] textAnswers = (string[])data[0];
            string title = (string)data[1];
            string imageUrl = (string)data[2];

            if (textAnswers.Count() != 4)
            {
                Console.WriteLine("ATTENTION : Taille du tableau de réponse différent de 4");
            }

            eb.WithTitle(title);
            eb.AddField("Réponse :regional_indicator_a:", textAnswers[0]);
            eb.AddField("Réponse :regional_indicator_b:", textAnswers[1]);
            eb.AddField("Réponse :regional_indicator_c:", textAnswers[2]);
            eb.AddField("Réponse :regional_indicator_d:", textAnswers[3]);
            if (imageUrl != string.Empty)
            {
                eb.WithThumbnailUrl(imageUrl);
            }
            else
            {
                imageUrl = "http://st.le-precepteur.net/imgr/s/symboles/point-interrogation-rond-rouge-225x225.png";
            }
            return eb;
        }

        private List<Object> ImportDataForDefaultContent(int id)
        {
            List<Object> data = new List<object>();
            Console.WriteLine("Importation du contenu :" + id + " pour default text content");
            string imageUrl = "";
            string question = "Pas de titre fourni";
            string correctTextAnswer = "Pas de réponse fournie";
            string a, b, c, d = null;
            string[] possibleTextAnswers = new string[4];
            switch (id)
            {
                case 0:
                    {
                        question = "Dans quel océan la Nouvelle-Zélande se trouve-t-elle ?";
                        imageUrl = "http://img.playbuzz.com/image/upload/f_auto,fl_lossy,q_auto/cdn/c2d5087c-b230-4a96-87aa-20e8f14f7412/f4e9db16-8212-4ced-8dad-4fe4f6ff86e8.jpg";
                        a = "Antarctique";
                        b = "Pacifique";
                        c = "Indien";
                        d = "Atlantique";
                        correctTextAnswer = b;
                        break;
                    }
                case 1:
                    {
                        question = "À quel écrivain doit-on la « Divine Comédie » ?";
                        imageUrl = "http://img.playbuzz.com/image/upload/f_auto,fl_lossy,q_auto/cdn/c2d5087c-b230-4a96-87aa-20e8f14f7412/1795bb50-ff65-4b62-b67e-d101a4afdcc1.jpg";
                        a = "Dante";
                        b = "Umberto Eco";
                        c = "Petraque";
                        d = "Boccace";
                        correctTextAnswer = a;
                        break;
                    }
                default:
                    {
                        question = "Index incorrect";
                        a = b = c = d = "N/A";
                        break;
                    }

            }

            Console.WriteLine("1");
            possibleTextAnswers[0] = a;
            Console.WriteLine("2");
            possibleTextAnswers[1] = b;
            possibleTextAnswers[2] = c;
            possibleTextAnswers[3] = d;

            data.Add(possibleTextAnswers);
            data.Add(question);
            data.Add(imageUrl);
            data.Add(correctTextAnswer);
            Console.WriteLine("3");
            return data;
        }

        public enum TextQuestion
            {
                DefaultContent = 0,
                MathContent = 1,
            }

        public void UpdateQuestionData(Question q, Object[] data)
        {
            q.name = (string)data[1];
            q.answer = (string)data[3];
        }

        public EmbedBuilder GenerateDefaultContent(Question q)
        {
            Console.WriteLine("Tentative de génération d'une question texte par défaut");
            // Génération d'un nombre aléatoire
            int textQuestionCount = 2;
            int randomNumber = r.Next(0, textQuestionCount-1);

            // Importation des données, et conversion dans le format de Discord
            var data = ImportDataForDefaultContent(randomNumber);
            return ConvertToEmbed(data);
        }

        public EmbedBuilder GenerateMathContent(CalculType t = CalculType.Multiplication)
        {
            var eb = new EmbedBuilder();
            if(t == CalculType.Multiplication)
            {
                int a = n.Next(3, 20);
                int b = n.Next(3, 20);
                result = a * b;
                eb.WithTitle("QCM de mathématiques\n" + a + " * " + b + " = ? ");
            }
            else if (t == CalculType.Addition)  
            {
                int a = n.Next(100, 2000);
                int b = n.Next(100, 2000);
                result = a + b;
                eb.WithTitle("QCM de mathématiques\n" + a + " + " + b + " = ? ");
            }
            else if (t == CalculType.Soustraction)
            {
                int a = n.Next(500, 2000);
                int b = n.Next(50, 300);
                result = a - b;
                eb.WithTitle("QCM de mathématiques\n" + a + " - " + b + " = ? ");
            }
            else
            {
                eb.WithTitle("Type de calcul inconnu :" + t);
            }

            eb.AddField("a) ", result);
            eb.AddField("b) ", (result - 1));
            eb.AddField("c) ", (result + 1));
            eb.AddField("d) ", (result + 3));

            eb.WithColor(Color.DarkGreen);
            return eb;
        }

        private int SimpleID = 0;
        public async Task<IMessage> DisplayInDiscord(ISocketMessageChannel channel, Question question)
        {
            Console.WriteLine("Fonction d'affichage lancée");
            IUserMessage msg;
            if (question.type == QType.text)
            {
                EmbedBuilder eb = (EmbedBuilder)question.content;
                msg = await channel.SendMessageAsync("DisplayInDiscord()", false, eb);
                await AddQcmReactions(msg);
            }
            else if(question.type == QType.image)
            {
                Console.WriteLine("Tentative d'affichage pour type image...");
                if( question.content.GetType() == typeof(List<EmbedBuilder>)  ) // si plusieurs embeds et donc plusieurs iamges
                {
                    List<EmbedBuilder> imagesEb = (List<EmbedBuilder>)question.content;
                    // Liste d'eb donc liste d'images
                    if (question.imageQuestion == ImageQuestion.CorrespondingImageMultiple)
                    {
                        Console.WriteLine(" Type: images multiples");
                        foreach (var eb in (List<EmbedBuilder>)question.content)
                        {
                            // on envoie chaque image
                            await channel.SendMessageAsync(question.name, false, eb);
                        }
                        // Enfin, on affiche le nom de la question
                        msg = await channel.SendMessageAsync(question.name);
                        await AddQcmReactions(msg);
                    }
                    else
                    {
                        Console.WriteLine(" Type: image seule dans une Liste");
                        msg = await channel.SendMessageAsync("", false, imagesEb[SimpleID]);
                        SimpleID++;
                        await AddQcmReactions(msg);
                    }

                }
                else 
                {
                    Console.WriteLine("Type: image seule");
                    // Si ce n'est pas une liste, c'est un seul élément, donc une seule image
                    EmbedBuilder eb = (EmbedBuilder)question.content;
                    msg = await channel.SendMessageAsync("DisplayInDiscord()", false, eb);
                    await AddQcmReactions(msg);
                }
            }
            else
            {
                await Console.Out.WriteLineAsync("Pas de type texte ou image");
                 msg = await channel.SendMessageAsync("Questions de type : " + question.type + " pas encore gérées");
                await AddQcmReactions(msg, true);
            }
            return msg;

        }


        public async Task AddQcmReactions(IUserMessage msg, bool skip = false)
        {
            if(!skip)
            {
                await msg.AddReactionAsync(new Emoji("🇦"));
                await msg.AddReactionAsync(new Emoji("🇧"));
                await msg.AddReactionAsync(new Emoji("🇨"));
                await msg.AddReactionAsync(new Emoji("🇩"));
            }
            else
            {
                Console.WriteLine("AddQcmReactions --> replaceSingle != null (l132) --> Ajout smiley suivant");

                // Problème, la flèche n'est pas reconnue
                await msg.AddReactionAsync(new Emoji("🇦"));
               // await msg.AddReactionAsync(new Emoji("➡🇦"));
            }

        }


        /// <summary>
        /// Supprime définitivement ce QCM
        /// </summary>
        public void Delete()
        {
            Ping.qcmList.Remove(this);
            ReplyAsync("Le QCM intitulé "+ name + " a été supprimé avec succès");
        }

        public void AddQuestions(int type = QType.text, int amount = 5, int pos = -1)
        {
            for (int i = 0; i < amount; i++)
            {
                if (pos < 0)
                {
                    AddQuestion(type);
                    Console.Out.WriteLineAsync("Question ajoutée : ");
                    //Random r = new Random();
                    //questions[r.Next(0,questions.Count)];
                }
                else
                {
                    questions.Insert(pos,AddQuestion(type));
                    // Pas encore géré
                }
            }
            Console.WriteLine("Nombre de questions ajoutées : " + (questions.Count - amount +1 ));
        }


        /// <summary>
        /// Affiche la liste des questions 
        /// </summary>
        /// 
        ISocketMessageChannel ch;
        public async Task Preview(ISocketMessageChannel channel)
        {
            EmbedBuilder e = new EmbedBuilder();
            ch = channel;
            int id = 1;
            int SimpleID = 0;
            int MultipleID = 0;
            string stock = string.Empty;
            foreach (var q in questions)
            {
                if (q.type == QType.text)
                {
                    // problème sûr à 100% avec async await
                    e.Title = "Aperçu du QCM : " + name;
                    e.Color = Color.DarkBlue;
                    var eb = (EmbedBuilder)q.content;
                    e.AddField("Question " + id, q.name + " | " + q.type + " | " + eb.Title);
                    //if (q.type == QType.text)
                        //await channel.SendMessageAsync("", false,(EmbedBuilder) q.content);
                }
                else if (q.type == QType.image)
                {
                    Console.WriteLine();
                    // Images multiples
                    List<EmbedBuilder> eb = (List<EmbedBuilder>)q.content;
                    if (q.imageQuestion == ImageQuestion.CorrespondingImageMultiple)
                    {
                        e.AddField("Multiple IMG " + id, q.name + " | " + ebListMultiple[MultipleID].Title);
                        MultipleID++;
                    }
                    // Image unique
                    else
                    {
                        e.AddField("Unique IMG " + id, ebListSimple[SimpleID].Description + " | " + ebListSimple[SimpleID].Title);
                        SimpleID++;
                    }
                }
                else
                {
                    e.AddField("Type non généré : ", q.type );
                }
                id++;
            }
            await channel.SendMessageAsync("", false, e);
            foreach (var field in e.Fields)
            {
                Console.WriteLine(field.Name + " : " + field.Value);
            }
        }

        public void DeleteQuestion(int pos)
        {
            if(questions.Count()>0)
            questions.Remove(questions.Last());
        }

        public void DeleteQuestions(int beginPos, int amount)
        {
            
        }

        public void ReplaceQuestion(int pos, int newType)
        {
           
        }

        

        public void Display()
        {

        }

        public void SendNextQuestion()
        {

        }

        /// <summary>
        /// Vérifie la réponse fournie via le système de réactions
        /// </summary>


        public void StopQCM()
        {

        }

        public void MixQuestions()
        {
            var MixedList = questions.OrderBy(a => Guid.NewGuid()).ToList();
            foreach(var q in MixedList)
            {
                Console.WriteLine(q.name + "|"+ q.content); 
            }
            Console.WriteLine("\nOK\n");
            questions = MixedList;
        }
        /// <summary>
        /// Règle le volume général du Qcm
        /// </summary>
        public void SetVolume()
        {

        }
        /// <summary>
        /// Définit un temps limité pour répondre à chaque question du Qcm
        /// </summary>
        public void SetChrono()
        {

        }

        public void SetBotReactivity()
        {

        }
        /// <summary>
        /// Déclenche un chrono pour tout le Qcm en général
        /// </summary>
        public void SetChronoGlobal()
        {

        }

        public void AutoCreate()
        {

        }

        public void CreateResultFile()
        {

        }


    }


}
