using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.IO;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MscSpawnMenu
{
    public class MSCSpawnMenu : Mod
    {
        public override string ID => "MSCSpawnMenu";
        public override string Name => "Spawn Menu";
        public override string Author => "michu97736";
        public override string Version => "1.0";
        public override string Description => "";

        public Keybind open = new Keybind("openmenu", "Open the spawn menu", KeyCode.Y);
        public GameObject UI;
        public GameObject Item;
        public GameObject grid;
        public RectTransform gridTransform;
        public GameObject Tabs;
        public FsmBool PlayerInMenu;
        public string[] blacklist = { "Use", "Chop"};
        //public string path = Path.GetFullPath("CustomSpawnMenuItems");
        public Dictionary<string, GameObject> MSCItems;
        public Dictionary<string, GameObject> MSCCharacters;
        public Dictionary<string, GameObject> MSCFurniture;
        public static Dictionary<Dictionary<string,GameObject>,Categories> List;
        public int ItemsSpawned = 0;
        public int RagdollsSpawned = 0;
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.ModSettings, Mod_Settings);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.OnMenuLoad, Mod_OnMenuLoad);
        }

        private void Mod_Settings()
        {
            Keybind.Add(this, open);
        }

        private void Mod_OnMenuLoad()
        {
            if(ModLoader.IsModPresent("AchievementCore"))
            Achievement.CreateAchievement("MSCSpawnMenu_FirstSpawn",ID, "Achievement Get!","You spawned your first item!",null,false);
            Achievement.CreateAchievement("MSCSpawnMenu_150Ragdolls", ID, "Achievement Get!", "You spawned 150 ragdolls!", null, false);
            Achievement.CreateAchievement("MSCSpawnMenu_500Ragdolls", ID, "Achievement Get!", "You (somehow) spawned 500 ragdolls!", null, false);
            Achievement.CreateAchievement("MSCSpawnMenu_150Items", ID, "Achievement Get!", "You spawned 150 items!", null, false);
            Achievement.CreateAchievement("MSCSpawnMenu_500Items", ID, "Achievement Get!", "You (somehow)spawned 500 items!", null, false);
        }

        private void AlignGrid()
        {
            //gridTransform.position = new Vector3(gridTransform.position.x, gridTransform.rect.height / -2f, 0.0f);
        }

        private void Mod_OnLoad()
        {
            AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate(Properties.Resources.spawnmenu);
            UI = GameObject.Instantiate(bundle.LoadAsset<GameObject>("SpawnMenuUI.prefab"));
            Item = GameObject.Instantiate(bundle.LoadAsset<GameObject>("Item.prefab"));
            Item.AddComponent<Item>();
            List = new Dictionary<Dictionary<string, GameObject>, Categories>();
            MSCFurniture = new Dictionary<string, GameObject>();
            MSCCharacters = new Dictionary<string, GameObject>();
            MSCItems = new Dictionary<string, GameObject>();
            ItemInit();
            Add(MSCFurniture, Categories.Furniture);
            Add(MSCCharacters, Categories.Ragdolls);
            Add(MSCItems, Categories.Other);
            PlayerInMenu = FsmVariables.GlobalVariables.GetFsmBool("PlayerInMenu");
            Transform transform = ModUI.CreateCanvas("SpawnMenu UI").transform;
            transform.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
            UI.transform.SetParent(transform, false);
            grid = UI.transform.Find("Content/Grid").gameObject;
            Tabs = UI.transform.Find("Header/Categories").gameObject;
            for(int i = 0; i < Tabs.transform.childCount; i++)
            {
                Transform transform1 = Tabs.transform.GetChild(i);
                transform1.GetComponent<Toggle>().onValueChanged.AddListener(delegate { ChangeCategory((Categories)Int32.Parse(transform1.name)); });
            }
            UI.SetActive(false);
            ItemsSpawned = SaveLoad.ReadValue<int>(this, "ItemsSpawned");
            RagdollsSpawned = SaveLoad.ReadValue<int>(this, "RagdollsSpawned");
            bundle.Unload(false);
        }

        public void EditRagdoll(GameObject thisRagdoll, GameObject glassesPath, SkinnedMeshRenderer yourRagdollBodyMesh, bool UsesGlasses, bool changesBodymesh)
        {

            thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture = yourRagdollBodyMesh.sharedMaterials[0].mainTexture;
            thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().materials[1].mainTexture = yourRagdollBodyMesh.sharedMaterials[1].mainTexture;
            thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().materials[2].mainTexture = yourRagdollBodyMesh.sharedMaterials[2].mainTexture;

            if (UsesGlasses)
            {
                GameObject glasses = GameObject.Instantiate(glassesPath).gameObject;
                glasses.transform.SetParent(thisRagdoll.transform.Find("pelvis/spine_mid/shoulders(xxxxx)/head"));
                glasses.transform.localPosition = glassesPath.transform.localPosition;
                glasses.transform.localEulerAngles = glassesPath.transform.localEulerAngles;
                glasses.name = "eye_glasses_regular";
            }

            if (changesBodymesh) { thisRagdoll.transform.Find("bodymesh").GetComponent<SkinnedMeshRenderer>().sharedMesh = yourRagdollBodyMesh.sharedMesh; }

            thisRagdoll.name = "RagDoll";


            thisRagdoll.SetActive(false);
        }
        private void SpawnItem(KeyValuePair<string, GameObject> pair)
        {
            bool isRagdoll = false;
            var item = GameObject.Instantiate(pair.Value);
            item.name = pair.Key + "(itemx)";
            item.SetActive(true);
            item.tag = "PART";
            item.layer = 19;
            if (item.GetComponent<PlayMakerFSM>() != null && item.GetComponent<PlayMakerFSM>().name == "Save")
            {
                UnityEngine.Object.Destroy(item.GetComponent<PlayMakerFSM>());
            }
            if (item.GetComponent<Joint>() != null)
            {
                UnityEngine.Object.Destroy(item.GetComponent<Joint>());
            }
            item.GetComponent<Rigidbody>().isKinematic = false;
            item.transform.localPosition = GameObject.Find("PLAYER").transform.localPosition;
            if (!ModLoader.IsModPresent("AchievementCore"))
                return;
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
                        AddToView(item.Key);
                    break;
                default:
                    foreach (var item in List)
                    {
                        if (item.Value == cat)
                        {
                            AddToView(item.Key);
                        }
                    }
                    break;
            }
            //AlignGrid();

        }
        public void ClearItems()
        {
            foreach (Transform child in grid.transform)
                UnityEngine.Object.Destroy(child.gameObject);
        }
        
        public void OpenMenu()
        {
            if(UI.activeSelf != true)
            {
                UI.SetActive(true);
                PlayerInMenu.Value = true;
            }
            else
            {
                UI.SetActive(false);
                PlayerInMenu.Value = false;
            }
        }

        private void AddToView(Dictionary<String, GameObject> dict)
        {
            foreach (var item in dict)
            {
                var gameobjectitem = GameObject.Instantiate(Item);
                gameobjectitem.GetComponent<Item>().text = item.Key.ToUpper();
                gameobjectitem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => SpawnItem(item));
                gameobjectitem.transform.SetParent(grid.transform);
                gameobjectitem.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public static void Add(Dictionary<string, GameObject> dict, Categories cat)
        {
            List.Add(dict,cat);
        }
        public void ItemInit()
        {
            GameObject stuff = GameObject.Find("JOBS/HouseDrunk/").transform.Find("Moving/Stuff").gameObject;
            GameObject otherstuff = GameObject.Find("JOBS/HouseDrunk").transform.Find("Moving/Stuff/DestroyThese").gameObject;
            MSCFurniture.Add("TV table", stuff.transform.Find("table(Clo02)").gameObject);
            MSCFurniture.Add("kitchen table", stuff.transform.Find("table(Clo03)").gameObject);
            MSCFurniture.Add("bedside table", stuff.transform.Find("table(Clo04)").gameObject);
            MSCFurniture.Add("coffee table", stuff.transform.Find("table(Clo05)").gameObject);
            MSCFurniture.Add("chair", stuff.transform.Find("chair(Clo02)").gameObject);
            MSCFurniture.Add("bench", stuff.transform.Find("bench(Clo01)").gameObject);
            MSCFurniture.Add("arm chair", stuff.transform.Find("arm chair(Clo01)").gameObject);
            MSCFurniture.Add("tv", stuff.transform.Find("tv(Clo01)").gameObject);
            MSCFurniture.Add("desk", stuff.transform.Find("desk(Clo01)").gameObject);
            MSCFurniture.Add("mattress", otherstuff.transform.Find("mattress(Clo02)").gameObject);
            MSCFurniture.Add("box", otherstuff.transform.Find("box(Clo02)").gameObject);
            GameObject humans = GameObject.Find("HUMANS");
            GameObject jokke = GameObject.Find("JOBS/HouseDrunk").transform.Find("Moving").gameObject;
            GameObject npccars = GameObject.Find("NPC_CARS");
            GameObject Kylajani = npccars.GetPlayMaker("Amis Setup").FsmVariables.GetFsmGameObject("Amikset").Value.transform.Find("CrashEvent").gameObject;
            GameObject petteri = npccars.GetPlayMaker("Amis2 Setup").FsmVariables.GetFsmGameObject("Amikset").Value.transform.Find("CrashEvent").gameObject;
            GameObject latanen = npccars.GetPlayMaker("Bus Setup").FsmVariables.GetFsmGameObject("Bus").Value.transform.Find("Latanen/Pivot").gameObject;
            GameObject pena = GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN/CrashEvent").gameObject;
            GameObject caravan = GameObject.Find("TRAFFIC").transform.Find("VehiclesHighway/POLSA/CARAVAN").gameObject;
            MSCCharacters.Add("Kale", humans.transform.Find("Kale/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Julli", humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Kristian", humans.transform.Find("Kristian/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Unto", humans.transform.Find("Unto/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Rauno", humans.transform.Find("Rauno/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Alpo", humans.transform.Find("Alpo/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Tohvakka", humans.transform.Find("Farmer/Walker/RagDoll").gameObject);
            MSCCharacters.Add("Jokke", jokke.transform.Find("Hitcher/Pivot/RagDoll").gameObject);
            MSCCharacters.Add("Suski", Kylajani.transform.GetChild(1).gameObject);
            MSCCharacters.Add("Jani", Kylajani.transform.GetChild(2).gameObject);
            MSCCharacters.Add("Petteri", petteri.transform.GetChild(1).gameObject);
            MSCCharacters.Add("Teimo", GameObject.Find("STORE/TeimoInBike/Bicycle").transform.Find("Functions/Teimo/RagDoll").gameObject);
            MSCCharacters.Add("Latanen", latanen.transform.GetChild(3).gameObject);
            MSCCharacters.Add("Grandma",GameObject.Find("ChurchGrandma/GrannyHiker").transform.Find("RagDoll2").gameObject);
            MSCCharacters.Add("Pena",pena.transform.Find("RagDoll").gameObject);
            GameObject sausages = null;
            GameObject moose = null;
            Transform[] arrayItems = UnityEngine.Resources.FindObjectsOfTypeAll<Transform>();
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
            MSCCharacters.Add("Moose", moose);
            MSCItems.Add("Sausages", sausages);
            MSCItems.Add("Caravan", caravan);
            MSCItems.Add("Garbage barrel",GameObject.Find("ITEMS/garbage barrel(itemx)"));
            GameObject bicycle = GameObject.Instantiate(GameObject.Find("STORE/TeimoInBike/Bicycle").transform.Find("Functions/bicycle").gameObject);
            BoxCollider box = bicycle.AddComponent<BoxCollider>();
            box.size = bicycle.transform.Find("coll").GetComponents<BoxCollider>()[0].size;
            box.center = bicycle.transform.Find("coll").GetComponents<BoxCollider>()[0].center;
            bicycle.SetActive(false);
            MSCItems.Add("Bicycle",bicycle);
            GameObject flashlight = GameObject.Instantiate(GameObject.Find("flashlight(itemx)"));
            flashlight.name = "flashlight(spawnmenu)";
            flashlight.transform.Find("ChangeBatteries/Insert").GetPlayMaker("Assembly").FsmVariables.GetFsmInt("Batteries").Value = 4;
            UnityEngine.Object.Destroy(flashlight.transform.Find("FlashLight").GetPlayMaker("Charge"));
            UnityEngine.Object.Destroy(flashlight.transform.Find("Open"));
            flashlight.transform.Find("FlashLight").GetComponent<Light>().intensity = 8;
            flashlight.SetActive(false);
            MSCItems.Add("Flashlight",flashlight);
            MSCItems.Add("Ax",GameObject.Find("ITEMS/ax(itemx)"));
            MSCItems.Add("Hammer", GameObject.Find("ITEMS/sledgehammer(itemx)"));
            //Fleetari
            GameObject fleetariragdoll = GameObject.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(fleetariragdoll, GameObject.Find("REPAIRSHOP").transform.Find("LOD/Office/Fleetari/Neighbour 2/skeleton/pelvis/spine_middle/spine_upper/HeadPivot/head/Shades/eye_glasses_regular").gameObject, GameObject.Find("REPAIRSHOP").transform.Find("LOD/Office/Fleetari/Neighbour 2/bodymesh").GetComponent<SkinnedMeshRenderer>(), true, false);
            ModConsole.Print("Fleetari loaded");
            //Servant
            GameObject waterFacility;
            GameObject servantRagdoll = null;
            foreach (Transform transform in arrayItems)
            {
                if (transform.gameObject.name == "WATERFACILITY")
                {
                    waterFacility = transform.gameObject;

                    servantRagdoll = GameObject.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
                    EditRagdoll(servantRagdoll, waterFacility.transform.Find("LOD/Functions/Servant/Pivot/Shitman/skeleton/pelvis/spine_middle/spine_upper/HeadPivot/head/eye_glasses_regular").gameObject, waterFacility.transform.Find("LOD/Functions/Servant/Pivot/Shitman/bodymesh").GetComponent<SkinnedMeshRenderer>(), true, true);
                }
            }
            ModConsole.Print("Servant loaded");
            //Strawberrry guy
            GameObject berrymanragdoll = GameObject.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(berrymanragdoll, GameObject.Find("JOBS").transform.Find("StrawberryField/LOD/Functions/Berryman/Pivot/Berryman/skeleton/pelvis/spine_middle/spine_upper/HeadPivot/head/Accessories 1").gameObject, GameObject.Find("JOBS").transform.Find("StrawberryField/LOD/Functions/Berryman/Pivot/Berryman/bodymesh").GetComponent<SkinnedMeshRenderer>(), true, true);
            ModConsole.Print("Strawberry guy loaded");
            //Lindell
            GameObject lindellRagdoll = GameObject.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(lindellRagdoll,null,GameObject.Find("INSPECTION").transform.Find("LOD/Officer/Work/Char/bodymesh").GetComponent<SkinnedMeshRenderer>(), false,true);
            //Kuski
            GameObject kuskiRagdoll = GameObject.Instantiate(GameObject.Find("NPC_CARS").transform.Find("KUSKI/CrashEvent/RagDoll").gameObject);
            ModConsole.Print("kuski loaded");
            //Kuski's brother
            GameObject kuskiBrother = GameObject.Instantiate(humans.transform.Find("Julli/Pivot/RagDoll").gameObject);
            EditRagdoll(kuskiBrother, GameObject.Find("NPC_CARS").transform.Find("KUSKI/Passenger/pelvis/spine_mid/shoulder/head/Cap 1").gameObject, GameObject.Find("NPC_CARS").transform.Find("KUSKI/Passenger/bodymesh").GetComponent<SkinnedMeshRenderer>(), true, true);
            ModConsole.Print("kuski's brother loaded");
            MSCCharacters.Add("Kuski", kuskiRagdoll);
            MSCCharacters.Add("Fleetari", fleetariragdoll);
            MSCCharacters.Add("Kuski's brother", kuskiBrother);
            MSCCharacters.Add("Strawberry guy", berrymanragdoll);
            MSCCharacters.Add("Lindell", lindellRagdoll);
            MSCCharacters.Add("Servant", servantRagdoll);

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
            if (open.GetKeybindDown())
            {
                OpenMenu();
            }
        }

        private void Mod_OnSave()
        {
            SaveLoad.WriteValue(this, "ItemsSpawned",ItemsSpawned);
            SaveLoad.WriteValue(this, "RagdollsSpawned", RagdollsSpawned);
        }

        public enum Categories
        {
            All,
            Furniture,
            Ragdolls,
            Other
        }
    }
}
