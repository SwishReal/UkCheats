using UnityEngine.SceneManagement;
using PluginConfig.API.Functionals;
using PluginConfig.API.Fields;
using PluginConfig.API;
using PluginConfig;
using UnityEngine;
using BepInEx;
using System.Collections;
using UnityEngine.SocialPlatforms;
using ULTRAKILL.Cheats;
using PluginConfig.API.Decorators;
using System.IO;

namespace UKCheats
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)] //Mod info
    [BepInDependency(PluginConfiguratorController.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)] //PluginConfig dependency
    public class Main : BaseUnityPlugin
    {
        // Mod info
        public const string pluginGuid = "swish.ultrakill.cheats";
        public const string pluginName = "UK Cheats";
        public const string pluginVersion = "1.0.0";

        // Game components
        private NewMovement movementController;
        //private WeaponCharges weaponCharges;
        private CheatsManager cheatsManager;

        // Cheats state
        private bool isInfHealth;
        private bool isInstantRecharge;

        //Some style bro
        static readonly string modFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string spritePath = Path.Combine(modFolder, "plugin_icon.png");

        // Plugin Configurator
        private PluginConfigurator config;

        private ConfigPanel generalPanel;
        private ConfigPanel movementPanel;
        private ConfigPanel weaponsPanel;
        private ConfigPanel enemiesPanel;
        private ConfigPanel visualPanel;
        private ConfigPanel specialPanel;

        private BoolField full_bright;
        private BoolField _invincibility;

        private BoolField _noclip;
        private BoolField _flight;
        private BoolField infinite_wall_jumps;
        private BoolField infinite_stamina;

        private BoolField no_weapon_cooldown;
        private BoolField Infinite_power_ups;

        private BoolField blind_enemies;
        private BoolField enemies_attack_each_other;
        private BoolField enemy_ignore_player;
        private BoolField disable_enemy_spawns;
        private BoolField invincible_enemies;
        private ButtonField kill_all_enemies;
 
        private BoolField hide_weapons;
        private BoolField hide_ui;

        private BoolField clash_mode;
        private BoolField drone_haunting;

        // Hakita, yours game protection is beatiful!
        private FullBright fullBright = new FullBright();
        private Invincibility invincibility = new Invincibility();

        private Noclip noclip = new Noclip();
        private Flight flight = new Flight();
        private InfiniteWallJumps infiniteWallJumps = new InfiniteWallJumps();
        private bool isInfDash;

        private NoWeaponCooldown noWeaponCooldown = new NoWeaponCooldown();
        private InfinitePowerUps InfinitePowerUps = new InfinitePowerUps();

        private BlindEnemies blindEnemies = new BlindEnemies();
        private EnemiesHateEnemies enemiesAttackEachOther = new EnemiesHateEnemies();
        private EnemyIgnorePlayer enemyIgnorePlayer = new EnemyIgnorePlayer();
        private DisableEnemySpawns disableEnemySpawns = new DisableEnemySpawns();
        private InvincibleEnemies invincibleEnemies = new InvincibleEnemies();
        private KillAllEnemies killAllEnemies = new KillAllEnemies();

        private HideWeapons hideWeapons = new HideWeapons();
        private HideUI hideUI = new HideUI();

        private CrashMode clashMode = new CrashMode();
        private GhostDroneMode droneHaunting = new GhostDroneMode();

        public void Awake()
        {
            Logger.LogInfo("[UKCheats] Mod initialized!");

            // Create plugin configuration
            config = PluginConfigurator.Create("UK Cheats", "swish_ultrakill_cheats");

            config.SetIconWithURL($"file://{spritePath}");

            new ConfigHeader(config.rootPanel, "Standart ULTRAKILL Cheats");

            new ButtonField(config.rootPanel, "Auto P-Rank level", "auto_prank").onClick += EzPRankCheat;

            generalPanel = new ConfigPanel(config.rootPanel, "General", "general_panel");
            movementPanel = new ConfigPanel(config.rootPanel, "Movement", "movement_panel");
            weaponsPanel = new ConfigPanel(config.rootPanel, "Weapons", "general_panel");
            enemiesPanel = new ConfigPanel(config.rootPanel, "Enemies", "general_panel");
            visualPanel = new ConfigPanel(config.rootPanel, "Visual", "general_panel");
            specialPanel = new ConfigPanel(config.rootPanel, "Special", "general_panel");

            // Creating options
            // General

            full_bright = new BoolField(generalPanel, "Full Bright", "full_bright", false);
            _invincibility = new BoolField(generalPanel, "Invincibility", "invincibility", false);

            // Movement

            _noclip = new BoolField(movementPanel, "Noclip", "noclip", false);
            _flight = new BoolField(movementPanel, "Flight", "flight", false);
            infinite_wall_jumps = new BoolField(movementPanel, "Infinite Wall Jumps", "infinite_wall_jumps", false);
            infinite_stamina = new BoolField(movementPanel, "Infinite Stamina", "infinite_stamina", false);

            // Weapons

            no_weapon_cooldown = new BoolField(weaponsPanel, "No Weapon Cooldown", "no_weapon_cooldown", false);
            Infinite_power_ups = new BoolField(weaponsPanel, "Infinite Power-Ups", "Infinite_power_ups", false);

            // Enemies

            blind_enemies = new BoolField(enemiesPanel, "Bling Enemies", "blind_enemies", false);
            enemies_attack_each_other = new BoolField(enemiesPanel, "Enemies Attach Each Other", "enemies_attack_each_other", false);
            enemy_ignore_player = new BoolField(enemiesPanel, "Enemy Ignore Player", "enemy_ignore_player", false);
            disable_enemy_spawns = new BoolField(enemiesPanel, "Disable Enemy Spawns", "disable_enemy_spawns", false);
            invincible_enemies = new BoolField(enemiesPanel, "Invicible Enemies", "invincible_enemies", false);
            kill_all_enemies = new ButtonField(enemiesPanel, "Kill All Enemies", "kill_all_enemies");

            // Visual

            hide_weapons = new BoolField(visualPanel, "Hide Weapons", "hide_weapons", false);
            hide_ui = new BoolField(visualPanel, "Hide UI", "hide_ui", false);

            // Special

            clash_mode = new BoolField(specialPanel, "Clash Mode", "clash_mode", false);
            drone_haunting = new BoolField(specialPanel, "Drone Haunting", "drone_haunting", false);

            // Adding to options lamda expressions
            // General

            full_bright.onValueChange += (e) => { cheatsManager.SetCheatActive(fullBright, e.value, false); };
            _invincibility.onValueChange += (e) => { cheatsManager.SetCheatActive(invincibility, e.value, false); };

            // Movement

            _noclip.onValueChange += (e) => { cheatsManager.SetCheatActive(noclip, e.value, false); };
            _flight.onValueChange += (e) => { cheatsManager.SetCheatActive(flight, e.value, false); };
            infinite_wall_jumps.onValueChange += (e) => { cheatsManager.SetCheatActive(infiniteWallJumps, e.value, false); };
            infinite_stamina.onValueChange += (e) => { isInfDash = e.value; };

            // Weapons

            no_weapon_cooldown.onValueChange += (e) => { cheatsManager.SetCheatActive(noWeaponCooldown, e.value, false); };
            Infinite_power_ups.onValueChange += (e) => { cheatsManager.SetCheatActive(InfinitePowerUps, e.value, false); };

            // Enemies

            blind_enemies.onValueChange += (e) => { cheatsManager.SetCheatActive(blindEnemies, e.value, false); };
            enemies_attack_each_other.onValueChange += (e) => { cheatsManager.SetCheatActive(enemiesAttackEachOther, e.value, false); };
            enemy_ignore_player.onValueChange += (e) => { cheatsManager.SetCheatActive(enemyIgnorePlayer, e.value, false); };
            disable_enemy_spawns.onValueChange += (e) => { cheatsManager.SetCheatActive(disableEnemySpawns, e.value, false); };
            invincible_enemies.onValueChange += (e) => { cheatsManager.SetCheatActive(invincibleEnemies, e.value, false); };
            kill_all_enemies.onClick += () => { killAllEnemies.Enable(cheatsManager); };

            // Visual

            hide_weapons.onValueChange += (e) => { cheatsManager.SetCheatActive(infiniteWallJumps, e.value, false); };
            hide_ui.onValueChange += (e) =>{cheatsManager.SetCheatActive(infiniteWallJumps, e.value, false);};

            // Special

            clash_mode.onValueChange += (e) =>{ cheatsManager.SetCheatActive(infiniteWallJumps, e.value, false); };
            drone_haunting.onValueChange += (e) =>{ cheatsManager.SetCheatActive(infiniteWallJumps, e.value, false); };

            ResetOptions();

            // Setup scene handling
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo($"[UKCheats] Scene loaded: {scene.name}");
            StartCoroutine(InitializeGameComponents());
            ResetOptions();
        }

        private IEnumerator InitializeGameComponents()
        {
            int attempts = 0;
            const int maxAttempts = 5;
            const float retryDelay = 0.5f;

            while (attempts < maxAttempts)
            {
                attempts++;
                yield return new WaitForSeconds(retryDelay);

                movementController = FindObjectOfType<NewMovement>();
                //weaponCharges = FindObjectOfType<WeaponCharges>();
                cheatsManager = FindObjectOfType<CheatsManager>();

                if (movementController != null && /*weaponCharges != null &&*/ cheatsManager != null)
                {
                    Logger.LogInfo("[UKCheats] All game components found!");
                    yield break;
                }
            }

            Logger.LogWarning("[UKCheats] Failed to find some game components");
        }

        public void Update()
        {
            if (movementController == null || /*weaponCharges == null ||*/ cheatsManager == null)
                return;

            if (isInfHealth && movementController.hp < 100f)
            {
                movementController.FullHeal(true);
                movementController.ForceAntiHP(0);
            }

            if (isInfDash && movementController.boostCharge < 300f)
            {
                movementController.FullStamina();
            }
        }

        private void ActivateCheat(ICheat Cheat, bool IsOn, CheatsManager _cheatsManager)
        {
            if (IsOn)
            {
                Cheat.Enable(_cheatsManager);
                StartCoroutine(Cheat.Coroutine(_cheatsManager));
            }
            else
            {
                Cheat.Disable();
            }
        }

        private void EzPRankCheat()
        {
            var statsManager = FindObjectOfType<StatsManager>();
            var finalRoom = FindObjectOfType<FinalRoom>();

            if (statsManager == null || finalRoom == null)
            {
                Logger.LogWarning("[UKCheats] Required components for P-Rank not found!");
                return;
            }

            // Set P-Rank stats
            statsManager.StopTimer();
            statsManager.seconds = statsManager.timeRanks[3] - 1f;
            statsManager.kills = statsManager.killRanks[3];
            statsManager.stylePoints = statsManager.styleRanks[3] + 5000;

            // Activate final room
            finalRoom.transform.SetParent(null, true);
            finalRoom.gameObject.SetActive(true);
            finalRoom.GetComponentInChildren<FinalDoor>().OpenDoors();

            // Teleport player
            if (movementController != null)
            {
                movementController.transform.position = finalRoom.dropPoint.position;
            }

            Logger.LogInfo("[UKCheats] P-Rank activated!");
        }

        private void ResetOptions()
        {
            // General

            full_bright.value = full_bright.defaultValue;
            _invincibility.value = _invincibility.defaultValue;

            // Movement

            _noclip.value = _noclip.defaultValue;
            _flight.value = _flight.defaultValue;
            infinite_wall_jumps.value = infinite_wall_jumps.defaultValue;
            infinite_stamina.value = infinite_stamina.defaultValue;

            // Weapons

            no_weapon_cooldown.value = no_weapon_cooldown.defaultValue;
            Infinite_power_ups.value = Infinite_power_ups.defaultValue;

            // Enemies

            blind_enemies.value = blind_enemies.defaultValue;
            enemies_attack_each_other.value = enemies_attack_each_other.defaultValue;
            enemy_ignore_player.value = enemy_ignore_player.defaultValue;
            disable_enemy_spawns.value = disable_enemy_spawns.defaultValue;
            invincible_enemies.value = invincible_enemies.defaultValue;

            // Visual

            hide_weapons.value = hide_weapons.defaultValue;
            hide_ui.value = hide_ui.defaultValue;

            // Special

            clash_mode.value = clash_mode.defaultValue;
            drone_haunting.value = drone_haunting.defaultValue;
        }
    }
}
