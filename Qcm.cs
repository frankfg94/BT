using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot_Test
{
 public static class QType
    {
        public const int text = 0;
        public const int image = 1;
        public const int audio = 2;
        public const int random = 99;
    }

 public class Qcm : ModuleBase<SocketCommandContext>
    {
        public string name = "defaut";
        public bool HasStarted = false;
        
           public enum Answers
            {
                A = 0,
                B = 1,
                C = 2,
                D = 3
            }


        public class Question : ModuleBase<SocketCommandContext>
        {
            public int type;
            public object content;
            public new string name = "Sans nom";
            public string answer = "🇦";
            public bool answered = false;
            public Question(object _answer, int _type = QType.text, object _content = null, string Qname = "Sans nom")
            {
                type = _type;
                content = _content;
                name = Qname;

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


        /// <summary>
        /// Ajoute une question de type texte, image ou audio à la position indiquée, si aucune n'est précisée, la question sera rajoutée à la fin du Qcm
        /// </summary>
        public void AddQuestion(int type, int pos = -1)
        {
            Question qToAdd;
            if (type == QType.text)
            {
                qToAdd = new Question(null,type, "Question texte")
                {
                    content = QcmTextSample()
                };
            }
            else if (type == QType.image)
                qToAdd = new Question(null,type, "Question visuelle");
            else
                qToAdd = new Question(null,type, "Autre");

            if (pos < 0)
            {
                questions.Add(qToAdd);
                // Console.WriteLine(qToAdd.name + "|" + qToAdd.type + "|" + qToAdd.content);
            }
            else
            {
                questions.Insert(pos, qToAdd);
            }
        }
        int result;
        Random n = new Random();
        public EmbedBuilder QcmTextSample()
        {
            var eb = new EmbedBuilder();
            int a = n.Next(1, 20);
            int b = n.Next(1, 20);
            result = a * b;
            eb.WithTitle("QCM basique de mathématiques\n" + a + " * " + b + " = ? ");
            eb.AddField("a) ", result);
            eb.AddField("b) ", (result - 1));
            eb.AddField("c) ", (result + 1));
            eb.AddField("d) ", (result + 3));
            eb.WithColor(Color.DarkGreen);
            return eb;
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
                    //questions.Insert(pos);
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
            int id = 0;
            foreach (var q in questions)
            {
                // problème sûr à 100% avec async await
                e.Title = "Aperçu";
                e.Color = Color.DarkBlue;
                e.AddField("Question " + id, q.name + "|" + q.type + "|" + q.content);
                if(q.type == QType.text)
                await channel.SendMessageAsync("", false,(EmbedBuilder) q.content);
                id++;
            }
            await channel.SendMessageAsync("test", false, e);
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
