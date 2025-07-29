using UnityEngine.SceneManagement;
using PluginConfig.API.Functionals;
using PluginConfig.API.Fields;
using PluginConfig.API;
using PluginConfig;
using UnityEngine;
using BepInEx;
using System.Collections;
using ULTRAKILL.Cheats;
using PluginConfig.API.Decorators;
using System.IO;

namespace UKCheats
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInDependency(PluginConfiguratorController.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        // Mod info
        public const string pluginGuid = "swish.ultrakill.cheats";
        public const string pluginName = "UK Cheats";
        public const string pluginVersion = "2.1.0";

        // Game components
        private NewMovement movementController;
        private CheatsManager cheatsManager;

        // Cheats state
        private bool isInfHealth;
        private bool isInstantRecharge;

        //Some style bro
        static readonly string modFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string spritePath = Path.Combine(modFolder, "plugin_icon.png");

        //Auto-Complete Level Enum
        enum GameRanks
        {
            D,
            C,
            B,
            A,
            S,
            P
        }

        // Plugin Configurator
        private PluginConfigurator config;

        private ConfigPanel autoCompleteLevel;

        private EnumField<GameRanks> gameRanksField;
        private BoolField withDamage;
        private ButtonField completeLevelButton;

        private ConfigPanel generalPanel;
        private ConfigPanel movementPanel;
        private ConfigPanel weaponsPanel;
        private ConfigPanel enemiesPanel;

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

        public void Awake()
        {
            Logger.LogInfo("[UKCheats] Mod initialized!");

            // Create plugin configuration
            config = PluginConfigurator.Create("UK Cheats", "swish_ultrakill_cheats");

            config.SetIconWithURL($"file://{spritePath}");

            new ConfigHeader(config.rootPanel, "Standart ULTRAKILL Cheats");

            //new ButtonField(config.rootPanel, "Auto P-Rank level", "auto_prank").onClick += EzPRankCheat;

            autoCompleteLevel = new ConfigPanel(config.rootPanel, "Auto-Complete Page", "auto_complete_page");
            generalPanel = new ConfigPanel(config.rootPanel, "General", "general_panel");
            movementPanel = new ConfigPanel(config.rootPanel, "Movement", "movement_panel");
            weaponsPanel = new ConfigPanel(config.rootPanel, "Weapons", "general_panel");
            enemiesPanel = new ConfigPanel(config.rootPanel, "Enemies", "general_panel");

            // Creating options
            //Auto-Complete
            gameRanksField = new EnumField<GameRanks>(autoCompleteLevel, "Rank", "rank", GameRanks.P);
            withDamage = new BoolField(autoCompleteLevel, "Damaged", "damaged", false, false);
            completeLevelButton = new ButtonField(autoCompleteLevel, "Complete Level", "complete_level");

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

            // Adding to options lamda expressions
            //Auto-Complete
            completeLevelButton.onClick += () => { AutoCompleteCheat(gameRanksField.value, withDamage.value); };

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
                cheatsManager = FindObjectOfType<CheatsManager>();

                if (movementController != null && cheatsManager != null)
                {
                    Logger.LogInfo("[UKCheats] All game components found!");
                    yield break;
                }
            }

            Logger.LogWarning("[UKCheats] Failed to find some game components");
        }

        public void Update()
        {
            if (movementController == null || cheatsManager == null)
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

        private void AutoCompleteCheat(GameRanks gameRanks, bool WithDamage)
        {
            var statsManager = FindObjectOfType<StatsManager>();
            var finalRoom = FindFinalRoom();

            if (statsManager == null || finalRoom == null)
            {
                Logger.LogWarning("[UKCheats] Required components for Auto-Complete not found!");
                return;
            }

            statsManager.StopTimer();

            if (gameRanks != GameRanks.P)
            {
                statsManager.restarts = 1;
            }

            if (WithDamage)
            {
                statsManager.tookDamage = true;
            }

            if (gameRanks == GameRanks.P)
            {
                statsManager.seconds = Random.Range(statsManager.timeRanks[3] - 60f, statsManager.timeRanks[3]);
                statsManager.kills = statsManager.killRanks[3];
                statsManager.stylePoints = statsManager.styleRanks[3];
            }
            else if (gameRanks == GameRanks.S)
            {
                statsManager.seconds = Random.Range(statsManager.timeRanks[3] - 60f, statsManager.timeRanks[3]);
                statsManager.kills = statsManager.killRanks[3];
                statsManager.stylePoints = statsManager.styleRanks[3];
            }
            else if (gameRanks == GameRanks.A)
            {
                statsManager.seconds = Random.Range(statsManager.timeRanks[2] - 25f, statsManager.timeRanks[2]);
                statsManager.kills = statsManager.killRanks[2];
                statsManager.stylePoints = statsManager.styleRanks[2];
            }
            else if (gameRanks == GameRanks.B)
            {
                statsManager.seconds = Random.Range(statsManager.timeRanks[1] - 20f, statsManager.timeRanks[1]);
                statsManager.kills = statsManager.killRanks[1];
                statsManager.stylePoints = statsManager.styleRanks[1];
            }
            else if (gameRanks == GameRanks.C)
            {
                statsManager.seconds = Random.Range(statsManager.timeRanks[0] - 15f, statsManager.timeRanks[0]);
                statsManager.kills = statsManager.killRanks[0];
                statsManager.stylePoints = statsManager.styleRanks[0];
            }
            else if (gameRanks == GameRanks.D)
            {
                statsManager.seconds = Random.Range(statsManager.timeRanks[0], statsManager.timeRanks[0] + 60f);
                statsManager.kills = statsManager.killRanks[0];
                statsManager.stylePoints = statsManager.styleRanks[0] - Random.Range(1500, 3500);
            }

            finalRoom.transform.SetParent(null, true);
            finalRoom.gameObject.SetActive(true);
            finalRoom.GetComponentInChildren<FinalDoor>().OpenDoors();

            if (movementController != null)
            {
                movementController.transform.position = finalRoom.dropPoint.position;
            }

            Logger.LogInfo("[UKCheats] Auto-Complete activated!");
        }

        private FinalRoom FindFinalRoom()
        {
            FinalRoom[] array = Resources.FindObjectsOfTypeAll<FinalRoom>();
            return (array.Length != 0) ? array[0] : null;
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
        }
    }
}
