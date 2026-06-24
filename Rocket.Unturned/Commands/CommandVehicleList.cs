using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Vehicles;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Rocket.Unturned.Commands
{
    public class CommandVehicleList : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "vehiclelist";

        public string Help => "Exports registered vehicle names and IDs to Rocket/Outputs/VehicleLists.csv";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.vehiclelist" };

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

                string outputPath = Path.Combine(outputDirectory, "VehicleLists.csv");
                UnturnedVehicles.VehicleAssetExportSnapshot snapshot = UnturnedVehicles.GetVehicleAssetExportSnapshot();
                WriteVehicleListCsv(outputPath, snapshot.Vehicles);

                string relativePath = Path.Combine("Servers", Dedicator.serverID, "Rocket", "Outputs", "VehicleLists.csv")
                    .Replace('\\', '/');
                string message;
                if (caller is ConsolePlayer)
                {
                    message = U.Translate("command_vehiclelist_success_console", snapshot.Vehicles.Count, outputPath, snapshot.SkippedCount);
                }
                else
                {
                    message = U.Translate("command_vehiclelist_success_player", snapshot.Vehicles.Count, relativePath, snapshot.SkippedCount);
                }

                UnturnedChat.Say(caller, message, Color.green);
                if (caller is not ConsolePlayer)
                {
                    Core.Logging.Logger.Log(U.Translate("command_vehiclelist_success_console", snapshot.Vehicles.Count, outputPath, snapshot.SkippedCount));
                }
            }
            catch (Exception ex)
            {
                if (caller is not ConsolePlayer)
                {
                    UnturnedChat.Say(caller, U.Translate("command_vehiclelist_failed", ex.Message), Color.red);
                }

                Core.Logging.Logger.LogException(ex, "vehiclelist export failed");
            }
        }

        private static void WriteVehicleListCsv(string path, IReadOnlyList<VehicleAsset> vehicles)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Id,Name");

            foreach (VehicleAsset vehicle in vehicles)
            {
                builder.Append(vehicle.id);
                builder.Append(',');
                builder.AppendLine(EscapeCsvField(vehicle.vehicleName));
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
