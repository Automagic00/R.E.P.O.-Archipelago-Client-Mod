using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoAP
{
    static class LocationData
    {
        static int baseID = 75912022;
        static Dictionary<long, string> idToName;
        static Dictionary<string, long> nameToId;
        static long[] ids;
        static long[] names;

        public static void CreateLocationTables()
        {
            for (int i = 0; i < 100; i++)
            {

            }
        }

        public static long PellyNameToID(string name)
        {
            int offset = 100;

            //Add Pelly Level Type Offset
            if (name.Contains("Swiftbroom"))
            {
                offset += 1;
            }
            else if (name.Contains("Manor"))
            {
                offset += 4;
            }
            else if (name.Contains("McJannek"))
            {
                offset += 7;
            }

            //Add Pelly Type Offset
            if (name.Contains("Standard"))
            {
                offset += 0;
            }
            else if (name.Contains("Glass"))
            {
                offset += 1;
            }
            else if (name.Contains("Gold"))
            {
                offset += 2;
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
