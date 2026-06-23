using Rocket.Unturned.Chat;
using Rocket.Unturned.Serialisation;
using Rocket.Unturned.Teleport;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Rocket.Unturned.Utils
{
    // Player notification inspired by educatalan02/AutoSaveKingModding (MIT).
    internal class AutomaticSaveWatchdog : MonoBehaviour
    {
        private const int MinimumInterval = 30;

        public static AutomaticSaveWatchdog Instance = null!;

        private DateTime? nextSaveTime;
        private int interval = MinimumInterval;
        private bool saveEnabled;
        private bool notifyPlayers;
        private string messageColor = "yellow";
        private string messageIconUrl = "";

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            checkTimer();
        }

        private void Start()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            AutoSaveSettings settings = U.AutoSaveSettings.Instance;
            saveEnabled = settings.Enabled;
            notifyPlayers = settings.NotifyPlayers;
            messageColor = settings.MessageColor;
            messageIconUrl = settings.MessageIconUrl ?? "";
            int minimumInterval = settings.MinInterval < 1 ? MinimumInterval : settings.MinInterval;
            nextSaveTime = null;

            if (!saveEnabled)
            {
                return;
            }

            interval = settings.Interval < minimumInterval ? minimumInterval : settings.Interval;
            if (settings.Interval < minimumInterval)
            {
                Core.Logging.Logger.LogError($"AutoSave interval must be at least {minimumInterval} seconds, changed to {minimumInterval} seconds");
            }

            Core.Logging.Logger.Log($"This server will automatically save every {interval} seconds");
            restartTimer();
        }

        private void restartTimer()
        {
            nextSaveTime = DateTime.Now.AddSeconds(interval);
        }

        private void checkTimer()
        {
            try
            {
                if (!saveEnabled || nextSaveTime == null)
                {
                    return;
                }

                if (nextSaveTime.Value < DateTime.Now)
                {
                    NotifyPlayers();
                    Core.Logging.Logger.Log("Saving server");
                    restartTimer();
                    SaveManager.save();
                }
            }
            catch (Exception er)
            {
                Core.Logging.Logger.LogException(er);
            }
        }

        private void NotifyPlayers()
        {
            if (!notifyPlayers)
            {
                return;
            }

            string message = TeleportMessageHelper.FormatMessage(U.Translate("autosave_notify"));
            Color color = UnturnedChat.GetColorFromName(messageColor, Color.yellow);
            ChatManager.serverSendMessage(message, color, null, null, EChatMode.GLOBAL, messageIconUrl, true);
        }
    }
}
