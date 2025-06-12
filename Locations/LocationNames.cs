using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace RepoAP
{
    class LocationNames
    {
        //Pelly Names
        static public string standard_pelly = "Standard";
        static public string glass_pelly = "Glass";
        static public string gold_pelly = "Gold";

        //Shop Location Names
        static public string upgrade_pur = "Upgrade Purchase";

        //Level Type Names
        static public string swiftbroom = "Swiftbroom Academy ";
        static public string headman_manor = "Headman Manor ";
        static public string mcjannek = "McJannek Station ";
        static public string museum = "Museum of Human Art ";

        //---- Valuables ----
        //-- All --
        public const string diamond = "Diamond";
        public const string ring = "Emerald Bracelet";
        public const string goblet = "Goblet";
        public const string ocarina = "Ocarina";
        public const string pocket_watch = "Pocket Watch";
        public const string uranium_mug = "Uranium Mug";
        public const string crown = "Crown";
        public const string doll = "Doll";
        public const string frog_toy = "Frog";
        public const string gem_box = "Gem Box";
        public const string globe = "Globe";
        public const string money = "Money";
        public const string monkey = "Toy Monkey";
        public const string uranium_plate = "Uranium Plate";
        public const string small_vase = "Vase Small";
        public const string champagne = "Bottle";
        public const string clown_doll = "Clown";
        public const string radio = "Radio";
        public const string ship_in_a_bottle = "Ship in a bottle";
        public const string trophy = "Trophy";
        public const string vase = "Vase";
        public const string tv = "Television";
        public const string large_vase = "Vase Big";
        public const string animal_crate = "Animal Crate";
        public const string bonsai = "Bonsai";

        //-- Heaman Manor --
        public const string music_box = "Music Box";
        public const string gramophone = "Gramophone";
        public const string rhino = "Diamond Display";
        public const string scream_doll = "Scream Doll";
        public const string grand_piano = "Piano";
        public const string harp = "Harp";
        public const string painting = "Painting";
        public const string grandfather_clock = "Grandfather Clock";
        public const string dinosaur_skeleton = "Dinosaur";
        public const string golden_statue = "Golden Statue";

        //-- McJannek Station --
        public const string desktop_computer = "Computer";
        public const string fan = "Fan";
        public const string explosive_barrel = "Barrel";
        public const string sample = "Sample";
        public const string big_sample = "Big Sample";
        public const string flamethrower = "Flamethrower";
        public const string science_station = "Science Station";
        public const string server = "Server Rack";
        public const string printer = "3D Printer";
        public const string hdd = "HDD";
        public const string ice_saw = "Ice Saw";
        public const string laptop = "Laptop";
        public const string propane = "Propane Tank";
        public const string sample_pack = "Sample Six Pack";
        public const string guitar = "Guitar";
        public const string sample_cooler = "Sample Cooler";
        public const string leg_ice = "Creature Leg";
        public const string skeleton_ice = "Ice Block";

        //-- Swiftbroom Academy --
        public const string chomp_book = "Chomp Book";
        public const string love_potion = "Love Potion";
        public const string cube_of_knowledge = "Cube of Knowledge";
        public const string staff = "Dumgolfs Staff";
        public const string large_sword = "Sword";
        public const string broom = "Broom";
        public const string hourglass = "Time Glass";
        public const string master_potion = "Master Potion";
        public const string goblin_head = "Goblin Head";
        public const string griffin = "Griffin Statue";
        public const string power_crystal = "Power Crystal";

        //-- Museum of Human Art --
        public const string egg = "Egg";
        public const string car = "Car";
        public const string banana_bow = "Banana Bow";
        public const string boombox = "Boombox";
        public const string cool_brain = "Cool brain";
        public const string cubic_tower = "Cubic Tower";
        public const string fish = "Fish";
        public const string silverfish = "Silverfish";
        public const string goldfish = "Goldfish";
        public const string golden_swirl = "Golden Swirl";
        public const string pacifier = "Pacifier";
        public const string baby_head = "Baby Head";
        public const string monkeybox = "MonkeyBox";
        public const string flesh_blob = "Flesh Blob";
        public const string tooth = "Tooth";
        public const string goldtooth = "GoldTooth";
        public const string toast = "Toast";
        public const string horse = "Horse";
        public const string blender = "Blender";
        public const string mug_deluxe = "Uranium Mug Deluxe";
        public const string gumball = "Gumball";
        public const string traffic_light = "Traffic Light";
        public const string milk = "Milk";
        public const string handface = "Handface";
        public const string gem_burger = "Gem Burger";
        public const string wire_figure = "Wire Figure";
        public const string rubendoll = "Ruben Doll";

        public static readonly ReadOnlyCollection<string> all_pellys = new ReadOnlyCollection<string>(new List<string>()
        {
           standard_pelly,
           glass_pelly,
           gold_pelly
        });

        public static readonly ReadOnlyCollection<string> all_levels_short = new ReadOnlyCollection<string>(new List<string>()
        {
           "Wizard",
           "Manor",
           "Arctic",
           "Museum"
        });

        public static readonly ReadOnlyCollection<string> all_levels = new ReadOnlyCollection<string>(new List<string>
        {
            swiftbroom,
            headman_manor,
            mcjannek,
            museum
        });

        public static readonly ReadOnlyCollection<string> all_valuables = new ReadOnlyCollection<string>(new List<string>
        {
            diamond,
            ring,
            goblet,
            ocarina,
            pocket_watch,
            uranium_mug,
            crown,
            doll,
            frog_toy,
            gem_box,
            globe,
            money,
            monkey,
            uranium_plate,
            small_vase,
            champagne,
            clown_doll,
            trophy,
            vase,
            large_vase,
            animal_crate,
            bonsai,

            music_box,
            gramophone,
            rhino,
            scream_doll,
            grand_piano,
            harp,
            painting,
            grandfather_clock,
            dinosaur_skeleton,
            golden_statue,
            tv,
            radio,
            ship_in_a_bottle,

            desktop_computer,
            fan,
            explosive_barrel,
            sample,
            big_sample,
            flamethrower,
            science_station,
            server,
            printer,
            hdd,
            ice_saw,
            laptop,
            propane,
            sample_pack,
            guitar,
            sample_cooler,
            leg_ice,
            skeleton_ice,

            chomp_book,
            love_potion,
            cube_of_knowledge,
            staff,
            large_sword,
            broom,
            hourglass,
            master_potion,
            goblin_head,
            griffin,
            power_crystal,
            
            egg,
            car,
            banana_bow,
            boombox,
            cool_brain,
            cubic_tower,
            fish,
            silverfish,
            goldfish,
            golden_swirl,
            pacifier,
            baby_head,
            monkeybox,
            flesh_blob,
            tooth,
            goldtooth,
            toast,
            horse,
            blender,
            mug_deluxe,
            gumball,
            traffic_light,
            milk,
            handface,
            gem_burger,
            wire_figure,
            rubendoll
        });

        // ---- Monster Souls ----
        public const string animal_soul = "Animal Soul";
        public const string duck_soul = "Apex Predator Soul";
        public const string bowtie_soul = "Bowtie Soul";
        public const string chef_soul = "Chef Soul";
        public const string clown_soul = "Clown Soul";
        public const string headman_soul = "Headman Soul";
        public const string hidden_soul = "Hidden Soul";
        public const string huntsman_soul = "Huntsman Soul";
        public const string mentalist_soul = "Mentalist Soul";
        public const string reaper_soul = "Reaper Soul";
        public const string robe_soul = "Robe Soul";
        public const string rugrat_soul = "Rugrat Soul";
        public const string shadow_child_soul = "Shadow Child Soul";
        public const string spewer_soul = "Spewer Soul";
        public const string trudge_soul = "Trudge Soul";
        public const string upscream_soul = "Upscream Soul";

        public static readonly ReadOnlyCollection<string> all_monster_souls = new ReadOnlyCollection<string>(new List<string>
        {
            animal_soul,
            duck_soul,
            bowtie_soul,
            chef_soul,
            clown_soul,
            headman_soul,
            hidden_soul,
            huntsman_soul,
            mentalist_soul,
            reaper_soul,
            robe_soul,
            rugrat_soul,
            shadow_child_soul,
            spewer_soul,
            trudge_soul,
            upscream_soul
        });
    }
}
