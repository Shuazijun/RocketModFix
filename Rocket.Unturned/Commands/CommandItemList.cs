using Rocket.API;
using Rocket.Unturned.Chat;using Rocket.Unturned.Items;
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
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public string Name
        {
            get { return "itemlist"; }
        }

        public string Help
        {
            get { return "Exports registered item names and IDs to Rocket/Outputs/ItemLists.csv"; }
        }

        public string Syntax
        {
            get { return ""; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.itemlist" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 0)
            {
                UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            try
            {
                string outputDirectory = Path.Combine(ReadWrite.PATH, "Servers", Dedicator.serverID, "Rocket", "Outputs");
                Directory.CreateDirectory(outputDirectory);

                string outputPath = Path.Combine(outputDirectory, "ItemLists.csv");
                IReadOnlyList<ItemAsset> items = UnturnedItems.GetRegisteredItemAssets();
                WriteItemListCsv(outputPath, items);

                string relativePath = Path.Combine("Servers", Dedicator.serverID, "Rocket", "Outputs", "ItemLists.csv")
                    .Replace('\\', '/');
                string message;
                if (caller is ConsolePlayer)
                {
                    message = U.Translate("command_itemlist_success_console", items.Count, outputPath);
                }
                else
                {
                    message = U.Translate("command_itemlist_success_player", items.Count, relativePath);
                }

                UnturnedChat.Say(caller, message, Color.green);
                Core.Logging.Logger.Log(U.Translate("command_itemlist_success_console", items.Count, outputPath));
            }
            catch (Exception ex)
            {
                string message = U.Translate("command_itemlist_failed", ex.Message);
                UnturnedChat.Say(caller, message, Color.red);
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
                builder.AppendLine(EscapeCsvField(item.itemName ?? ""));
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
