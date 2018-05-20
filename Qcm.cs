using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        public DisplayMode displayMode = DisplayMode.QCM;
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
            public string answerLetter =    "🇦";
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


        int textQuestionNumber = 0;
        const int qCount = 12;
        List<int> uniqueList = new List<int>();
        public List<ulong> questionsID = new List<ulong>();
        /// <summary>
        /// Ajoute une question de type texte, image ou audio à la position indiquée, si aucune n'est précisée, la question sera rajoutée à la fin du Qcm
        /// </summary>
        /// 

        public Question AddQuestion(EmbedBuilder content)
        {
            Question qToAdd = null;
            qToAdd.content = content;
            return qToAdd;
        }


        public Question AddQuestion(int type, bool SingleImage = false, TextQuestion tq = TextQuestion.MathContent, int pos = -1)
        {
            Question qToAdd = null;
            if (type == QType.text)
            {
                if (tq == TextQuestion.MathContent)
                {
                    CalculType[] valeurs = { CalculType.Addition, CalculType.Multiplication, CalculType.Soustraction };
                    CalculType CalculTypeAleatoire = valeurs[r.Next(valeurs.Length)];
                    qToAdd = new Question(null, type, "Question texte Maths");
                    qToAdd.content = GenerateMathContent(qToAdd, CalculTypeAleatoire);
                }
                else if (tq == TextQuestion.DefaultContent)
                {
                    List<int> IDAll = new List<int>();

                    qToAdd = new Question(null, type, null, "Question Texte");
                    // On ajoute les questions dans l'ordre 
                    int newRandom = n.Next(0, qCount);
                    while (uniqueList.Contains(newRandom))
                    {
                        newRandom = n.Next(0, qCount);
                        // On cherche au hasard tant que l'on a pas un bon random
                    }
                    uniqueList.Add(newRandom);
                    qToAdd.content = GenerateDefaultContent(qToAdd, uniqueList[textQuestionNumber]);
                    textQuestionNumber++;
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
            else if(type == QType.audio)
            {
                Console.WriteLine("Gen");
                qToAdd = new Question(null, type, "Question audio");
                qToAdd.content = GenerateAudioContent(qToAdd);
                Console.WriteLine("Gen à priori ok");
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

        private object GenerateAudioContent(Question qToAdd)
        {
            List<FileInfo> audioFiles = new List<FileInfo>();
            try
            {
                // Dossier où sont placées les musiques
                DirectoryInfo dinfo = new DirectoryInfo("./BlindTest");

                // On peut obtenir des sons pour le moment dans ces deux formats
                FileInfo[] Files = dinfo.GetFiles("*.mp3");
                FileInfo[] Files2 = dinfo.GetFiles("*.wav");

                foreach (FileInfo f in Files)
                {
                    audioFiles.Add(f);
                    Console.WriteLine(f.Name);
                }
                foreach (FileInfo f in Files2)
                {
                    audioFiles.Add(f);
                    Console.WriteLine(f.Name);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }


            var file = audioFiles[r.Next(audioFiles.Count)];

            string a, b, c, d ;
            a = b = c = d = null;
            string correctAnswer = "Erreur, pas de réponse fournie";

            // Génération des reponses
            switch (file.Name)
            {
                // Exemples de sons pour blindtest
                case "Zombie.mp3":
                    a = "Macklemore";
                    b = "Zombie";
                    c = "Marroon 5";
                    d = "Ed Sheeran";
                    correctAnswer = b;
                    break;

                case "Mackle.mp3" :
                    a = "Macklemore";
                    b = "B.o.B";
                    c = "Vic Mensa";
                    d = "Logic";
                    correctAnswer = a;
                    break;
                case "cWinston.mp3":
                    a = "Charlie Winston";
                    b = "Yodelice";
                    c = "Ben Harper";
                    d = "Jehro";
                    correctAnswer = a;
                    break;

                case "Trophée.mp3":
                    a = "Vald";
                    b = "Niska";
                    c = "Fianso";
                    d = "Hugo TSR";
                    correctAnswer = a;
                    break;

                case "Motel.mp3":
                    a = "Maitre Gims";
                    b = "S.Pri Noir";
                    c = "Lefa";
                    d = "Niska";
                    correctAnswer = c;
                    break;

                case "Hugo_Tsr.mp3":
                    a = "Hugo TSR";
                    b = "Maska";
                    c = "MHD";
                    d = "Nekfeu";
                    correctAnswer = a;
                    break;

                default:
                    // Si jamais il y'a une erreur dans le nom, ce message s'affiche pour la signaler au développeur
                    Console.WriteLine("Réponses du fichier audio : " + file.Name + " non configurées");
                    break;
            }

            List<string> initAnswers = new List<string>();
            initAnswers.Add(a);
            initAnswers.Add(b);
            initAnswers.Add(c);
            initAnswers.Add(d);



            // On enregistre la bonne réponse de type texte
            qToAdd.answer = correctAnswer;


            // Mélange des réponses dans une nouvelle liste
            var randomAnswers = initAnswers.OrderBy(elem => Guid.NewGuid()).ToList();

            // On enregistre la bonne icone de réponse
            if (randomAnswers[0] == correctAnswer)
            {
                qToAdd.answerLetter = ":regional_indicator_a:";
            }
            else if (randomAnswers[1] == correctAnswer)
            {
                qToAdd.answerLetter = ":regional_indicator_b:";
            }
            else if (randomAnswers[2] == correctAnswer)
            {
                qToAdd.answerLetter = ":regional_indicator_c:";
            }
            else if (randomAnswers[3] == correctAnswer)
            {
                qToAdd.answerLetter = ":regional_indicator_d:";
            }

            EmbedBuilder eb = new EmbedBuilder();
            eb.Title = "Qui est l'auteur de cette musique ?";
            eb.WithDescription("a) " + randomAnswers[0] + "\nb) " + randomAnswers[1] + "\nc) " + randomAnswers[2] + "\nd) " + randomAnswers[3]);

            object[] data = new object[2];

            // On retourne l'embed
            data[0] =  eb ;
            // On retourne le nom du fichier
            data[1] = file.Name;
            
            return data;
        }

        List<EmbedBuilder> ebListSimple = new List<EmbedBuilder>();
        List<EmbedBuilder> ebListMultiple = new List<EmbedBuilder>();
        List<string> recognizeThingsPictures = new List<string>(new string[] { "Tank", "JeanneA", "JeuneFillePerle", "SpireDublin", "SacreNapo", "Alkpote", "Acteur", "Coree", "Spiderman", "Oiseau" });
        Random r = new Random();
        private List<EmbedBuilder> GeneratePictureContent(Question q, ImageQuestion type = ImageQuestion.RecognizePictureSingle)
        {
            EmbedBuilder imgEb1 = new EmbedBuilder();
            if (type == ImageQuestion.RecognizePictureSingle)
            {
                Console.WriteLine("Création d'une image avec 4 réponses a) b) c) d)");
                int randomNumber = r.Next(0, recognizeThingsPictures.Count());
                string url = null;
                string name = recognizeThingsPictures[randomNumber];
                string a, b, c, d;
                string title = "Que représente cette image ?";
                string answer = "Non définie";
                a = b = c = d = "N/A";
                switch (name)
                {
                    case "Tank":
                        url = "https://www.industrie-techno.com/mediatheque/5/4/4/000012445_imageArticlePrincipaleLarge.jpg";
                        a = "Un prototype Polonais en construction";
                        b = "Un char apparu dans le film Bladde Runner 2049";
                        c = "Un Fan-Art";
                        d = "Un nouveau char américain";
                        answer = a;
                        break;
                    case "JeanneA":
                        url = "http://www.musee-orsay.fr/typo3temp/zoom/tmp_828c5c0a84bf57e06bc87366dc30b237.gif";
                        title = "Que représente cette statue";
                        a = "Une jeune paysanne priant pour son mari";
                        b = "Jeanne D'Arc";
                        c = "Fernande Olivier";
                        d = "Camille Claudel";
                        answer = b;
                        break;
                    case "JeuneFillePerle":
                        // lien ne marche pas
                        url = "https://cdn.radiofrance.fr/s3/cruiser-production/2016/05/5791f11c-4124-45b9-b493-2bd99aace836/738_meisje_met_de_parel.jpg";
                        title = "Quel est l'auteur de ce tableau intitulé 'La jeune fille à la Perle'?";
                        a = "Monet";
                        b = "Vermeer";
                        c = "Peeter de Hooch";
                        d = "Van Gogh";
                        answer = b;
                        break;
                    case "SpireDublin":
                        title = "Quel représente ce monument?";
                        url = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1b/E4324-Spire-of-Dublin.jpg/390px-E4324-Spire-of-Dublin.jpg";
                        a = "La Spire de Dublin";
                        b = "L'épine de Berlin ";
                        c = "Le pic de Berlin";
                        d = "La Pointe de Dublin";
                        answer = a;
                        break;
                    case "SacreNapo":
                        title = "En quelle année s'est déroulée cette scène?";
                        url = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/08/Jacques-Louis_David%2C_The_Coronation_of_Napoleon_edit.jpg/1200px-Jacques-Louis_David%2C_The_Coronation_of_Napoleon_edit.jpg";
                        a = "1806";
                        b = "1807";
                        c = "1808";
                        d = "1805";
                        answer = a;
                        break;
                    case "Alkpote":
                        title = "Quel est le nom de ce rappeur?";
                        url = "https://www.trunksmagazine.com/wp-content/uploads/2017/07/86d0dd081a35d7e08781c2c889b6a788.1000x1000x1.jpg";
                        a = "Ninho";
                        b = "Lartiste";
                        c = "Alkpote";
                        d = "Sadek";
                        answer = c;
                        break;
                    case "Acteur":
                        title = "Quel est le nom de l'acteur sur ce meme?";
                        url = "http://www.tuxboard.com/photos/2017/02/Kayode-Ewumi-meme-720x416.jpg";
                        a = "Chadwick Boseman";
                        b = "Will Smith";
                        c = "Daniel Kaluuya";
                        d = "Kayode Ewumi";
                        answer = d;
                        break;
                    case "Coree":
                        title = "De quand date cette image?";
                        url = "https://cdn.cnn.com/cnnnext/dam/assets/180426203126-06-korea-summit-0426-exlarge-169.jpg";
                        a = "avril 2017";
                        b = "avril 2018";
                        c = "mars 2018";
                        d = "mai 2017";
                        answer = b;
                        break;
                    case "Spiderman":
                        title = "Quel est l'acteur principal de cette série de films?";
                        url = "https://vignette.wikia.nocookie.net/spiderman/images/5/52/Spider-Man.jpg/revision/latest?cb=20131201182543";
                        a = "Tobey Maguire";
                        b = "Andrew Garfield";
                        c = "Tom Holland";
                        answer = a;
                        break;
                    case "Oiseau":
                        title = "Quel est cet oiseau?";
                        url = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/Black_eagle.jpg/290px-Black_eagle.jpg";
                        a = "Aigle royal";
                        b = "Aigle gris";
                        c = "Aigle noir";
                        d = "Buse noire";
                        answer = c;
                        break;
                }

                // On met les réponses dans l'ordre dans une première liste
                List<string> initAnswers = new List<string>
                {
                    a,
                    b,
                    c,
                    d
                };

                // On enregistre la bonne réponse de type texte
                q.answer = answer;


                // Mélange des réponses dans une nouvelle liste
                var randomAnswers = initAnswers.OrderBy(elem => Guid.NewGuid()).ToList();


                // On enregistre la bonne icone de réponse
                if (randomAnswers[0] == answer)
                {
                    q.answerLetter = ":regional_indicator_a:";
                }
                else if (randomAnswers[1] == answer)
                {
                    q.answerLetter = ":regional_indicator_b:";
                }
                else if (randomAnswers[2] == answer)
                {
                    q.answerLetter = ":regional_indicator_c:";
                }
                else if (randomAnswers[3] == answer)
                {
                    q.answerLetter = ":regional_indicator_d:";
                }

                // Ajout du titre à l'embed
                imgEb1.WithTitle(title);

                // Création du corps à l'aide des réponses placées aléatoirement
                imgEb1.WithDescription("a) " + randomAnswers[0] + "\nb) " + randomAnswers[1] + "\nc) " + randomAnswers[2] + "\nd) " + randomAnswers[3]);
                // Ajout d'une image à l'embed
                imgEb1.WithImageUrl(url);
                // Ajout de l'embed à la liste des embeds
                ebListSimple.Add(imgEb1);
                // On précise le type de cette question
                q.imageQuestion = type;

                // On peut enfin retourner notre embed terminé
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

                        ebListMultiple.Add(imgEb1.WithImageUrl(url1).WithTitle("Réponse :regional_indicator_a:"));
                        ebListMultiple.Add(imgEb2.WithImageUrl(url2).WithTitle("Réponse :regional_indicator_b:"));
                        ebListMultiple.Add(imgEb3.WithImageUrl(url3).WithTitle("Réponse :regional_indicator_c:"));
                        ebListMultiple.Add(imgEb4.WithImageUrl(url4).WithTitle("Réponse :regional_indicator_d:"));
                        q.name = "Lequel de ces tableaux n'a pas été peint par Vermeer?"; // Ce qui est affiché trop de fois d'affilée
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
                case 2:
                    {
                        question = "À quel site appartient le fameux forum connu sous le nom de 18-25 ?";
                        imageUrl = "https://www.nouvelordremondial.cc/wp-content/uploads/2017/11/18-25-elite.jpg";
                        a = "Doctissimo.com";
                        b = "Jeuxvideo.com";
                        c = "Koreus.com";
                        d = "GOG.com";
                        correctTextAnswer = b;
                        break;
                    }
                case 3:
                    {
                        question = "Quel acteur a obtenu un Oscar avec le film 'The Artist' en 2012 ?";
                        imageUrl = "https://www.nouvelordremondial.cc/wp-content/uploads/2017/11/18-25-elite.jpg";
                        a = "Omar Sy";
                        b = "Benoit Poelvoorde";
                        c = "Vincent Cassel";
                        d = "Jean Dujardin";
                        correctTextAnswer = d;
                        break;
                    }
                case 4:
                    {
                        question = "Quel élément a causé la première crise spéculative ?";
                        imageUrl = "https://www.breizh-info.com/wp-content/uploads/2015/08/Un_krach_boursier_annonciateur_de-r%C3%A9cession.jpg";
                        a = "L'or";
                        b = "Le charbon";
                        c = "Le bulbe de tulipe";
                        d = "La soie";
                        correctTextAnswer = c;
                        break;
                    }
                case 5:
                    {
                        question = "A quelle hauteur se situe le point culminant du Mont Blanc ?";
                        imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/4b/Mont_Blanc_depuis_Valmorel_2.jpg/1200px-Mont_Blanc_depuis_Valmorel_2.jpg";
                        a = "4 804 m";
                        b = "4 810 m";
                        c = "4 408 m";
                        d = "4 410 m";
                        correctTextAnswer = b;
                        break;
                    }
                case 6:
                    {
                        question = "Quel album de musique est le plus vendu de tous les temps ?";
                        a = "'Metallica' de Metallica";
                        b = "'Thriller' de Michael Jackson";
                        c = "'Back in Black' de AC/DC";
                        d = "'The Marshall Mathers LP' de Eminem";
                        correctTextAnswer = b;
                        break;
                    }
                case 7:
                    {
                        question = "Quand est sorti le long métrage de Disney 'Blanche - Neige' ?";
                        //imageUrl = "https://www.nouvelordremondial.cc/wp-content/uploads/2017/11/18-25-elite.jpg";
                        a = "1935";
                        b = "1936";
                        c = "1937";
                        d = "1938";
                        correctTextAnswer = c;
                        break;
                    }
                case 8:
                    {
                        question = "Quels dirigeants étaient présents à la conférence de Yalta ?";
                        a = "Staline, Churchill et Roosevelt";
                        b = "Staline, Hitler et Churchill";
                        c = "De Gaulle, Churchill et Roosevelt";
                        d = "De Gaulle, Staline et Churchill";
                        correctTextAnswer = a;
                        break;
                    }
                case 9:
                    {
                        question = "En quelle année s'est éteint le cougar américain ?";
                        imageUrl = "http://img-aws.ehowcdn.com/750x428p/photos.demandstudios.com/getty/article/94/171/ALI_066_XS.jpg";
                        a = "2009";
                        b = "2012";
                        c = "2007";
                        d = "2011";
                        correctTextAnswer = c;
                        break;
                    }
                case 10:
                    {
                        question = "Quelle est cette région ?";
                        imageUrl = "https://www.nouvelordremondial.cc/wp-content/uploads/2017/11/18-25-elite.jpghttps://upload.wikimedia.org/wikipedia/commons/thumb/6/69/Lorraine_region_locator_map.svg/1200px-Lorraine_region_locator_map.svg.png";
                        a = "Aquitaine";
                        b = "Alsace";
                        c = "Lorraine";
                        d = "Vendée";
                        correctTextAnswer = c;
                        break;
                    }
                case 11:
                    {
                        question = "Qui est-ce ?";
                        imageUrl = "http://www.crainsnewyork.com/apps/pbcsi.dll/storyimage/CN/20140702/REAL_ESTATE/140709973/AR/0/Larry-Silverstein.jpg";
                        a = "Bill Gates";
                        b = "Warren Buffett";
                        c = "Larry Ellison";
                        d = "Larry Silverstein";
                        correctTextAnswer = c;
                        break;
                    }
                case 12:
                    {
                        question = "Quel est le nom de ce groupe de musique ?";
                        imageUrl = "http://www.kultplus.com/wp-content/uploads/2017/10/rs-metallica-1d77d17a-78a8-4da2-b52a-5cc734e8f2db.jpg";
                        a = "Metallica";
                        b = "Eagles";
                        c = "AC/DC";
                        d = "Scorpions";
                        correctTextAnswer = c;
                        break;
                    }
                default:
                    {
                        question = "Index incorrect";
                        a = b = c = d = "N/A";
                        break;
                    }

            }

            possibleTextAnswers[0] = a;
            possibleTextAnswers[1] = b;
            possibleTextAnswers[2] = c;
            possibleTextAnswers[3] = d;
            data.Add(possibleTextAnswers);
            data.Add(question);
            data.Add(imageUrl);
            data.Add(correctTextAnswer);
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

        public EmbedBuilder GenerateDefaultContent(Question q, int id)
        {
            // Importation des données, et conversion dans le format de Discord
            var data = ImportDataForDefaultContent(id);
            return ConvertToEmbed(data);
        }

        public EmbedBuilder GenerateMathContent(Question q, CalculType t = CalculType.Multiplication)
        {
            var eb = new EmbedBuilder();
            if (t == CalculType.Multiplication)
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
            Console.WriteLine("Reach 1.5");
            q.answer = result.ToString();
            List<int> listToRandomizeAnswers = new List<int>();
            Console.WriteLine("Reach2");
            listToRandomizeAnswers.Add(result);
            listToRandomizeAnswers.Add(result - 1);
            listToRandomizeAnswers.Add(result + 1);
            listToRandomizeAnswers.Add(result + 3);
            var listMel = listToRandomizeAnswers.OrderBy(a => Guid.NewGuid()).ToList();
            Console.WriteLine("Reach3   ");
            eb.AddField("a) ", listMel[0]);
            eb.AddField("b) ", listMel[1]);
            eb.AddField("c) ", listMel[2]);
            eb.AddField("d) ", listMel[3]);
            if (listMel[0] == result)
            {
                q.answerLetter = ":regional_indicator_a:";
            }
            else if (listMel[1] == result)
            {
                q.answerLetter = ":regional_indicator_b:";
            }
            else if (listMel[2] == result)
            {
                q.answerLetter = ":regional_indicator_c:";
            }
            else if (listMel[3] == result)
            {
                q.answerLetter = ":regional_indicator_d:";
            }
            Console.WriteLine("Reach4");

            Console.WriteLine("Bonne réponse : " + q.answerLetter + ":" + q.answer);

            // On Enleve avec RemoveAt() l'élément pour ne pas pouvoir le reprendre

            eb.WithColor(Color.DarkGreen);
            return eb;
        }

        private int SimpleID = 0;

        public enum DisplayMode
            {
                QCM,
                JDR_Passage
            }

        AudioModule audioModule;
        public async Task<IMessage> DisplayInDiscord(ISocketMessageChannel channel, Question question, DisplayMode display  = DisplayMode.QCM, AudioModule am = null)
        {

            Console.WriteLine("Fonction d'affichage lancée ( Nom : DisplayInDiscord ) ");
            IUserMessage msg;

            if(display == DisplayMode.QCM)
            {
                if (question.type == QType.text)
                {
                    EmbedBuilder eb = (EmbedBuilder)question.content;
                    msg = await channel.SendMessageAsync("DisplayInDiscord()", false, eb);
                    await AddQcmReactions(msg);
                }
                else if (question.type == QType.image)
                {
                    Console.WriteLine("Tentative d'affichage pour type image...");
                    if (question.content.GetType() == typeof(List<EmbedBuilder>)) // si plusieurs embeds et donc plusieurs iamges
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
                else if (question.type == QType.audio)
                {
                    Console.WriteLine(">>>Question Audio");
                    EmbedBuilder eb = ((question.content as object[])[0] as EmbedBuilder);
                    msg = await channel.SendMessageAsync("DisplayInDiscord()", false, eb);
                    try
                    {
                        // Permet d'empêcher l'audio module d'être recrée, ce qui entraînerait une exception
                        if (audioModule == null)
                            audioModule = am;
                        Thread t = new Thread(async () => await audioModule.PlayMusic("./BlindTest/" + ((question.content as object[])[1] as string)));
                        t.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    await AddQcmReactions(msg);

                    
                    Console.WriteLine(">>>Succès");
                }
                else
                {
                    await Console.Out.WriteLineAsync("Pas de type texte ou image");
                    msg = await channel.SendMessageAsync("Questions de type : " + question.type + " pas encore gérées");
                    await AddQcmReactions(msg, true);
                }
                return msg;
            }

            else
            {
                Console.WriteLine("A faire");
                return null;
            }
            
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
            }

        }

        public async Task AddQcmReactionCustom(IUserMessage msg, string unicode)
        {

                await msg.AddReactionAsync(new Emoji(unicode));

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
            AudioService audioService = (AudioService)Program._services.GetService(typeof(AudioService));
            AudioModule am = new AudioModule(audioService, (SocketCommandContext) audioService.Context);
            try { await am.Music1(); }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
                    e.AddField("Type non géré par le système de prévisualisation: ", q.type );
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
