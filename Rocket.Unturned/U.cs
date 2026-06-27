using Rocket.API;
using Rocket.API.Collections;
using Rocket.API.Extensions;
using Rocket.Core;
using Rocket.Core.RCON;
using Rocket.Core.Assets;
using Rocket.Core.Extensions;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Effects;
using Rocket.Unturned.Events;
using Rocket.Unturned.Permissions;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using Rocket.Unturned.Serialisation;
using Rocket.Unturned.Homes;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps;
using Rocket.Unturned.Utils;
using Rocket.Core.Utils;
using SDG.Framework.Modules;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rocket.Unturned
{
    public class U : MonoBehaviour, IRocketImplementation, IModuleNexus
    {
        private static GameObject rocketGameObject = null!;
        public static U Instance = null!;

        private static readonly TranslationList defaultTranslations = new TranslationList(){
            { "command_generic_failed_find_player","Failed to find player"},
                { "command_generic_invalid_parameter","Invalid parameter"},
                { "command_generic_target_player_not_found","Target player not found"},
                { "command_generic_teleport_while_driving_error","You cannot teleport while driving or riding in a vehicle."},
                { "command_admin_player_invalid","Player {0} is not invalid or not found."},
                { "command_admin_player_is_admin","Player {0} is an admin."},
                { "command_admin_success","Successfully admined {0}."},
                { "command_unadmin_player_invalid","Player {0} is not invalid or not found."},
                { "command_unadmin_player_is_not_admin","Player {0} is not an admin."},
                { "command_unadmin_success","Successfully unadmined {0}"},
                { "command_god_enable_console","{0} enabled Godmode"},
                { "command_god_enable_private","You can feel the strength now..."},
                { "command_god_disable_console","{0} disabled Godmode"},
                { "command_god_disable_private","The godly powers left you..."},
                { "command_vanish_enable_console","{0} enabled Vanishmode"},
                { "command_vanish_enable_private","You are vanished now..."},
                { "command_vanish_disable_console","{0} disabled Vanishmode"},
                { "command_vanish_disable_private","You are no longer vanished..."},
                { "command_duty_enable_console","{0} is in duty"},
                { "command_duty_enable_private","You are in duty now..."},
                { "command_duty_disable_console","{0} is no longer in duty"},
                { "command_duty_disable_private","You are no longer in duty..."},
                { "command_bed_no_bed_found_private","You do not have a bed to teleport to."},
                { "command_i_too_much","You have tried to spawn too many items! The limit is {0}." },
                { "command_i_blacklisted","This item is restricted!" },
                { "command_i_giving_console","Giving {0} item {1}:{2}"},
                { "command_i_giving_private","Giving you item {0}x {1} ({2})"},
                { "command_z_giving_console","Spawning {1} zombies near {0}"},
                { "command_z_giving_private","Spawning {0} zombies nearby"},
                { "command_position_get","Current position: {0}"},
                { "command_i_giving_failed_private","Failed giving you item {0}x {1} ({2})"},
                { "command_rgive_player_not_found","Player not found: {0}"},
                { "command_rgive_invalid_amount","Invalid amount: {0}"},
                { "command_rgive_item_not_found","Item not found: {0}"},
                { "command_rgive_failed","Failed giving {0} item {1}x {2} ({3})"},
                { "command_rgive_giving_console","{0} gave {1} item {2}:{3}"},
                { "command_rgive_giving_self","Giving you {0}x {1} ({2})"},
                { "command_rgive_giving_executor","Gave {0} {1}x {2} ({3})"},
                { "command_rgive_giving_target","You received {0}x {1} ({2})"},
                { "command_rgive_currency_console","{0} granted {1} currency {2} x{3}"},
                { "command_rgive_currency_self","Granted you currency {0}x {1} ({2})"},
                { "command_rgive_currency_executor","Granted {0} currency {1}x {2} ({3})"},
                { "command_rgive_currency_target","You received currency {0}x {1} ({2})"},
                { "command_rvehicle_player_not_found","Player not found: {0}"},
                { "command_rvehicle_not_found","Vehicle not found: {0}"},
                { "command_rvehicle_failed","Failed spawning vehicle {1} ({2}) for {0}"},
                { "command_rvehicle_spawn_console","{0} spawned vehicle {2} for {1}"},
                { "command_rvehicle_spawn_self","Spawned vehicle {0} ({1}) ahead of you"},
                { "command_rvehicle_spawn_executor","Spawned vehicle {1} ({2}) for {0}"},
                { "command_rvehicle_spawn_target","You received vehicle {0} ({1})"},
                { "command_v_giving_console","Giving {0} vehicle {1}"},
                { "command_v_blacklisted","This vehicle is restricted!" },
                { "command_v_giving_private","Giving you a {0} ({1})"},
                { "command_v_giving_failed_private","Failed giving you a {0} ({1})"},
                { "command_tps_tps","TPS: {0}"},
                { "command_tps_running_since","Running since: {0} UTC"},
                { "command_p_reload_private","Reloaded permissions"},
                { "command_p_groups_private","{0} groups are: {1}"},
                { "command_p_permissions_private","{0} permissions are: {1}"},
                { "command_tp_teleport_console","{0} teleported to {1}"},
                { "command_tp_teleport_private","Teleported to {0}"},
                { "command_tp_failed_find_destination","Failed to find destination"},
                { "command_tphere_teleport_console","{0} was teleported to {1}"},
                { "command_tphere_teleport_from_private","Teleported {0} to you"},
                { "command_tphere_teleport_to_private","You were teleported to {0}"},
                { "command_tpwp_marker_not_set","Your waypoint is not set"},
                { "command_tpwp_failed_raycast","Failed to find ground"},
                { "command_clear_error","There was an error clearing {0} inventory."},
                { "command_clear_private","Your inventory was cleared!"},
                { "command_clear_other","Your inventory was cleared by {0}!"},
                { "command_clear_other_success","You successfully cleared {0} inventory."},
                { "command_investigate_private","{0} SteamID64 is {1}"},
                { "command_heal_success_me","{0} was successfully healed"},
                { "command_heal_success_other","You were healed by {0}"},
                { "command_heal_success","You were healed"},
                { "command_rocket_plugins_loaded","Loaded: {0}"},
                { "command_rocket_plugins_unloaded","Unloaded: {0}"},
                { "command_rocket_plugins_failure","Failure: {0}"},
                { "command_rocket_plugins_cancelled","Cancelled: {0}"},
                { "command_rocket_reload_plugin","Reloading... {0}"},
                { "command_rocket_reloaded_plugin","Reloaded {0}"},
                { "command_rocket_reload_plugin_error","An error occured while reloading plugin {0}. See logs for more details."},
                { "command_rocket_not_loaded","The plugin {0} is not loaded"},
                { "command_rocket_unloading_plugin","Unloading... {0}"},
                { "command_rocket_unloading_plugin_error","An error occured while unloading plugin {0}. See logs for more details."},
                { "command_rocket_unloaded_plugin","Unloaded {0}"},
                { "command_rocket_load_plugin","Loading... {0}"},
                { "command_rocket_loaded_plugin_error","Loaded {0}"},
                { "command_rocket_loaded_plugin","Loaded {0}"},
                { "command_rocket_already_loaded","The plugin {0} is already loaded"},
                { "command_rocket_reload","Reloading Rocket"},
                { "command_rocket_reload_disabled", "Please reload individual plugins instead" },
                { "command_p_group_not_found","Group not found"},
                { "command_p_group_player_added","{0} was added to the group {1}"},
                { "command_p_group_player_removed","{0} was removed from from the group {1}"},
                { "command_p_unknown_error","Unknown error"},
                { "command_p_player_not_found","{0} was not found"},
                { "command_p_group_not_found","{1} was not found"},
                { "command_p_duplicate_entry","{0} is already in the group {1}"},
                { "command_p_permissions_reload","Permissions reloaded"},
                { "command_rocket_plugin_not_found","Plugin {0} not found"},
                { "command_clear_success","You successfully cleared {0} items"},
                { "command_more_usage", "Usage: /more <amount>" },
                { "command_more_dequipped", "No item being held in hands." },
                { "command_more_give", "Giving {0} of item: {1}." },
                { "command_itemlist_success_console", "Exported {0} valid items to {1} ({2} skipped)"},
                { "command_itemlist_success_player", "Exported {0} valid items. File saved on server: {1} ({2} skipped)"},
                { "command_itemlist_failed", "Failed to export item list: {0}"},
                { "command_vehiclelist_success_console", "Exported {0} valid vehicles to {1} ({2} skipped)"},
                { "command_vehiclelist_success_player", "Exported {0} valid vehicles. File saved on server: {1} ({2} skipped)"},
                { "command_vehiclelist_failed", "Failed to export vehicle list: {0}"},
                { "tpa_target_not_found", "Target player not found" },
                { "tpa_combat_start", "You entered combat mode" },
                { "tpa_combat_expire", "Combat mode ended" },
                { "tpa_raid_start", "You entered raid mode" },
                { "tpa_raid_expire", "Raid mode ended" },
                { "tpa_help", "[[b]]TPA commands:[[/b]]&#xA;/tpa [player] - send request&#xA;/tpa accept - accept&#xA;/tpa deny - deny&#xA;/tpa cancel - cancel" },
                { "tpa_cooldown", "Wait [[b]]{0}[[/b]] seconds before sending another request" },
                { "tpa_duplicate", "You already have a pending request to that player" },
                { "tpa_sent", "Sent a teleport request to [[b]]{0}[[/b]]" },
                { "tpa_receive", "[[b]]{0}[[/b]] wants to teleport to you&#xA;Type [[b]]/tpa accept[[/b]] to accept" },
                { "tpa_no_request", "You have no pending TPA request" },
                { "tpa_accepted", "Accepted [[b]]{0}[[/b]]'s teleport request" },
                { "tpa_delay", "Teleporting to [[b]]{0}[[/b]] in [[b]]{1}[[/b]] seconds..." },
                { "tpa_while_combat", "Teleport failed - [[b]]{0}[[/b]] is in combat" },
                { "tpa_while_combat_you", "Teleport failed - you are in combat" },
                { "tpa_while_raid", "Teleport failed - [[b]]{0}[[/b]] is raiding" },
                { "tpa_while_raid_you", "Teleport failed - you are raiding" },
                { "tpa_dead", "Teleport failed - a player is dead" },
                { "tpa_cave", "Teleport failed - [[b]]{0}[[/b]] is in a cave" },
                { "tpa_cave_you", "Teleport failed - you are in a cave" },
                { "tpa_vehicle", "Teleport failed - [[b]]{0}[[/b]] is in a vehicle" },
                { "tpa_vehicle_you", "Teleport failed - you are in a vehicle" },
                { "tpa_no_sent_request", "You have no outgoing teleport request" },
                { "tpa_canceled", "Canceled teleport request to [[b]]{0}[[/b]]" },
                { "tpa_denied", "Denied [[b]]{0}[[/b]]'s teleport request" },
                { "tpa_canceled_sender_moved", "Teleport canceled - [[b]]{0}[[/b]] moved" },
                { "tpa_canceled_you_moved", "Teleport canceled - you moved" },
                { "tpa_success", "Successfully teleported to [[b]]{0}[[/b]]" },
                { "tpa_yourself", "You cannot send a teleport request to yourself" },
                { "homes_cooldown", "Wait [[b]]{0}[[/b]] seconds before using home again" },
                { "homes_delay_warn", "Teleporting to home [[b]]{0}[[/b]] in [[b]]{1}[[/b]] seconds..." },
                { "homes_max_warn", "You have reached the maximum number of beds" },
                { "homes_bed_destroyed", "Home unavailable: bed destroyed or unclaimed. Teleportation canceled" },
                { "homes_while_driving", "You cannot teleport while driving" },
                { "homes_no_home", "No matching home found" },
                { "homes_success", "Teleported to home [[b]]{0}[[/b]]" },
                { "homes_list", "Your homes [[b]][{0}/{1}][[/b]]: " },
                { "homes_no_homes", "You do not have any claimed beds" },
                { "homes_destroy_format", "Usage: /destroyhome [name]" },
                { "homes_not_found", "No home found named [[b]]{0}[[/b]]" },
                { "homes_destroy_success", "Removed home [[b]]{0}[[/b]]" },
                { "homes_rename_format", "Usage: /renamehome [current name] [new name]" },
                { "homes_already_exists", "You already have a home named [[b]]{0}[[/b]]" },
                { "homes_rename_success", "Renamed home from [[b]]{0}[[/b]] to [[b]]{1}[[/b]]" },
                { "homes_while_raid", "You cannot teleport while raiding" },
                { "homes_while_combat", "You cannot teleport while in combat" },
                { "homes_restore_success", "[[b]]{0}[[/b]] homes have been restored" },
                { "homes_removed", "Your home [[b]]{0}[[/b]] was removed" },
                { "homes_claimed", "Claimed new home: [[b]]{0}[[/b]]" },
                { "homes_teleport_failed", "Failed to teleport to home [[b]]{0}[[/b]]" },
                { "homes_destroyed", "Your home [[b]]{0}[[/b]] was destroyed" },
                { "homes_canceled_you_moved", "Home teleport canceled because you moved" },
                { "homes_underground", "Cannot teleport to home [[b]]{0}[[/b]] because it is underground" },
                { "autosave_notify", "[[color=yellow]][[b]]Server save in progress...[[/b]][[/color]]" },
                { "warp_help", "Usage: /warp &lt;name&gt; [player]" },
                { "warps_help", "Usage: /warps [filter]" },
                { "warp_set_help", "Usage: /setwarp &lt;name&gt;" },
                { "warp_del_help", "Usage: /delwarp &lt;name&gt;" },
                { "warp_del_all_help", "Usage: /delwarpall &lt;mapname&gt;" },
                { "warp_success", "Teleported to warp [[b]]{0}[[/b]]" },
                { "warp_delay", "Warping to [[b]]{0}[[/b]] in [[b]]{1}[[/b]] seconds..." },
                { "warp_delay_nomove", "Warping to [[b]]{0}[[/b]] in [[b]]{1}[[/b]] seconds. Do not move." },
                { "warp_canceled_moved", "Warp to [[b]]{0}[[/b]] canceled because you moved" },
                { "warp_canceled_died", "Warp to [[b]]{0}[[/b]] canceled because you died" },
                { "warp_not_found", "Warp [[b]]{0}[[/b]] was not found" },
                { "warp_player_not_found", "Target player was not found" },
                { "warp_while_driving", "You cannot warp while in a vehicle" },
                { "warp_other_denied", "You are not allowed to warp other players" },
                { "warp_console_need_player", "Console must specify a target player" },
                { "warp_other_success", "Teleported [[b]]{0}[[/b]] to warp [[b]]{1}[[/b]]" },
                { "warp_other_log", "Admin {0}({1}) teleported {2} to warp {3}" },
                { "warp_set_success", "Warp has been set" },
                { "warp_set_failed", "Failed to set warp" },
                { "warp_del_success", "Warp has been removed" },
                { "warp_del_not_found", "Warp was not found" },
                { "warp_del_all_success", "Removed [[b]]{0}[[/b]] warp(s)" },
                { "warps_empty", "No warps were found" },
                { "warps_list_header", "Warps on this map: [[b]]{0}[[/b]]" },
                { "warps_list", "{0}" },
                { "warp_while_combat", "You cannot warp while in combat" },
                { "warp_while_raid", "You cannot warp while raiding" },
                { "invalid_character_name","invalid character name"},
                { "command_not_found","Command not found."},
                { "command_reloadoptions_usage", "Usage: reloadoptions <module> [module ...] | all" },
                { "command_reloadoptions_modules", "Available modules: {0}" },
                { "command_reloadoptions_unknown", "Unknown module(s): {0}" },
                { "command_reloadoptions_success", "Reloaded configuration: {0}" },
                { "command_reloadoptions_success_all", "Reloaded all configuration files." },
                { "command_reloadoptions_failed", "Failed to reload configuration. Check server log for details." }
        };


        public static XMLFileAsset<UnturnedSettings> Settings = null!;
        public static XMLFileAsset<TpaSettings> TpaSettings = null!;
        public static XMLFileAsset<HomeSettings> HomeSettings = null!;
        public static XMLFileAsset<CommandAliasSettings> CommandAliasSettings = null!;
        public static XMLFileAsset<WarpSettings> WarpSettings = null!;
        public static XMLFileAsset<AutoSaveSettings> AutoSaveSettings = null!;
        public static XMLFileAsset<TranslationList> Translation = null!;

        private GameObject? teleportServicesHost;
        private HomeRegistry? homeRegistry;
        private WarpRegistry? warpRegistry;

        public IRocketImplementationEvents ImplementationEvents { get { return Events; } }
        public static UnturnedEvents Events = null!;

        public event RocketImplementationInitialized OnRocketImplementationInitialized = null!;

        public static string Translate(string translationKey, params object[] placeholder)
        {
            return Translation.Instance.Translate(translationKey, placeholder);
        }

        public void initialize()
        {
            rocketGameObject = new GameObject("Rocket");
            DontDestroyOnLoad(rocketGameObject);

            CommandWindow.Log("Rocket Unturned v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " for Unturned v" + Provider.APP_VERSION);

            HeadlessLogFilter.Install();
            HeadlessLogFilter.TryApplyEarlyFromConfigFile();

            IPluginAdvertising pluginAdvertising = PluginAdvertising.Get();
            pluginAdvertising.PluginFrameworkName = "rocket";

            R.OnRockedInitialized += () =>
            {
                Instance.Initialize();
            };

            Provider.onServerHosted += () =>
            {
                rocketGameObject.TryAddComponent<U>();
                rocketGameObject.TryAddComponent<R>();
            };
        }

        private void Awake()
        {
            Instance = this;
            Environment.Initialize();
        }

        internal void Initialize()
        {
            try
            {
                Settings = new XMLFileAsset<UnturnedSettings>(Environment.SettingsFile);
                TpaSettings = new XMLFileAsset<TpaSettings>(Environment.TpaSettingsFile);
                HomeSettings = new XMLFileAsset<HomeSettings>(Environment.HomesSettingsFile);
                CommandAliasSettings = new XMLFileAsset<CommandAliasSettings>(Environment.CommandAliasSettingsFile);
                WarpSettings = new XMLFileAsset<WarpSettings>(Environment.WarpsSettingsFile);
                AutoSaveSettings = new XMLFileAsset<AutoSaveSettings>(Environment.AutoSaveSettingsFile);
                CommandAliasResolver.Load(CommandAliasSettings.Instance);
                HeadlessLogFilter.ApplyFromSettings(Settings.Instance.SuppressHeadlessGraphicsLogs);
                Events = gameObject.TryAddComponent<UnturnedEvents>();
                ApplyTeleportServices();
                string languageCode = LanguageCodeHelper.Normalize(Core.R.Settings.Instance.LanguageCode);
                Translation = new XMLFileAsset<TranslationList>(String.Format(Environment.TranslationFile, languageCode), new Type[] { typeof(TranslationList), typeof(TranslationListEntry) }, defaultTranslations);
                defaultTranslations.AddUnknownEntries(Translation);

                gameObject.TryAddComponent<UnturnedPermissions>();
                gameObject.TryAddComponent<UnturnedChat>();
                gameObject.TryAddComponent<UnturnedCommands>();

                gameObject.TryAddComponent<AutomaticSaveWatchdog>();

                bindDelegates();
                RCONServer.PreprocessCommand = CommandAliasResolver.ExpandCommandLine;
                StartupBanner.Subscribe();

                RocketPlugin.OnPluginLoading += (IRocketPlugin plugin, ref bool cancelLoading) =>
                {
                    try
                    {
                        plugin.TryAddComponent<PluginUnturnedPlayerComponentManager>();
                    }
                    catch (Exception ex)
                    {
                        Core.Logging.Logger.LogException(ex, "Failed to load plugin " + plugin.Name + ".");
                        cancelLoading = true;
                    }
                };

                RocketPlugin.OnPluginUnloading += (IRocketPlugin plugin) =>
                {
                    plugin.TryRemoveComponent<PluginUnturnedPlayerComponentManager>();
                };

                R.Commands.RegisterFromAssembly(Assembly.GetExecutingAssembly());

                try
                {
                    R.Plugins.OnPluginsLoaded += () =>
                    {
                        IPluginAdvertising pluginAdvertising = PluginAdvertising.Get();
                        List<IRocketPlugin> rocketPlugins = R.Plugins.GetPlugins();
                        List<string> pluginNames = new List<string>(rocketPlugins.Count);
                        foreach(IRocketPlugin plugin in rocketPlugins)
                        {
                            if(plugin != null && !string.IsNullOrEmpty(plugin.Name))
                            {
                                pluginNames.Add(plugin.Name);
                            }
                        }
                        pluginAdvertising.AddPlugins(pluginNames);
                        CommandAliasCatalog.Sync(CommandAliasSettings);
                    };

                    SteamGameServer.SetKeyValue("unturned", Provider.APP_VERSION);
                    SteamGameServer.SetKeyValue("rocket", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                }
                catch (Exception ex)
                {
                    Core.Logging.Logger.LogError("Steam can not be initialized: " + ex.Message);
                }

                OnRocketImplementationInitialized.TryInvoke();

            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex);
            }
        }

        private void bindDelegates()
        {
            CommandWindow.onCommandWindowInputted += (string text, ref bool shouldExecuteCommand) =>
            {
                if (R.Commands != null)
                {
                    string expanded = CommandAliasResolver.ExpandCommandLine(text);
                    bool executed = R.Commands.Execute(new ConsolePlayer(), expanded);
                    if (!executed && !string.IsNullOrWhiteSpace(text.Trim()))
                    {
                        string commandName = expanded.TrimStart('/').Split(' ')[0];
                        if (R.Commands.GetCommand(commandName) == null)
                        {
                            CommandWindow.Log(U.Translate("command_not_found"));
                        }
                    }
                }
                shouldExecuteCommand = false;
            };

            CommandWindow.onCommandWindowOutputted += (object text, ConsoleColor color) =>
            {
                Core.Logging.Logger.ExternalLog(text, color);
            };

            /*
            SteamChannel.onTriggerReceive += (SteamChannel channel, CSteamID steamID, byte[] packet, int offset, int size) =>
             {
                 UnturnedPlayerEvents.TriggerReceive(channel, steamID, packet, offset, size);
             };
             */

            // Replacements for Rocket usage of onTriggerSend:
            SDG.Unturned.Player.onPlayerStatIncremented += UnturnedPlayerEvents.InternalOnPlayerStatIncremented;
            PlayerClothing.OnShirtChanged_Global += UnturnedPlayerEvents.InternalOnShirtChanged;
            PlayerClothing.OnPantsChanged_Global += UnturnedPlayerEvents.InternalOnPantsChanged;
            PlayerClothing.OnHatChanged_Global += UnturnedPlayerEvents.InternalOnHatChanged;
            PlayerClothing.OnBackpackChanged_Global += UnturnedPlayerEvents.InternalOnBackpackChanged;
            PlayerClothing.OnVestChanged_Global += UnturnedPlayerEvents.InternalOnVestChanged;
            PlayerClothing.OnMaskChanged_Global += UnturnedPlayerEvents.InternalOnMaskChanged;
            PlayerClothing.OnGlassesChanged_Global += UnturnedPlayerEvents.InternalOnGlassesChanged;
            PlayerAnimator.OnGestureChanged_Global += UnturnedPlayerEvents.InternalOnGestureChanged;
            PlayerLife.OnTellHealth_Global += UnturnedPlayerEvents.InternalOnTellHealth;
            PlayerLife.OnTellFood_Global += UnturnedPlayerEvents.InternalOnTellFood;
            PlayerLife.OnTellWater_Global += UnturnedPlayerEvents.InternalOnTellWater;
            PlayerLife.OnTellVirus_Global += UnturnedPlayerEvents.InternalOnTellVirus;
            PlayerLife.OnTellBleeding_Global += UnturnedPlayerEvents.InternalOnTellBleeding;
            PlayerLife.OnTellBroken_Global += UnturnedPlayerEvents.InternalOnTellBroken;
            PlayerLife.OnRevived_Global += UnturnedPlayerEvents.InternalOnRevived;
            PlayerLife.RocketLegacyOnDeath += UnturnedPlayerEvents.InternalOnPlayerDeath;
            PlayerLife.onPlayerDied += UnturnedPlayerEvents.InternalOnPlayerDied;
            PlayerSkills.OnExperienceChanged_Global += UnturnedPlayerEvents.InternalOnExperienceChanged;
            PlayerStance.OnStanceChanged_Global += UnturnedPlayerEvents.InternalOnStanceChanged;

            ChatManager.onCheckPermissions += (SteamPlayer player, string text, ref bool shouldExecuteCommand, ref bool shouldList) =>
            {
                if (text.StartsWith("/"))
                {
                    text.Substring(1);
                    if (R.Commands != null && UnturnedPermissions.CheckPermissions(player, text))
                    {
                        string expanded = CommandAliasResolver.ExpandCommandLine(text);
                        R.Commands.Execute(UnturnedPlayer.FromSteamPlayer(player), expanded);
                    }
                    shouldList = false;
                }
                shouldExecuteCommand = false;
            };

            Provider.onCheckValidWithExplanation += (ValidateAuthTicketResponse_t callback, ref bool isValid, ref string explanation) =>
            {
                if(isValid)
                    isValid = UnturnedPermissions.CheckValid(callback);
            };
    }

        public void Reload()
        {
            ConfigOptionsReloader.ReloadAll();
        }

        internal void ApplyTeleportServices()
        {
            bool needSharedTracker = TpaSettings.Instance.Enabled || HomeSettings.Instance.Enabled || WarpSettings.Instance.Enabled;
            bool needTpa = TpaSettings.Instance.Enabled;
            bool needHomes = HomeSettings.Instance.Enabled;
            bool needWarps = WarpSettings.Instance.Enabled;

            if (needSharedTracker)
            {
                if (teleportServicesHost == null)
                {
                    teleportServicesHost = new GameObject("RocketTeleportServices");
                    DontDestroyOnLoad(teleportServicesHost);
                    teleportServicesHost.TryAddComponent<MovementCancelWatcher>();
                    teleportServicesHost.TryAddComponent<CombatRaidTracker>();
                }

                if (needTpa)
                {
                    if (TpaService.Instance == null)
                    {
                        teleportServicesHost.TryAddComponent<TpaService>();
                    }
                    else if (MovementCancelWatcher.Instance != null)
                    {
                        MovementCancelWatcher.Instance.SetMoveMaxDistance(TpaSettings.Instance.MoveMaxDistance);
                    }
                }
                else if (TpaService.Instance != null)
                {
                    Destroy(TpaService.Instance);
                }
            }
            else if (teleportServicesHost != null)
            {
                Destroy(teleportServicesHost);
                teleportServicesHost = null;
            }

            if (needHomes)
            {
                if (homeRegistry == null)
                {
                    homeRegistry = gameObject.TryAddComponent<HomeRegistry>();
                }
            }
            else if (homeRegistry != null)
            {
                Destroy(homeRegistry);
                homeRegistry = null;
            }

            if (needWarps)
            {
                if (warpRegistry == null)
                {
                    warpRegistry = gameObject.TryAddComponent<WarpRegistry>();
                }
            }
            else if (warpRegistry != null)
            {
                Destroy(warpRegistry);
                warpRegistry = null;
            }
        }

        public void shutdown()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            Provider.shutdown();
        }

        public string InstanceId
        {
            get
            {
                return Dedicator.serverID;
            }
        }
    }

}
