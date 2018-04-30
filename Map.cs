using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT
{
    public class Map
    {
        public List<Structure> allStructures = new List<Structure>();
        string schema = string.Empty;
        private int currentStructID = 0;
        public static int numberOfRoom = 0;
        enum StructureType { Room,/* RiskyPassage, SafePassage, TalismanPassage,*/ ThreePassages}
        public Map (int nbSalles)
        {
            for(int i = 0; i < nbSalles; i++)
            {
                    if(i != 0)   AddStructure(StructureType.ThreePassages);
                    AddStructure(StructureType.Room);
            }
            Console.WriteLine("// Terminé");
        }


        Random random = new Random();
        /// <summary>
        /// Permet d'autoriser un placement de pistolet dans l'une des salles avec une probabilité que le pistolet existe dans toute la partie  de 0,5
        /// </summary>
        public void PistolPossible(double proba = 0.5)
        {
            Console.WriteLine("Entrée PistolPossible()");
            var aleat = random.NextDouble();
            if(aleat <= proba)
            {
                PlacePistol();
            } 
            Console.WriteLine("PistolPossible(), le pistolet n'a pas été placé");
        }

        private void PlacePistol()
        {
            //foreach(var s in allStructures)
            //{
            //    if(s.GetType() == typeof(Room))
            //    {
            //        roomList.Add(s as Room);
            //    }
            //    Console.WriteLine("PlacePistol(), "+ roomList.Count + " salles détectées");
            //}

            Console.WriteLine("PlacePistol(), il y'a " + Map.numberOfRoom + " salles");
            int roomWithPistolID = random.Next(0, Map.numberOfRoom);

            // On ne parcourt que les salles 
            int i = 0;
            foreach(Room s in allStructures.OfType<Room>())
            {
                if(i == roomWithPistolID)
                {
                    s.hasPistol = true;
                }
                i++;
            }

            // Affichage 
            Console.WriteLine("\n\n---------------------------------------------");
            var roomList = new List<Room>();
            roomList = allStructures.OfType<Room>().ToList();
            int j = 1;
                foreach(var r in roomList)
                {
                Console.WriteLine("Salle: " + j);
                if (r.hasPistol)
                    {
                        Console.WriteLine("Contient un pistolet");
                    }
                    else
                    {
                        Console.WriteLine("Ne contient pas un pistolet");
                    }
                j++;
                }
            Console.WriteLine("---------------------------------------------\n\n");

        }

        public string GetMiniMap()
        {
            // On remet le schéma à 0 pour ne pas faire que le nouveau se colle au précédent
            schema = string.Empty;
            foreach(var s in allStructures)
            {
                if(s == allStructures.First())
                {
                    schema += "(Entree) -->" ;
                }
                if (s.GetType() == typeof(Room))
                {
                    schema += "(Salle)";
                }
                else if (s.GetType() == typeof(Passages))
                {
                    schema += "(3 passages)";
                }
                schema += " --> ";
                if (s == allStructures.Last())
                {
                    schema += "(Sortie)";
                }
            }
            Console.WriteLine(schema);
            Console.WriteLine("Remarque: (Entree) et (Sortie) sont ici présents à titre indicatif");
            return schema;
        }

        private void AddStructure(StructureType structure)
        {
            if(structure == StructureType.Room)
            {
                Room room = new Room();
                allStructures.Add(room);
            }
            else if(structure == StructureType.ThreePassages)
            {
                Passages pass = new Passages();
                allStructures.Add(pass);
            }
            else
            {
                Console.WriteLine("Type de structure pas encore géré");
            }
        }

        private void DeleteStructure()
        {

        }

        private void MixAllStructures()
        {

        }

        public async Task MoveToNextStructure(SocketCommandContext context)
        {

        }

        public void MoveToPrecedentStructure()
        {

        }

        

    }
}
