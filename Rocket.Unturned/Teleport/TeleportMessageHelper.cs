using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using SDG.Unturned;
using UnityEngine;

namespace Rocket.Unturned.Teleport
{
    internal static class TeleportMessageHelper
    {
        public static void SendTpa(IRocketPlayer? player, string translationKey, params object[] placeholder)
        {
            Send(player, U.TpaSettings.Instance, translationKey, placeholder);
        }

        public static void SendHomes(IRocketPlayer? player, string translationKey, params object[] placeholder)
        {
            Send(player, U.HomeSettings.Instance, translationKey, placeholder);
        }

        public static void SendHomesRaw(IRocketPlayer? player, string message)
        {
            SendRaw(player, U.HomeSettings.Instance.MessageColor, U.HomeSettings.Instance.MessageIconUrl, message);
        }

        public static void SendWarps(IRocketPlayer? player, string translationKey, params object[] placeholder)
        {
            Send(player, U.WarpSettings.Instance.MessageColor, U.WarpSettings.Instance.MessageIconUrl, translationKey, placeholder);
        }

        private static void Send(IRocketPlayer? player, TpaSettings tpaSettings, string translationKey, params object[] placeholder)
        {
            Send(player, tpaSettings.MessageColor, tpaSettings.MessageIconUrl, translationKey, placeholder);
        }

        private static void Send(IRocketPlayer? player, HomeSettings homeSettings, string translationKey, params object[] placeholder)
        {
            Send(player, homeSettings.MessageColor, homeSettings.MessageIconUrl, translationKey, placeholder);
        }

        private static void Send(IRocketPlayer? player, string messageColorName, string iconUrl, string translationKey, params object[] placeholder)
        {
            SendRaw(player, messageColorName, iconUrl, FormatMessage(U.Translate(translationKey, placeholder)));
        }

        private static void SendRaw(IRocketPlayer? player, string messageColorName, string iconUrl, string message)
        {
            string formatted = FormatMessage(message);

            if (player == null || player is ConsolePlayer)
            {
                Core.Logging.Logger.Log(StripRichText(formatted));
                return;
            }

            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
            if (unturnedPlayer.Player == null)
            {
                return;
            }

            Color color = UnturnedChat.GetColorFromName(messageColorName, Color.green);
            ChatManager.serverSendMessage(formatted, color, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, iconUrl, true);
        }

        internal static string FormatMessage(string message)
        {
            return message.Replace("[[", "<").Replace("]]", ">");
        }

        private static string StripRichText(string message)
        {
            return message.Replace("<", "").Replace(">", "");
        }
    }
}
