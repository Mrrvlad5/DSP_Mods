using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DysonSphereProgram.Modding.StarterTemplate
{
    public static class Configs
    {
        public static bool AdjustTechUnlocks = true;
        public static bool AdjustTechCosts = true;
        public static double TechCostMultiple = 100;
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess("DSPGAME.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "epic.research.mrrvlad";
        public const string NAME = "EpicResearch";
        public const string VERSION = "0.1.1";

        private Harmony _harmony;
        internal static ManualLogSource Log;

        private string Print(int[] val)
        {
            string s = "(";
            for (int i = 0; i < val.Length; i++)
            {
                if (i > 0) s += ",";
                s += val[i].ToString();
            }
            s += ")";
            return s;
        }

        private string Print(double[] val)
        {
            string s = "(";
            for (int i = 0; i < val.Length; i++)
            {
                if (i > 0) s += ",";
                s += val[i].ToString();
            }
            s += ")";
            return s;
        }

        private void Awake()
        {
            Plugin.Log = Logger;
            _harmony = new Harmony(GUID);
            Logger.LogInfo("EpicResearch Awake() called");
            ConfigEntry<bool> config_unlock = Config.Bind<bool>("General", "AdjustTechUnlocks", true,
            "Modify tech tree unlocks for rare receips");
            ConfigEntry<bool> config_cost = Config.Bind<bool>("General", "AdjustTechCosts", true,
            "Modify type and quantity of materials needed to unlock a tech");
            ConfigEntry<int> config_multiple = Config.Bind<int>("General", "TechCostMultiple", 100,
            "Apply this multiple to tech costs. Valid vaules are 1, 5, 20, 50, 100.");
            Configs.AdjustTechUnlocks = config_unlock.Value;
            Configs.AdjustTechCosts = config_cost.Value;
            if (config_multiple.Value <= 1)
                Configs.TechCostMultiple = 1;
            else if (config_multiple.Value <= 5)
                Configs.TechCostMultiple = 5;
            else if (config_multiple.Value <= 20)
                Configs.TechCostMultiple = 20;
            else if (config_multiple.Value <= 50)
                Configs.TechCostMultiple = 50;
            else Configs.TechCostMultiple = 100;

            if (Configs.AdjustTechUnlocks)
                AdjustUnlocks();
            AdjustTechHashCosts(Configs.AdjustTechCosts, Configs.TechCostMultiple);

            //int tech_count = LDB.techs.dataArray.Length;
            //for (int i = 0; i < tech_count; i++)
            //{
            //    Logger.LogInfo(
            //        LDB.techs.dataArray[i].ID.ToString() +
            //        " HashNeeded: " + LDB.techs.dataArray[i].HashNeeded.ToString() +
            //        " ItemPoints: " + Print(LDB.techs.dataArray[i].ItemPoints) +
            //        " Items: " + Print(LDB.techs.dataArray[i].Items) +
            //        " Level: " + LDB.techs.dataArray[i].Level +
            //        " LevelCoef1: " + LDB.techs.dataArray[i].LevelCoef1 +
            //        " LevelCoef2: " + LDB.techs.dataArray[i].LevelCoef2 +
            //        " MaxLevel: " + LDB.techs.dataArray[i].MaxLevel +
            //        " UnlockRecipes: " + Print(LDB.techs.dataArray[i].UnlockRecipes) +
            //        " UnlockValues: " + Print(LDB.techs.dataArray[i].UnlockValues) +
            //        " UnlockFunctions: " + Print(LDB.techs.dataArray[i].UnlockFunctions)
            //        ); ;
            //}

        }

        private void AdjustUnlocks()
        {
            // Move logi warp to lvl6
            int lvl4_logi, lvl6_logi;
            bool result1 = LDB.techs.dataIndices.TryGetValue(3404, out lvl4_logi);
            bool result2 = LDB.techs.dataIndices.TryGetValue(3406, out lvl6_logi);

            if (result1 && result2)
            {
                // find position of warp
                int pos = -1;
                for (int i = 0; i < LDB.techs.dataArray[lvl4_logi].UnlockFunctions.Length; i++)
                {
                    if (LDB.techs.dataArray[lvl4_logi].UnlockFunctions[i] == 17) { pos = i; break; }
                }
                if (pos != -1)
                {
                    LDB.techs.dataArray[lvl6_logi].UnlockFunctions = LDB.techs.dataArray[lvl6_logi].UnlockFunctions.AddToArray<int>(17);
                    LDB.techs.dataArray[lvl6_logi].UnlockValues = LDB.techs.dataArray[lvl6_logi].UnlockValues.AddToArray<double>(1.0);

                    int[] old_f = LDB.techs.dataArray[lvl4_logi].UnlockFunctions;
                    double[] old_v = LDB.techs.dataArray[lvl4_logi].UnlockValues;
                    LDB.techs.dataArray[lvl4_logi].UnlockFunctions = System.Array.Empty<int>();
                    LDB.techs.dataArray[lvl4_logi].UnlockValues = System.Array.Empty<double>();
                    for (int i = 0; i < old_f.Length; i++)
                    {
                        if (i != pos)
                        {
                            LDB.techs.dataArray[lvl4_logi].UnlockFunctions = LDB.techs.dataArray[lvl4_logi].UnlockFunctions.AddToArray(old_f[i]);
                            LDB.techs.dataArray[lvl4_logi].UnlockValues = LDB.techs.dataArray[lvl4_logi].UnlockValues.AddToArray(old_v[i]);
                        }
                    }
                }
                else
                {
                    Logger.LogInfo("Failed to move logi warp from l4 to l6 - warp capability not found in l4");
                }
            }
            else
            {
                Logger.LogInfo("Failed find logi tech while moving logi warp from l4 to l6");
            }

            // Adjust sphere stress tech. Change % from 15 to 14. Move the missing 6% to Grav wave refraction
            {
                int grav_tech;
                int ds_tech;
                bool r1 = LDB.techs.dataIndices.TryGetValue(1704, out grav_tech);
                bool r2 = LDB.techs.dataIndices.TryGetValue(1523, out ds_tech);
                if (r1 && r2)
                {
                    int pos = -1;
                    for (int i = 0; i < LDB.techs.dataArray[ds_tech].UnlockFunctions.Length; i++)
                    {
                        if (LDB.techs.dataArray[ds_tech].UnlockFunctions[i] == 26) { pos = i; break; }
                    }
                    if (pos != -1)
                    {
                        LDB.techs.dataArray[grav_tech].UnlockFunctions = LDB.techs.dataArray[grav_tech].UnlockFunctions.AddToArray<int>(26);
                        LDB.techs.dataArray[grav_tech].UnlockValues = LDB.techs.dataArray[grav_tech].UnlockValues.AddToArray<double>(6.0);

                        int[] old_f = LDB.techs.dataArray[ds_tech].UnlockFunctions;
                        double[] old_v = LDB.techs.dataArray[ds_tech].UnlockValues;
                        LDB.techs.dataArray[ds_tech].UnlockFunctions = System.Array.Empty<int>();
                        LDB.techs.dataArray[ds_tech].UnlockValues = System.Array.Empty<double>();
                        for (int i = 0; i < old_f.Length; i++)
                        {
                            if (i != pos)
                            {
                                LDB.techs.dataArray[ds_tech].UnlockFunctions = LDB.techs.dataArray[ds_tech].UnlockFunctions.AddToArray(old_f[i]);
                                LDB.techs.dataArray[ds_tech].UnlockValues = LDB.techs.dataArray[ds_tech].UnlockValues.AddToArray(old_v[i]);
                            }
                        }
                        LDB.techs.dataArray[ds_tech].UnlockFunctions = LDB.techs.dataArray[ds_tech].UnlockFunctions.AddToArray<int>(26);
                        LDB.techs.dataArray[ds_tech].UnlockValues = LDB.techs.dataArray[ds_tech].UnlockValues.AddToArray<double>(14.0);
                    }
                    else
                    {
                        Logger.LogInfo("Failed to move dyson sphere stress capability from tech 1523 to 1704");
                    }
                }
                else
                {
                    Logger.LogInfo("Failed find tech while moving dyson sphere stress capability from 1523 to 1704");
                }

            }
            // Adjust vessel speed with unlocks.
            {
                for (int tech_id = 3403; tech_id <= 3405; tech_id++)
                {
                    int logi_tech;
                    bool r1 = LDB.techs.dataIndices.TryGetValue(tech_id, out logi_tech);
                    if (r1)
                    {
                        int pos = -1;
                        for (int i = 0; i < LDB.techs.dataArray[logi_tech].UnlockFunctions.Length; i++)
                        {
                            if (LDB.techs.dataArray[logi_tech].UnlockFunctions[i] == 16) { pos = i; break; }
                        }
                        if (pos != -1)
                        {
                            LDB.techs.dataArray[logi_tech].UnlockValues[pos]*=5;
                        }
                        else
                        {
                            Logger.LogInfo("Failed to find vessel speed function to adjust");
                        }
                    }
                    else
                    {
                        Logger.LogInfo("Failed find Logistics Carrier Engine tech while adjusting vessel speed");
                    }
                }

            }
            // Adjust vessel capacity with unlocks.
            {
                for (int tech_id = 3503; tech_id <= 3508; tech_id++)
                {
                    if (tech_id == 3505) continue;
                    if (tech_id == 3506) continue;
                    int logi_tech;
                    bool r1 = LDB.techs.dataIndices.TryGetValue(tech_id, out logi_tech);
                    if (r1)
                    {
                        int pos = -1;
                        for (int i = 0; i < LDB.techs.dataArray[logi_tech].UnlockFunctions.Length; i++)
                        {
                            if (LDB.techs.dataArray[logi_tech].UnlockFunctions[i] == 19) { pos = i; break; }
                        }
                        if (pos != -1)
                        {
                            double m = 2;
                            if (tech_id > 3506) m = 0.5;
                            LDB.techs.dataArray[logi_tech].UnlockValues[pos] *= m;
                        }
                        else
                        {
                            Logger.LogInfo("Failed to find vessel capacity function to adjust");
                        }
                    }
                    else
                    {
                        Logger.LogInfo("Failed find Logistics Carrier Capacity tech while adjusting vessel capacity");
                    }
                }

            }

            // Move advanced receipes
            Logger.LogInfo("Moving techs:" + unlocks_move_data.GetLength(0).ToString());
            for (int move_id = 0; move_id < unlocks_move_data.GetLength(0); move_id++)
            {
                int from_tech;
                int to_tech;
                bool r1 = LDB.techs.dataIndices.TryGetValue(unlocks_move_data[move_id, 0], out from_tech);
                bool r2 = LDB.techs.dataIndices.TryGetValue(unlocks_move_data[move_id, 1], out to_tech);
                if (r1 && r2)
                {
                    int pos = -1;
                    for (int i = 0; i < LDB.techs.dataArray[from_tech].UnlockRecipes.Length; i++)
                    {
                        if (LDB.techs.dataArray[from_tech].UnlockRecipes[i] == unlocks_move_data[move_id, 2]) { pos = i; break; }
                    }
                    if (pos != -1)
                    {
                        LDB.techs.dataArray[to_tech].UnlockRecipes = LDB.techs.dataArray[to_tech].UnlockRecipes.AddToArray<int>(unlocks_move_data[move_id, 2]);

                        int[] old_f = LDB.techs.dataArray[from_tech].UnlockRecipes;
                        LDB.techs.dataArray[from_tech].UnlockRecipes = System.Array.Empty<int>();
                        for (int i = 0; i < old_f.Length; i++)
                        {
                            if (i != pos)
                            {
                                LDB.techs.dataArray[from_tech].UnlockRecipes = LDB.techs.dataArray[from_tech].UnlockRecipes.AddToArray(old_f[i]);
                            }
                        }

                    }
                    else
                    {
                        Logger.LogInfo("Failed to move receipt " + unlocks_move_data[move_id, 2].ToString() +
                            " from tech " + unlocks_move_data[move_id, 0].ToString() +
                            " to " + unlocks_move_data[move_id, 1].ToString());
                    }
                }
                else
                {
                    Logger.LogInfo("Failed find tech while moving receipt from" + unlocks_move_data[move_id, 0] + " to:" + unlocks_move_data[move_id, 1]);
                }
            }
        }
        private void AdjustTechHashCosts(bool adjust_techs = true, double multiple = 100)
        {
            if (adjust_techs)
                InitializeDictionary();
            int tech_count = LDB.techs.dataArray.Length;
            for (int t = 0; t < tech_count; t++)
            {
                int[] data_patch_id;
                bool r1 = tech_data.TryGetValue(LDB.techs.dataArray[t].ID, out data_patch_id);
                if (r1)
                {
                    double tech_multiple = 0.01*multiple* data_patch_id[1];
                    if (tech_multiple < 1) tech_multiple = 1;
                    LDB.techs.dataArray[t].HashNeeded = (long)((double)data_patch_id[0]*tech_multiple);
                    LDB.techs.dataArray[t].ItemPoints = System.Array.Empty<int>();
                    LDB.techs.dataArray[t].Items = System.Array.Empty<int>();
                    for(int i = 0; i < 6; i++)
                    {
                        if (data_patch_id[i + 2] > 0)
                        {
                            LDB.techs.dataArray[t].ItemPoints = LDB.techs.dataArray[t].ItemPoints.AddToArray<int>(data_patch_id[i + 2]);
                            LDB.techs.dataArray[t].Items = LDB.techs.dataArray[t].Items.AddToArray<int>(data_patch_id[i + 2 + 6]);
                        }
                    }
                }
                else
                {
                    double tech_multiple = multiple;
                    if (LDB.techs.dataArray[t].LevelCoef1 != 0)
                    {
                        if (tech_multiple > 50) tech_multiple = 50; // to avoid overflow in int32
                        LDB.techs.dataArray[t].HashNeeded *= (long)tech_multiple;
                        LDB.techs.dataArray[t].LevelCoef1 *= (int)tech_multiple;
                        LDB.techs.dataArray[t].LevelCoef2 *= (int)tech_multiple;
                    }
                    else
                    {
                        LDB.techs.dataArray[t].HashNeeded *= (long)tech_multiple;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            Logger.LogInfo("StarterTemplate OnDestroy() called");
            _harmony?.UnpatchSelf();
            Plugin.Log = null;
        }

        // {from_tech_id, to_tech_id, receip_id}
        private static readonly int[,] unlocks_move_data = {
                {1705, 1508, 79 }, //Adv warper to mission complete tech
                {1703, 1508, 100 }, //UM particle container to mission complete tech
                {1125, 1417, 29 }, //Adv casmir crystal to plane smelter
                {1502, 1203, 69 }, //Adv photon combiner to quantumn printing
                {1132, 1305, 35 }, //Adv nanotubes to quantumn chem plant
                {1133, 1305, 62 }, //Adv crystal silicon to quantumn chem plant
                {1403, 1304, 61 }, //Adv diamonds to adv mining machine
                {1131, 1705, 32 } //Adv graphene to grav matrix
            };

        // how to make this a dictionary from the start?
        // {tech_id, hash_needed, hash_scale, item_id0... item_id5, item_rate0... item_rate5}
        private static System.Collections.Generic.Dictionary<int, int[]> tech_data;
        private static void InitializeDictionary()
        {
            tech_data = new System.Collections.Generic.Dictionary<int, int[]>();
            tech_data[1001] = new int[] { 1200, 2, 30, -1, -1, -1, -1, -1, 1202, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1601] = new int[] { 9000, 10, 20, 20, -1, -1, -1, -1, 1301, 1201, 6003, 6004, 6005, 6006 };
            tech_data[1401] = new int[] { 1800, 2, 40, 40, -1, -1, -1, -1, 1202, 1301, 6003, 6004, 6005, 6006 };
            tech_data[1002] = new int[] { 9000, 10, 40, 40, -1, -1, -1, -1, 1202, 1301, 6003, 6004, 6005, 6006 };
            tech_data[1201] = new int[] { 3600, 10, 40, 40, -1, -1, -1, -1, 1101, 1104, 6003, 6004, 6005, 6006 };
            tech_data[1120] = new int[] { 9000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1101] = new int[] { 9000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1701] = new int[] { 9000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1602] = new int[] { 18000, 50, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1411] = new int[] { 21600, 100, 25, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1402] = new int[] { 18000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1412] = new int[] { 36000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1102] = new int[] { 36000, 100, 10, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1151] = new int[] { 36000, 100, 5, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1415] = new int[] { 72000, 100, 2, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1403] = new int[] { 90000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1501] = new int[] { 18000, 100, 40, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1311] = new int[] { 18000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1134] = new int[] { 36000, 100, 20, 30, 10, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1121] = new int[] { 72000, 100, 10, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1111] = new int[] { 36000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1702] = new int[] { 72000, 100, 20, 5, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1603] = new int[] { 72000, 100, 20, 5, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1608] = new int[] { 108000, 100, 20, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1413] = new int[] { 36000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1511] = new int[] { 108000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1502] = new int[] { 36000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1513] = new int[] { 144000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1302] = new int[] { 144000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1131] = new int[] { 72000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1122] = new int[] { 72000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1103] = new int[] { 108000, 100, 3, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1112] = new int[] { 72000, 100, -1, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1711] = new int[] { 180000, 100, 20, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1604] = new int[] { 144000, 100, 20, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1607] = new int[] { 180000, 100, 10, -1, 2, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1503] = new int[] { 108000, 100, 10, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1202] = new int[] { 108000, 100, 20, -1, 10, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1123] = new int[] { 216000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1113] = new int[] { 180000, 100, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1152] = new int[] { 144000, 100, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1703] = new int[] { 144000, 100, 25, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1104] = new int[] { 180000, 100, -1, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1414] = new int[] { 144000, 100, 20, 20, 2, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1521] = new int[] { 216000, 100, 20, -1, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1504] = new int[] { 108000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1416] = new int[] { 180000, 100, -1, 20, 20, 20, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1132] = new int[] { 540000, 100, 16, -1, 16, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1124] = new int[] { 720000, 100, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1114] = new int[] { 288000, 100, -1, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1605] = new int[] { 216000, 100, 20, 10, 2, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1512] = new int[] { 216000, 100, 20, 10, 2, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1133] = new int[] { 360000, 100, 8, 8, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1126] = new int[] { 360000, 100, 8, -1, 4, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1125] = new int[] { 360000, 100, -1, 8, 8, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1142] = new int[] { 360000, 100, 8, -1, 8, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1153] = new int[] { 540000, 100, -1, 8, 8, 4, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1712] = new int[] { 360000, 100, 2, 8, 8, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1606] = new int[] { 432000, 60, 10, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1312] = new int[] { 1440000, 250, 3, 1, 3, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1141] = new int[] { 216000, 100, 20, 20, 5, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1143] = new int[] { 360000, 100, 10, -1, 10, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1522] = new int[] { 360000, 100, 16, -1, 16, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1303] = new int[] { 720000, 100, 12, 4, 4, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1417] = new int[] { 1080000, 100, -1, 3, 3, 3, 7, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1704] = new int[] { 540000, 100, -1, 8, 8, 8, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1523] = new int[] { 900000, 100, 8, 8, 8, 8, 8, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1505] = new int[] { 900000, 100, 8, -1, 8, 8, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1203] = new int[] { 540000, 100, 6, 6, 6, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1304] = new int[] { 360000, 100, 10, 10, -1, 10, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1305] = new int[] { 360000, 100, 10, -1, 10, 10, 5, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1705] = new int[] { 9000000, 50, 2, 2, 2, 4, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1506] = new int[] { 1080000, 100, 10, 10, 3, 3, 5, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1145] = new int[] { 900000, 100, 16, -1, -1, -1, 8, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1144] = new int[] { 720000, 100, 6, 6, 6, 6, 6, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1507] = new int[] { 18000000, 50, 2, 2, 2, 2, 4, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[1508] = new int[] { 72000000, 50, -1, -1, -1, -1, -1, 1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4101] = new int[] { 1800, 10, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4102] = new int[] { 36000, 10, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4103] = new int[] { 300000, 90, 1, 12, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4104] = new int[] { 12000000, 60, 1, 1, -1, 1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2101] = new int[] { 3600, 20, 20, 20, -1, -1, -1, -1, 1101, 1104, 6003, 6004, 6005, 6006 };
            tech_data[2102] = new int[] { 36000, 50, 1, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2103] = new int[] { 108000, 100, 10, 10, 10, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2104] = new int[] { 300000, 100, -1, 6, 6, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2105] = new int[] { 1440000, 100, 2, -1, -1, 2, 2, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2201] = new int[] { 7200, 5, 30, -1, -1, -1, -1, -1, 1203, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2202] = new int[] { 36000, 10, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2203] = new int[] { 72000, 10, 20, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2204] = new int[] { 180000, 100, 12, -1, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2205] = new int[] { 240000, 100, -1, 12, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2206] = new int[] { 300000, 100, 12, -1, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2207] = new int[] { 360000, 100, 12, 12, -1, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2208] = new int[] { 1200000, 600, 1, -1, 1, 1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2301] = new int[] { 7200, 10, 20, 30, -1, -1, -1, -1, 1103, 1301, 6003, 6004, 6005, 6006 };
            tech_data[2302] = new int[] { 36000, 100, 2, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2303] = new int[] { 72000, 100, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2304] = new int[] { 180000, 200, -1, 12, 12, 12, 12, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2305] = new int[] { 480000, 200, 6, 6, -1, 6, 6, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2306] = new int[] { 600000, 200, 6, -1, 6, 6, 6, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2401] = new int[] { 9000, 10, 20, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2402] = new int[] { 36000, 100, 2, 10, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2403] = new int[] { 72000, 1000, 2, 2, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2404] = new int[] { 180000, 100, 12, -1, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2405] = new int[] { 300000, 100, -1, 12, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2406] = new int[] { 1440000, 100, 4, 4, -1, 4, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2701] = new int[] { 12000, 1, 30, -1, -1, -1, -1, -1, 1301, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2702] = new int[] { 36000, 10, 10, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2703] = new int[] { 60000, 100, 3, 30, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2704] = new int[] { 192000, 100, 15, 15, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2705] = new int[] { 240000, 100, 15, 15, 15, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2501] = new int[] { 7200, 10, 15, 30, 30, -1, -1, -1, 1030, 1006, 1109, 6004, 6005, 6006 };
            tech_data[2502] = new int[] { 72000, 50, 2, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2503] = new int[] { 216000, 20, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2504] = new int[] { 600000, 100, 12, -1, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2505] = new int[] { 2700000, 100, 4, 4, -1, 4, 4, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2601] = new int[] { 18000, 5, 12, -1, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2602] = new int[] { 72000, 10, 6, 16, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2603] = new int[] { 144000, 20, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2604] = new int[] { 360000, 100, 12, 12, -1, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2605] = new int[] { 1440000, 150, -1, 4, 4, -1, 4, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2901] = new int[] { 10800, 10, 40, 20, -1, -1, -1, -1, 1006, 1202, 6003, 6004, 6005, 6006 };
            tech_data[2902] = new int[] { 36000, 100, 5, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2903] = new int[] { 180000, 10, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2904] = new int[] { 720000, 200, 2, 2, -1, 10, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[2905] = new int[] { 1080000, 100, -1, -1, 4, -1, 6, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3101] = new int[] { 36000, 100, 40, 40, 40, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3102] = new int[] { 108000, 100, 20, -1, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3103] = new int[] { 144000, 100, -1, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3104] = new int[] { 300000, 100, 12, -1, 12, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3105] = new int[] { 480000, 100, 12, 12, -1, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3106] = new int[] { 1200000, 100, 6, -1, 6, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3201] = new int[] { 54000, 100, 40, 40, 40, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3202] = new int[] { 144000, 100, 20, -1, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3203] = new int[] { 180000, 100, -1, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3204] = new int[] { 360000, 100, 12, -1, 12, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3205] = new int[] { 420000, 100, 12, 12, -1, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3206] = new int[] { 480000, 100, -1, 12, 12, -1, 12, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3207] = new int[] { 1620000, 100, 4, 4, -1, 4, 4, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3701] = new int[] { 36000, 100, -1, 40, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3702] = new int[] { 108000, 100, -1, 20, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3703] = new int[] { 144000, 100, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3704] = new int[] { 300000, 100, 12, -1, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3705] = new int[] { 480000, 100, -1, 12, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3706] = new int[] { 1200000, 100, 6, -1, -1, 6, 6, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3301] = new int[] { 36000, 10, 40, 40, 40, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3302] = new int[] { 108000, 100, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3303] = new int[] { 144000, 100, -1, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3304] = new int[] { 300000, 200, 6, -1, 6, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3305] = new int[] { 900000, 120, -1, 4, 4, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4001] = new int[] { 36000, 100, 40, 40, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4002] = new int[] { 108000, 100, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4003] = new int[] { 144000, 100, 20, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4004] = new int[] { 180000, 100, 20, -1, 20, 20, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[4004] = new int[] { 360000, 100, 12, 12, -1, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3401] = new int[] { 36000, 100, 40, -1, 40, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3402] = new int[] { 108000, 100, -1, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3403] = new int[] { 144000, 100, 20, -1, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3404] = new int[] { 300000, 100, 12, 12, -1, 18, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3405] = new int[] { 360000, 100, 12, -1, 12, 18, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3406] = new int[] { 1440000, 50, 10, 10, -1, 10, 1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3501] = new int[] { 36000, 100, 40, 40, 40, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3502] = new int[] { 108000, 100, 20, -1, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3503] = new int[] { 144000, 100, -1, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3504] = new int[] { 300000, 100, 12, -1, 12, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3505] = new int[] { 720000, 90, 6, 6, -1, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3506] = new int[] { 960000, 150, -1, 3, 3, 3, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3507] = new int[] { 2400000, 65, -1, -1, 3, 3, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3508] = new int[] { 5400000, 100, 1, 1, -1, 1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3601] = new int[] { 36000, 100, 20, 60, -1, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3602] = new int[] { 180000, 100, -1, 20, 20, -1, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3603] = new int[] { 180000, 100, 20, -1, 20, 20, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3604] = new int[] { 480000, 100, 12, 12, -1, 12, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3605] = new int[] { 1800000, 100, -1, 4, 4, -1, 4, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3901] = new int[] { 600000, 100, -1, -1, -1, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3902] = new int[] { 900000, 100, -1, -1, -1, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };
            tech_data[3903] = new int[] { 1200000, 100, -1, -1, -1, 6, -1, -1, 6001, 6002, 6003, 6004, 6005, 6006 };


        }

    }
}
