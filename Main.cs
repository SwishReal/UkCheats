using UnityEngine.SceneManagement;
using PluginConfig.API.Functionals;
using PluginConfig.API.Fields;
using PluginConfig.API;
using PluginConfig;
using UnityEngine;
using BepInEx;

namespace UKCheats
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)] //Mod info
    [BepInDependency(PluginConfiguratorController.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)] //PluginConfig dependency
    public class Main : BaseUnityPlugin
    {
        //Mod info
        public const string pluginGuid = "swish.ultrakill.cheats";
        public const string pluginName = "UK Cheats";
        public const string pluginVersion = "1.0.0";

        //Mod variables
        private NewMovement movementController;
        private WeaponCharges weaponCharges;
        private StatsManager statsManager;
        private bool isThisScene = false;

        //Cheats variables
        private bool isInfHealth;
        private bool isInfDash;
        private bool isInstantRecharge;

        //Plugin Configurator
        private PluginConfigurator config;

        public void Awake() //Awake method (really)
        {
            Logger.LogInfo("UkCheats is started!");

            //Creating panel in PluginConfig
            config = PluginConfigurator.Create("Uk Cheats", "swish_ultrakill_cheats");

            ButtonField EzPrank = new ButtonField(config.rootPanel, "Auto P-Rank level", "swish_ultrakill_cheats_ezprank");
            BoolField infHealth = new BoolField(config.rootPanel, "Infinite Health", "swish_ultrakill_cheats_infhealth", false);
            BoolField infDash = new BoolField(config.rootPanel, "Infinite Dash", "swish_ultrakill_cheats_infDash", false);
            BoolField instantRecharge = new BoolField(config.rootPanel, "Instant Recharge", "swish_ultrakill_cheats_instantRecharge", false);

            //Adding lamda expressions
            EzPrank.onClick += EzPRankCheat;

            infHealth.onValueChange += (BoolField.BoolValueChangeEvent e) => 
            {
                isInfHealth = e.value;
            };

            infDash.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                isInfDash = e.value;
            };

            instantRecharge.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                isInstantRecharge = e.value;
            };
        }

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded; //Adding lamda expressions
        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo("[UkCheats] Scene loaded: " + scene.name + " | Load Mode: " + mode);
            isThisScene = false; //Flag for optimization
        }

        public void Update()
        {
            if (movementController != null || weaponCharges != null || statsManager != null)
            {
                if (movementController.hp < 100f && isInfHealth) //Cheats 1
                {
                    movementController.FullHeal(true);
                    movementController.ForceAntiHP(0);
                }
                else if (movementController.boostCharge < 300f && isInfDash) //Cheats 2
                {
                    movementController.FullStamina();
                }
                else if (isInstantRecharge) //Cheats 3
                {
                    weaponCharges.MaxCharges();
                }
            }
            else //Trying to find some components
            {
                FindMovementController();
                FindWeaponCharges();
                FindStatsManager();
            }
        }

        private void EzPRankCheat() // OP cheat
        {
            FinalRoom finalRoom = FindFinalRoom(); //Trying to find a FinalRoom
            if (finalRoom == null)
            {
                Logger.LogWarning("[UkCheats] FinalRoom not found!");
                return;
            }

            statsManager.StopTimer();
            statsManager.seconds = Random.Range(statsManager.timeRanks[3] - 5f, statsManager.timeRanks[3]); //Realistic stats
            statsManager.kills = statsManager.killRanks[3];
            statsManager.stylePoints = Random.Range(statsManager.styleRanks[3], statsManager.styleRanks[3] + 10000); //Realistic stats

            finalRoom.transform.parent = null; //Change final room parent bc it need be activated
            finalRoom.gameObject.SetActive(true);
            finalRoom.GetComponentInChildren<FinalDoor>().OpenDoors(); //OPEN DOORS!!! OMG

            if (movementController != null)
                movementController.transform.position = finalRoom.dropPoint.position; //Teleporting player to the FinalRoom "Pit"

            Logger.LogInfo("[UkCheats] OMG U SO SKILLED, GO TOUCH GRASS BROO!!! Nah, im joking XD");
        }

        //Some finding methods
        private void FindMovementController()
        {
            movementController = FindFirstObjectByType<NewMovement>();

            if (!isThisScene)
            {
                if (movementController != null)
                {
                    Logger.LogInfo("[UkCheats] NewMovement founded");
                    isThisScene = true;
                }
                else
                {
                    Logger.LogWarning("[UkCheats] NewMovement not found");
                    isThisScene = true;
                }
            }
        }

        private void FindWeaponCharges()
        {
            weaponCharges = FindFirstObjectByType<WeaponCharges>();

            if (!isThisScene)
            {
                if (weaponCharges != null)
                {
                    Logger.LogInfo("[UkCheats] WeaponCharges founded");
                    isThisScene = true;
                }
                else
                {
                    Logger.LogWarning("[UkCheats] WeaponCharges not found!");
                    isThisScene = true;
                }
            }
        }

        private void FindStatsManager()
        {
            statsManager = FindFirstObjectByType<StatsManager>();

            if (!isThisScene)
            {
                if (statsManager != null)
                {
                    Logger.LogInfo("[UkCheats] StatsManager founded");
                    isThisScene = true;
                }
                else
                {
                    Logger.LogWarning("[UkCheats] StatsManager not found!");
                    isThisScene = true;
                }
            }
        }

        private FinalRoom FindFinalRoom()
        {
            FinalRoom[] allFinalRooms = Resources.FindObjectsOfTypeAll<FinalRoom>();
            return allFinalRooms.Length > 0 ? allFinalRooms[0] : null;
        }
    }
}