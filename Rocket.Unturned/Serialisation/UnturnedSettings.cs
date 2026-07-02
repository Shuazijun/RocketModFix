using Rocket.API;
using System.Xml.Serialization;
using Rocket.Unturned.Items;
using System.Collections.Generic;
using System;

namespace Rocket.Unturned.Serialisation
{
    public class UnturnedSettings : IDefaultable
    {
        [XmlElement("CharacterNameValidation")]
        public bool CharacterNameValidation = false;

        [XmlElement("CharacterNameValidationRule")]
        public string CharacterNameValidationRule = @"([\x00-\xAA]|[\w_\ \.\+\-])+";

        public bool LogSuspiciousPlayerMovement = true;

        public bool EnableItemBlacklist;

        public bool EnableItemSpawnLimit;

        public int MaxSpawnAmount;

        public bool EnableVehicleBlacklist;

        public bool EnableUnturnedPlayerColorFromPriorityGroup;

        public void LoadDefaults()
        {
            CharacterNameValidation = true;
            CharacterNameValidationRule = @"([\x00-\xAA]|[\w_\ \.\+\-])+";
            LogSuspiciousPlayerMovement = true;
            EnableItemBlacklist = false;
            EnableItemSpawnLimit = false;
            MaxSpawnAmount = 10;
            EnableVehicleBlacklist = false;
            EnableUnturnedPlayerColorFromPriorityGroup = false;
        }
    }
}
