using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RepoAP
{
    static class LocationData
    {
        const int baseID = 75912022;
        public const int pellyOffset = 100;
        public const int valuableOffset = 200;
        public const int monsterOffset = 500;
        //static Dictionary<long, string> idToName;
        //static Dictionary<string, long> nameToId;
        //static long[] ids;
        //static long[] names;

        public static void CreateLocationTables()
        {
            for (int i = 0; i < 100; i++)
            {

            }
        }

        public static string GetBaseName(string name)
        {
            name = name.Replace("Valuable ", "").Replace("(Clone)", "");
            foreach (string level in LocationNames.all_levels_short)
            {
                name = name.Replace($"{level} ", "");
            }
            return name;
        }
      
        public static long MonsterSoulNameToID(string name)
        {
            long id = 0;
            name = GetBaseName(name);

            id = LocationNames.all_monster_souls.IndexOf(name);

            if (id == -1)
            {
                Plugin.Logger.LogWarning($"{name}'s id not found");
                id = 0;
            }
            else
            {
                id += monsterOffset;
            }

            return baseID + id;
        }

        public static string MonsterSoulIDToName(long id)
        {
            id -= baseID;
            id -= monsterOffset;
            string name = LocationNames.all_monster_souls[(int)id];
            return name;
        }
        public static long ValuableNameToID(string name)
        {
            long id = 0;
            name = GetBaseName(name);

            id = LocationNames.all_valuables.IndexOf(name);

            if (id == -1)
            {
                Plugin.Logger.LogWarning($"{name}'s id not found");
                id = 0;
            }
            else
            {
                id += valuableOffset;
            }

            return baseID + id;
        }

        public static string ValuableIDToName(long id)
        {
            id -= baseID;
            id -= valuableOffset;
            string name = LocationNames.all_valuables[(int)id];
            return name;

        }

        public static long PellyNameToID(string name)
        {
            int offset = pellyOffset;
            int idx = 1;
            foreach (string level in LocationNames.all_levels_short)
            {
                if( name.Contains(level) )
                { 
                    offset += idx;
                    break;
                }
                idx += 3;
            }

            idx = 0;
            foreach (string pelly in LocationNames.all_pellys)
            {
               if (name.Contains(pelly))
               {
                   offset += idx;
                   break;
               }
               idx++;
            }

            return baseID + offset;
        }

        public static long ShopItemToID(string name)
        {
            if (name.Any(Char.IsDigit))
            {
                name = new string(name.Where(x => char.IsDigit(x)).ToArray());
                int offset = int.Parse(name);
                return baseID + offset;
            }
            else
            {
                return 0;
            }
        }

        public static long RemoveBaseId(long id)
        {
            return id - baseID;
        }
        public static long AddBaseId(long id)
        {
            return id + baseID;
        }


    }
}
