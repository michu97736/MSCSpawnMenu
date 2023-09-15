using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;
using UnityEngine.UI;
using Resources = MSCSpawnMenu.Properties.Resources;

namespace MSCSpawnMenu
{
    public class MscSpawnMenu : Mod
    {
        public enum Categories
        {
            All,
            Furniture,
            Ragdolls,
            Other
        }

        public static Dictionary<Dictionary<string, GameObject>, Categories> List;
        public string[] Blacklist = { "Use", "Chop" };
        public GameObject Grid;
        public RectTransform GridTransform;
        public GameObject Item;
        public int ItemsSpawned;
        public static Dictionary<string, GameObject> MscCharacters;

        public static Dictionary<string, GameObject> MscFurniture;

        //public string path = Path.GetFullPath("CustomSpawnMenuItems");
        public static Dictionary<string, GameObject> MscItems;

        public Keybind Open = new Keybind("openmenu", "Open the spawn menu", KeyCode.Y);
        public FsmBool PlayerInMenu;
        public int RagdollsSpawned;
        public GameObject Tabs;
        public GameObject Ui;
        public override string ID => "MSCSpawnMenu";
        public override string Name => "Spawn Menu";
        public override string Author => "michu97736";
        public override string Version => "1.0";
        public override string Description => "";

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.ModSettings, Mod_Settings);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.OnMenuLoad, Mod_OnMenuLoad);
        }

        private void Mod_Settings() => Keybind.Add(this, Open);

        private void Mod_OnMenuLoad()
        {
            if (ModLoader.IsModPresent("AchievementCore"))
            {
                Achievement.CreateAchievement("MSCSpawnMenu_FirstSpawn", ID, "Achievement Get!",
                    "You spawned your first item!", null, false);
            }

            Achievement.CreateAchievement("MSCSpawnMenu_150Ragdolls", ID, "Achievement Get!",
                "You spawned 150 ragdolls!", null, false);
            Achievement.CreateAchievement("MSCSpawnMenu_500Ragdolls", ID, "Achievement Get!",
                "You (somehow) spawned 500 ragdolls!", null, false);
            Achievement.CreateAchievement("MSCSpawnMenu_150Items", ID, "Achievement Get!", "You spawned 150 items!",
                null, false);
            Achievement.CreateAchievement("MSCSpawnMenu_500Items", ID, "Achievement Get!",
                "You (somehow)spawned 500 items!", null, false);
;        }

        private void AlignGrid()
        {
            //gridTransform.position = new Vector3(gridTransform.position.x, gridTransform.rect.height / -2f, 0.0f);
        }

        private void Mod_OnLoad()
        {
            var bundle = AssetBundle.CreateFromMemoryImmediate(Resources.spawnmenu);
            Ui = Object.Instantiate(bundle.LoadAsset<GameObject>("SpawnMenuUI.prefab"));
            Item = Object.Instantiate(bundle.LoadAsset<GameObject>("Item.prefab"));
            Item.AddComponent<Item>();
            List = new Dictionary<Dictionary<string, GameObject>, Categories>();
            MscFurniture = new Dictionary<string, GameObject>();
            MscCharacters = new Dictionary<string, GameObject>();
            MscItems = new Dictionary<string, GameObject>();
            ItemInit();
            Add(MscFurniture, Categories.Furniture);
            Add(MscCharacters, Categories.Ragdolls);
            Add(MscItems, Categories.Other);
            PlayerInMenu = FsmVariables.GlobalVariables.GetFsmBool("PlayerInMenu");
            var transform = ModUI.CreateCanvas("SpawnMenu UI").transform;
            transform.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
            Ui.transform.SetParent(transform, false);
            Grid = Ui.transform.Find("Content/Grid").gameObject;
            Tabs = Ui.transform.Find("Header/Categories").gameObject;
            for (var i = 0; i < Tabs.transform.childCount; i++)
            {
                var transform1 = Tabs.transform.GetChild(i);
                transform1.GetComponent<Toggle>().onValueChanged.AddListener(delegate
                {
                    ChangeCategory((Categories)int.Parse(transform1.name));
                });
            }
            Ui.SetActive(false);
            ItemsSpawned = SaveLoad.ReadValue<int>(this, "ItemsSpawned");
            RagdollsSpawned = SaveLoad.ReadValue<int>(this, "RagdollsSpawned");
            bundle.Unload(false);
        }

        public void EditRagdoll(GameObject thisRagdoll, SkinnedMeshRenderer yourRagdollBodyMesh, bool changesBodymesh, bool usesGlasses, GameObject glassesPath)
        {
            thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture =
                yourRagdollBodyMesh.sharedMaterials[0].mainTexture;
            thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().materials[1].mainTexture =
                yourRagdollBodyMesh.sharedMaterials[1].mainTexture;
            thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().materials[2].mainTexture =
                yourRagdollBodyMesh.sharedMaterials[2].mainTexture;

            if (changesBodymesh)
            {
                thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().sharedMesh =
                    yourRagdollBodyMesh.sharedMesh;
            }
            if (usesGlasses == true && glassesPath != null)
            {
                var glasses = Object.Instantiate(glassesPath);
                glasses.transform.SetParent(thisRagdoll.transform.Find("pelvis/spine_mid/shoulders(xxxxx)/head"));
                glasses.transform.localPosition = glassesPath.transform.localPosition;
                glasses.transform.localEulerAngles = glassesPath.transform.localEulerAngles;
                glasses.name = "eye_glasses_regular";
            }
            thisRagdoll.name = "RagDoll";


            thisRagdoll.SetActive(false);
        }

        private void SpawnItem(KeyValuePair<string, GameObject> pair)
        {
            var isRagdoll = false;
            var item = Object.Instantiate(pair.Value);
            item.name = pair.Key + "(itemx)";
            item.SetActive(true);
            item.tag = "PART";
            item.layer = 19;
            if (item.GetComponent<PlayMakerFSM>() != null && item.GetComponent<PlayMakerFSM>().name == "Save")
            {
                Object.Destroy(item.GetComponent<PlayMakerFSM>());
            }

            if (item.GetComponent<Joint>() != null)
            {
                Object.Destroy(item.GetComponent<Joint>());
            }

            item.GetComponent<Rigidbody>().isKinematic = false;
            item.transform.localPosition = GameObject.Find("PLAYER").transform.localPosition;
            if (!ModLoader.IsModPresent("AchievementCore"))
            {
                return;
            }

            if (item.transform.Find("bodymesh") != null)
            {
                RagdollsSpawned++;
                switch (RagdollsSpawned)
                {
                    case 150:
                        Achievement.TriggerAchievement("MSCSpawnMenu_150Ragdolls");
                        break;
                    case 500:
                        Achievement.TriggerAchievement("MSCSpawnMenu_500Ragdolls");
                        break;
                }
            }
            else
            {
                ItemsSpawned++;
                switch (ItemsSpawned)
                {
                    case 150:
                        Achievement.TriggerAchievement("MSCSpawnMenu_150Items");
                        break;
                    case 500:
                        Achievement.TriggerAchievement("MSCSpawnMenu_500Items");
                        break;
                }
            }

            if (!Achievement.IsAchievementUnlocked("MSCSpawnMenu_FirstSpawn"))
            {
                Achievement.TriggerAchievement("MSCSpawnMenu_FirstSpawn");
            }
        }

        public void ChangeCategory(Categories cat)
        {
            ClearItems();

            switch (cat)
            {
                case Categories.All:
                    foreach (var item in List)
                    {
                        AddToView(item.Key);
                    }

                    break;
                default:
                    foreach (var item in List.Where(item => item.Value == cat))
                    {
                        AddToView(item.Key);
                    }

                    break;
            }
            //AlignGrid();
        }
        public void ClearItems()
        {
            foreach (Transform child in Grid.transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

        public void OpenMenu()
        {
            if (Ui.activeSelf != true)
            {
                Ui.SetActive(true);
                PlayerInMenu.Value = true;
            }
            else
            {
                Ui.SetActive(false);
                PlayerInMenu.Value = false;
            }
        }

        private void AddToView(Dictionary<string, GameObject> dict)
        {
            foreach (var item in dict)
            {
                var gameobjectitem = Object.Instantiate(Item);
                gameobjectitem.GetComponent<Item>().Text = item.Key.ToUpper();
                gameobjectitem.transform.Find("Button").GetComponent<Button>().onClick
                    .AddListener(() => SpawnItem(item));
                gameobjectitem.transform.SetParent(Grid.transform);
                gameobjectitem.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public static void Add(Dictionary<string, GameObject> dict, Categories cat) => List.Add(dict, cat);

        public static void Add(string itemName, GameObject gameObject, Categories cat)
        {
            switch (cat)
            {
                case Categories.Furniture: 
                    MscFurniture.Add(itemName, gameObject); 
                    break;
                case Categories.Other:
                    MscItems.Add(itemName,gameObject);
                    break;
                case Categories.Ragdolls:
                    MscCharacters.Add(itemName, gameObject);
                    break;
            }
        }

        public void ItemInit()
        {
            var stuff = GameObject.Find("JOBS/HouseDrunk/").transform.Find("Moving/Stuff").gameObject;
            var otherstuff = GameObject.Find("JOBS/HouseDrunk").transform.Find("Moving/Stuff/DestroyThese").gameObject;
            MscFurniture.Add("TV table", stuff.transform.Find("table(Clo02)").gameObject);
            MscFurniture.Add("kitchen table", stuff.transform.Find("table(Clo03)").gameObject);
            MscFurniture.Add("bedside table", stuff.transform.Find("table(Clo04)").gameObject);
            MscFurniture.Add("coffee table", stuff.transform.Find("table(Clo05)").gameObject);
            MscFurniture.Add("chair", stuff.transform.Find("chair(Clo02)").gameObject);
            MscFurniture.Add("bench", stuff.transform.Find("bench(Clo01)").gameObject);
            MscFurniture.Add("arm chair", stuff.transform.Find("arm chair(Clo01)").gameObject);
            MscFurniture.Add("tv", stuff.transform.Find("tv(Clo01)").gameObject);
            MscFurniture.Add("desk", stuff.transform.Find("desk(Clo01)").gameObject);
            MscFurniture.Add("mattress", otherstuff.transform.Find("mattress(Clo02)").gameObject);
            MscFurniture.Add("box", otherstuff.transform.Find("box(Clo02)").gameObject);
            var humans = GameObject.Find("HUMANS");
            var jokke = GameObject.Find("JOBS/HouseDrunk").transform.Find("Moving").gameObject;
            var npccars = GameObject.Find("NPC_CARS");
            var kylajani = npccars.GetPlayMaker("Amis Setup").FsmVariables.GetFsmGameObject("Amikset").Value.transform
                .Find("CrashEvent").gameObject;
            var petteri = npccars.GetPlayMaker("Amis2 Setup").FsmVariables.GetFsmGameObject("Amikset").Value.transform
                .Find("CrashEvent").gameObject;
            var latanen = npccars.GetPlayMaker("Bus Setup").FsmVariables.GetFsmGameObject("Bus").Value.transform
                .Find("Latanen/Pivot").gameObject;
            var pena = GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN/CrashEvent").gameObject;
            var caravan = GameObject.Find("TRAFFIC").transform.Find("VehiclesHighway/POLSA/CARAVAN").gameObject;
            MscCharacters.Add("Kale", humans.transform.Find("Kale/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Julli", humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Kristian", humans.transform.Find("Kristian/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Unto", humans.transform.Find("Unto/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Rauno", humans.transform.Find("Rauno/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Alpo", humans.transform.Find("Alpo/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Tohvakka", humans.transform.Find("Farmer/Walker/RagDoll").gameObject);
            MscCharacters.Add("Jokke", jokke.transform.Find("Hitcher/Pivot/RagDoll").gameObject);
            MscCharacters.Add("Suski", kylajani.transform.GetChild(1).gameObject);
            MscCharacters.Add("Jani", kylajani.transform.GetChild(2).gameObject);
            MscCharacters.Add("Petteri", petteri.transform.GetChild(1).gameObject);
            MscCharacters.Add("Teimo",
                GameObject.Find("STORE/TeimoInBike/Bicycle").transform.Find("Functions/Teimo/RagDoll").gameObject);
            MscCharacters.Add("Latanen", latanen.transform.GetChild(3).gameObject);
            MscCharacters.Add("Grandma",
                GameObject.Find("ChurchGrandma/GrannyHiker").transform.Find("RagDoll2").gameObject);
            MscCharacters.Add("Pena", pena.transform.Find("RagDoll").gameObject);
            GameObject sausages = null;
            GameObject moose = null;
            var arrayItems = UnityEngine.Resources.FindObjectsOfTypeAll<Transform>();
            foreach (var gameobject in arrayItems)
            {
                if (gameobject.name == "sausages")
                {
                    sausages = gameobject.gameObject;
                }

                if (ModLoader.IsModPresent("MooseHunter"))
                {
                    if (gameobject.name == "dead moose(xxxxx)(Clone)")
                    {
                        moose = gameobject.gameObject;
                    }
                }
                else
                {
                    if (gameobject.name == "dead moose(xxxxx)")
                    {
                        moose = gameobject.gameObject;
                    }
                }
            }

            MscCharacters.Add("Moose", moose);
            MscItems.Add("Sausages", sausages);
            MscItems.Add("Caravan", caravan);
            MscItems.Add("Garbage barrel", GameObject.Find("ITEMS/garbage barrel(itemx)"));
            var bicycle = Object.Instantiate(GameObject.Find("STORE/TeimoInBike/Bicycle").transform
                .Find("Functions/bicycle").gameObject);
            var box = bicycle.AddComponent<BoxCollider>();
            box.size = bicycle.transform.Find("coll").GetComponents<BoxCollider>()[0].size;
            box.center = bicycle.transform.Find("coll").GetComponents<BoxCollider>()[0].center;
            bicycle.SetActive(false);
            MscItems.Add("Bicycle", bicycle);
            var flashlight = Object.Instantiate(GameObject.Find("flashlight(itemx)"));
            flashlight.name = "flashlight(spawnmenu)";
            flashlight.transform.Find("ChangeBatteries/Insert").GetPlayMaker("Assembly").FsmVariables
                .GetFsmInt("Batteries").Value = 4;
            Object.Destroy(flashlight.transform.Find("FlashLight").GetPlayMaker("Charge"));
            Object.Destroy(flashlight.transform.Find("Open"));
            flashlight.transform.Find("FlashLight").GetComponent<Light>().intensity = 8;
            flashlight.SetActive(false);
            MscItems.Add("Flashlight", flashlight);
            MscItems.Add("Ax", GameObject.Find("ITEMS/ax(itemx)"));
            MscItems.Add("Hammer", GameObject.Find("ITEMS/sledgehammer(itemx)"));
            //Fleetari
            var fleetariragdoll = Object.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(fleetariragdoll, GameObject.Find("REPAIRSHOP").transform.Find("LOD/Office/Fleetari/Neighbour 2/bodymesh").GetComponent<SkinnedMeshRenderer>(), true, true, GameObject.Find("REPAIRSHOP").transform.Find(
                        "LOD/Office/Fleetari/Neighbour 2/skeleton/pelvis/spine_middle/spine_upper/HeadPivot/head/Shades/eye_glasses_regular")
                    .gameObject);
            ModConsole.Print("Fleetari loaded");
            //Servant
            GameObject waterFacility;
            GameObject servantRagdoll = null;
            foreach (var transform in arrayItems)
            {
                if (transform.gameObject.name == "WATERFACILITY")
                {
                    waterFacility = transform.gameObject;

                    servantRagdoll = Object.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
                    EditRagdoll(servantRagdoll,
                        waterFacility.transform.Find("LOD/Functions/Servant/Pivot/Shitman/bodymesh")
                            .GetComponent<SkinnedMeshRenderer>(), true, true,
                        waterFacility.transform
                            .Find(
                                "LOD/Functions/Servant/Pivot/Shitman/skeleton/pelvis/spine_middle/spine_upper/HeadPivot/head/eye_glasses_regular")
                            .gameObject);
                }
            }

            ModConsole.Print("Servant loaded");
            //Strawberrry guy
            var berrymanragdoll = Object.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(berrymanragdoll,
                GameObject.Find("JOBS").transform.Find("StrawberryField/LOD/Functions/Berryman/Pivot/Berryman/bodymesh")
                    .GetComponent<SkinnedMeshRenderer>(), true, true,
                GameObject.Find("JOBS").transform
                    .Find(
                        "StrawberryField/LOD/Functions/Berryman/Pivot/Berryman/skeleton/pelvis/spine_middle/spine_upper/HeadPivot/head/Accessories 1")
                    .gameObject);
            ModConsole.Print("Strawberry guy loaded");
            //Lindell
            var lindellRagdoll = Object.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(lindellRagdoll,
                GameObject.Find("INSPECTION").transform.Find("LOD/Officer/Work/Char/bodymesh")
                    .GetComponent<SkinnedMeshRenderer>(), true, false, null);
            //Kuski
            var kuskiRagdoll =
                Object.Instantiate(GameObject.Find("NPC_CARS").transform.Find("KUSKI/CrashEvent/RagDoll").gameObject);
            ModConsole.Print("kuski loaded");
            //Kuski's brother
            var kuskiBrother = Object.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(kuskiBrother,
                GameObject.Find("NPC_CARS").transform.Find("KUSKI/Passenger/bodymesh")
                    .GetComponent<SkinnedMeshRenderer>(), true, true,
                GameObject.Find("NPC_CARS").transform.Find("KUSKI/Passenger/pelvis/spine_mid/shoulder/head/Cap 1")
                    .gameObject);
            ModConsole.Print("kuski's brother loaded");
            MscCharacters.Add("Kuski", kuskiRagdoll);
            MscCharacters.Add("Fleetari", fleetariragdoll);
            MscCharacters.Add("Kuski's brother", kuskiBrother);
            MscCharacters.Add("Strawberry guy", berrymanragdoll);
            MscCharacters.Add("Lindell", lindellRagdoll);
            MscCharacters.Add("Servant", servantRagdoll);

            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            //string[] dirs = Directory.GetDirectories(path);
            //for (int i = 0; i < dirs.Length; i++)
            //{
            //    string[] models = Directory.GetFiles(dirs[i], "*.*").Where(file => file.ToLower().EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase)).ToArray();
            //    string[] textures = Directory.GetFiles(dirs[i], "*.*").Where(file => file.ToLower().EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)).ToArray();
            //}
        }

        private void Mod_Update()
        {
            if (Open.GetKeybindDown())
            {
                OpenMenu();
            }
        }

        private void Mod_OnSave()
        {
            SaveLoad.WriteValue(this, "ItemsSpawned", ItemsSpawned);
            SaveLoad.WriteValue(this, "RagdollsSpawned", RagdollsSpawned);
        }
    }
}