using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        public static long MonsterSoulNameToID(string name)
        {
            long id = 0;
            name = name.Replace("(Clone)", "");
            switch (name)
            {
                case LocationNames.animal_soul: id = 300; break;
                case LocationNames.duck_soul: id = 301; break;
                case LocationNames.bowtie_soul: id = 302; break;
                case LocationNames.chef_soul: id = 303; break;
                case LocationNames.clown_soul: id = 304; break;
                case LocationNames.headman_soul: id = 305; break;
                case LocationNames.hidden_soul: id = 306; break;
                case LocationNames.huntsman_soul: id = 307; break;
                case LocationNames.mentalist_soul: id = 308; break;
                case LocationNames.reaper_soul: id = 309; break;
                case LocationNames.robe_soul: id = 310; break;
                case LocationNames.rugrat_soul: id = 311; break;
                case LocationNames.shadow_child_soul: id = 312; break;
                case LocationNames.spewer_soul: id = 313; break;
                case LocationNames.trudge_soul: id = 314; break;
                case LocationNames.upscream_soul: id = 315; break;
            }


            if (id == 0)
            {
                Debug.Log($"{name}'s id not found");
            }

            return baseID + id;
        }
        public static long ValuableNameToID(string name)
        {
            long id = 0;
            name = name.Replace("Arctic ", "").Replace("Wizard ", "").Replace("Valuable ", "").Replace("(Clone)","");
            switch(name)
            {
                case LocationNames.diamond: id = 200; break;
                case LocationNames.ring: id = 201; break;
                case LocationNames.goblet: id = 202; break;
                case LocationNames.ocarina: id = 203; break;
                case LocationNames.pocket_watch: id = 204; break;
                case LocationNames.uranium_mug: id = 205; break;
                case LocationNames.crown: id = 206; break;
                case LocationNames.doll: id = 207; break;
                case LocationNames.explosive_barrel: id = 208; break;
                case LocationNames.frog_toy: id = 209; break;
                case LocationNames.gem_box: id = 210; break;
                case LocationNames.globe: id = 211; break;
                case LocationNames.money: id = 212; break;
                case LocationNames.monkey: id = 213; break;
                case LocationNames.uranium_plate: id = 214; break;
                case LocationNames.small_vase: id = 215; break;
                case LocationNames.champagne: id = 216; break;
                case LocationNames.clown_doll: id = 217; break;
                case LocationNames.radio: id = 218; break;
                case LocationNames.ship_in_a_bottle: id = 219; break;
                case LocationNames.trophy: id = 220; break;
                case LocationNames.vase: id = 221; break;
                case LocationNames.tv: id = 222; break;
                case LocationNames.large_vase: id = 223; break;
                case LocationNames.animal_crate: id = 224; break;
                case LocationNames.bonsai: id = 225; break;
                case LocationNames.music_box: id = 226; break;
                case LocationNames.gramophone: id = 227; break;
                case LocationNames.rhino: id = 228; break;
                case LocationNames.scream_doll: id = 229; break;
                case LocationNames.grand_piano: id = 230; break;
                case LocationNames.harp: id = 231; break;
                case LocationNames.painting: id = 232; break;
                case LocationNames.grandfather_clock: id = 233; break;
                case LocationNames.dinosaur_skeleton: id = 234; break;
                case LocationNames.golden_statue: id = 235; break;
                case LocationNames.desktop_computer: id = 236; break;
                case LocationNames.fan: id = 237; break;
                case LocationNames.sample: id = 238; break;
                case LocationNames.big_sample: id = 239; break;
                case LocationNames.flamethrower: id = 240; break;
                case LocationNames.science_station: id = 241; break;
                case LocationNames.server: id = 242; break;
                case LocationNames.printer: id = 243; break;
                case LocationNames.hdd: id = 244; break;
                case LocationNames.ice_saw: id = 245; break;
                case LocationNames.laptop: id = 246; break;
                case LocationNames.propane: id = 247; break;
                case LocationNames.sample_pack: id = 248; break;
                case LocationNames.guitar: id = 249; break;
                case LocationNames.sample_cooler: id = 250; break;
                case LocationNames.leg_ice: id = 251; break;
                case LocationNames.skeleton_ice: id = 252; break;
                case LocationNames.chomp_book: id = 253; break;
                case LocationNames.love_potion: id = 254; break;
                case LocationNames.cube_of_knowledge: id = 255; break;
                case LocationNames.staff: id = 256; break;
                case LocationNames.large_sword: id = 257; break;
                case LocationNames.broom: id = 258; break;
                case LocationNames.hourglass: id = 259; break;
                case LocationNames.master_potion: id = 260; break;
                case LocationNames.goblin_head: id = 261; break;
                case LocationNames.griffin: id = 262; break;
                case LocationNames.power_crystal: id = 263; break;
            }



            if(id == 0)
            {
                Debug.Log($"{name}'s id not found");
            }

            return baseID + id;
        }
        public static long PellyNameToID(string name)
        {
            int offset = 100;

            //Add Pelly Level Type Offset
            if (name.Contains("Wizard")) //Swiftbroom Academy
            {
                offset += 1;
            }
            else if (name.Contains("Manor"))
            {
                offset += 4;
            }
            else if (name.Contains("Arctic")) //McJannek Station
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
