using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Rocket.Unturned.Commands
{
    public class CommandItemList : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "itemlist";

        public string Help => "Exports registered item names and IDs to Rocket/Outputs/ItemLists.csv";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.itemlist" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 0)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
            }

            try
            {
                string outputDirectory = Path.Combine(ReadWrite.PATH, "Servers", Dedicator.serverID, "Rocket", "Outputs");
                Directory.CreateDirectory(outputDirectory);

                string outputPath = Path.Combine(outputDirectory, "ItemLists.csv");
                UnturnedItems.ItemAssetExportSnapshot snapshot = UnturnedItems.GetItemAssetExportSnapshot();
                WriteItemListCsv(outputPath, snapshot.Items);

                string relativePath = Path.Combine("Servers", Dedicator.serverID, "Rocket", "Outputs", "ItemLists.csv")
                    .Replace('\\', '/');
                string message;
                if (caller is ConsolePlayer)
                {
                    message = U.Translate("command_itemlist_success_console", snapshot.Items.Count, outputPath, snapshot.SkippedCount);
                }
                else
                {
                    message = U.Translate("command_itemlist_success_player", snapshot.Items.Count, relativePath, snapshot.SkippedCount);
                }

                UnturnedChat.Say(caller, message, Color.green);
                if (caller is not ConsolePlayer)
                {
                    Core.Logging.Logger.Log(U.Translate("command_itemlist_success_console", snapshot.Items.Count, outputPath, snapshot.SkippedCount));
                }
            }
            catch (Exception ex)
            {
                if (caller is not ConsolePlayer)
                {
                    UnturnedChat.Say(caller, U.Translate("command_itemlist_failed", ex.Message), Color.red);
                }

                Core.Logging.Logger.LogException(ex, "itemlist export failed");
            }
        }

        private static void WriteItemListCsv(string path, IReadOnlyList<ItemAsset> items)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Id,Name");

            foreach (ItemAsset item in items)
            {
                builder.Append(item.id);
                builder.Append(',');
                builder.AppendLine(EscapeCsvField(item.itemName));
            }

            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        private static string EscapeCsvField(string value)
        {
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
