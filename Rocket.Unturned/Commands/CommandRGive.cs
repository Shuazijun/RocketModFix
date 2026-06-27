using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandRGive : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "rgive";

        public string Help => "Give items without server cheats (same syntax as vanilla /give)";

        public string Syntax => "<Player/>Item[/Amount]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.rgive" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
            }

            string[] components = Parser.getComponentsFromSerial(string.Join("/", command), '/');
            if (components.Length < 1 || components.Length > 3)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
            }

            bool targetIsExecutor = false;
            SteamPlayer? steamPlayer = null;

            if (!PlayerTool.tryGetSteamPlayer(components[0], out steamPlayer))
            {
                CSteamID executorId = CSteamID.Nil;
                if (caller is UnturnedPlayer executor)
                {
                    executorId = executor.CSteamID;
                }

                steamPlayer = PlayerTool.getSteamPlayer(executorId);
                if (steamPlayer == null)
                {
                    UnturnedChat.Say(caller, U.Translate("command_rgive_player_not_found", components[0]), UnityEngine.Color.red);
                    return;
                }

                targetIsExecutor = true;
            }

            uint amount = 1;
            if (targetIsExecutor)
            {
                if (components.Length > 1 && !uint.TryParse(components[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out amount))
                {
                    caller.ThrowWrongUsage(this, U.Translate("command_rgive_invalid_amount", components[1]));
                }
            }
            else if (components.Length > 2 && !uint.TryParse(components[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out amount))
            {
                caller.ThrowWrongUsage(this, U.Translate("command_rgive_invalid_amount", components[2]));
            }

            if (amount == 0 || amount > byte.MaxValue)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_rgive_invalid_amount", amount.ToString(CultureInfo.InvariantCulture)));
            }

            string itemToken = components[targetIsExecutor ? 0 : 1];
            UnturnedPlayer target = UnturnedPlayer.FromPlayer(steamPlayer.player);
            Asset? asset = ResolveAsset(itemToken);

            if (asset == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_rgive_item_not_found", itemToken), UnityEngine.Color.red);
                return;
            }

            if (!TryGiveAsset(caller, target, asset, (byte)amount, out string? failureMessage))
            {
                if (!string.IsNullOrEmpty(failureMessage))
                {
                    UnturnedChat.Say(caller, failureMessage!, UnityEngine.Color.red);
                }
            }
        }

        private static bool TryGiveAsset(IRocketPlayer caller, UnturnedPlayer target, Asset asset, byte amount, out string? failureMessage)
        {
            failureMessage = null;

            if (asset is ItemAsset itemAsset)
            {
                return TryGiveItem(caller, target, itemAsset, amount, out failureMessage);
            }

            if (asset is ItemCurrencyAsset currencyAsset)
            {
                currencyAsset.grantValue(target.Player, amount);
                Logger.Log(U.Translate("command_rgive_currency_console", caller.DisplayName, target.DisplayName, asset.name, amount));
                NotifyGiveSuccess(caller, target, asset.name, amount, asset.id, isCurrency: true);
                return true;
            }

            failureMessage = U.Translate("command_rgive_item_not_found", asset.name);
            return false;
        }

        private static bool TryGiveItem(IRocketPlayer caller, UnturnedPlayer target, ItemAsset itemAsset, byte amount, out string? failureMessage)
        {
            failureMessage = null;
            ushort itemId = itemAsset.id;
            string itemName = itemAsset.itemName;

            if (U.Settings.Instance.EnableItemBlacklist && !target.HasPermission("itemblacklist.bypass"))
            {
                if (target.HasPermission("item." + itemId))
                {
                    failureMessage = U.Translate("command_i_blacklisted");
                    return false;
                }
            }

            if (U.Settings.Instance.EnableItemSpawnLimit && !target.HasPermission("itemspawnlimit.bypass"))
            {
                if (amount > U.Settings.Instance.MaxSpawnAmount)
                {
                    failureMessage = U.Translate("command_i_too_much", U.Settings.Instance.MaxSpawnAmount);
                    return false;
                }
            }

            if (!target.GiveItem(itemId, amount))
            {
                failureMessage = U.Translate("command_rgive_failed", target.DisplayName, amount, itemName, itemId);
                return false;
            }

            Logger.Log(U.Translate("command_rgive_giving_console", caller.DisplayName, target.DisplayName, itemId, amount));
            NotifyGiveSuccess(caller, target, itemName, amount, itemId, isCurrency: false);
            return true;
        }

        private static void NotifyGiveSuccess(IRocketPlayer caller, UnturnedPlayer target, string assetName, byte amount, ushort assetId, bool isCurrency)
        {
            if (caller is UnturnedPlayer callerPlayer && callerPlayer.CSteamID == target.CSteamID)
            {
                UnturnedChat.Say(caller, isCurrency
                    ? U.Translate("command_rgive_currency_self", amount, assetName, assetId)
                    : U.Translate("command_rgive_giving_self", amount, assetName, assetId));
                return;
            }

            UnturnedChat.Say(caller, isCurrency
                ? U.Translate("command_rgive_currency_executor", target.DisplayName, amount, assetName, assetId)
                : U.Translate("command_rgive_giving_executor", target.DisplayName, amount, assetName, assetId));

            UnturnedChat.Say(target, isCurrency
                ? U.Translate("command_rgive_currency_target", amount, assetName, assetId)
                : U.Translate("command_rgive_giving_target", amount, assetName, assetId));
        }

        private static Asset? ResolveAsset(string input)
        {
            input = input.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (Guid.TryParse(input, out Guid guid))
            {
                return Assets.find(guid);
            }

            if (ushort.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort itemId))
            {
                return Assets.find(EAssetType.ITEM, itemId);
            }

            List<ItemAsset> assets = new List<ItemAsset>();
            Assets.find(assets);

            foreach (ItemAsset item in assets)
            {
                if (string.Equals(input, item.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item;
                }
            }

            foreach (ItemAsset item in assets)
            {
                if (string.Equals(input, item.itemName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return item;
                }
            }

            foreach (ItemAsset item in assets)
            {
                if (item.name != null && item.name.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    return item;
                }
            }

            foreach (ItemAsset item in assets)
            {
                if (item.itemName != null && item.itemName.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
