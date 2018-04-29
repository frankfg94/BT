using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT
{
    class Map
    {
        public  List<Structure> allStructures = new List<Structure>();
        string schema = string.Empty;
        private int currentStructID = 0;
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
            Console.WriteLine("Enter1");
            await allStructures[currentStructID].ShowIllustration(context);
            currentStructID++;
        }

        public void MoveToPrecedentStructure()
        {

        }

        

    }
}
