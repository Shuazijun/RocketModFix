using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rocket.Unturned.Serialisation
{
    public sealed class VipPermissionEntry
    {
        [XmlAttribute("PermissionTag")]
        public string PermissionTag = "";

        [XmlAttribute("Value")]
        public int Value;
    }

    public sealed class HomeSettings : IDefaultable
    {
        [XmlElement("Enabled")]
        public bool Enabled;

        [XmlElement("MessageColor")]
        public string MessageColor = "yellow";

        [XmlElement("MessageIconUrl")]
        public string MessageIconUrl = "https://i.imgur.com/9TF5aB1.png";

        [XmlElement("DefaultHomeCooldown")]
        public int DefaultHomeCooldown = 20;

        [XmlElement("DefaultHomeDelay")]
        public int DefaultHomeDelay = 10;

        [XmlElement("DefaultMaxHomes")]
        public int DefaultMaxHomes = 2;

        [XmlElement("TeleportHeight")]
        public float TeleportHeight = 0.5f;

        [XmlElement("CancelOnMove")]
        public bool CancelOnMove = true;

        [XmlElement("MoveMaxDistance")]
        public float MoveMaxDistance = 0.5f;

        [XmlElement("BlockUnderground")]
        public bool BlockUnderground;

        [XmlArray("VipCooldowns")]
        [XmlArrayItem("VIPPermission")]
        public List<VipPermissionEntry> VipCooldowns = new List<VipPermissionEntry>();

        [XmlArray("VipDelays")]
        [XmlArrayItem("VIPPermission")]
        public List<VipPermissionEntry> VipDelays = new List<VipPermissionEntry>();

        [XmlArray("VipMaxHomes")]
        [XmlArrayItem("VIPPermission")]
        public List<VipPermissionEntry> VipMaxHomes = new List<VipPermissionEntry>();

        public void LoadDefaults()
        {
            Enabled = false;
            MessageColor = "yellow";
            MessageIconUrl = "https://i.imgur.com/9TF5aB1.png";
            DefaultHomeCooldown = 20;
            DefaultHomeDelay = 10;
            DefaultMaxHomes = 2;
            TeleportHeight = 0.5f;
            CancelOnMove = true;
            MoveMaxDistance = 0.5f;
            BlockUnderground = false;
            VipCooldowns = new List<VipPermissionEntry>
            {
                new VipPermissionEntry { PermissionTag = "rocket.homes.vip", Value = 10 },
                new VipPermissionEntry { PermissionTag = "rocket.homes.star", Value = 5 }
            };
            VipDelays = new List<VipPermissionEntry>
            {
                new VipPermissionEntry { PermissionTag = "rocket.homes.vip", Value = 5 },
                new VipPermissionEntry { PermissionTag = "rocket.homes.star", Value = 3 }
            };
            VipMaxHomes = new List<VipPermissionEntry>
            {
                new VipPermissionEntry { PermissionTag = "rocket.homes.vip", Value = 3 },
                new VipPermissionEntry { PermissionTag = "rocket.homes.star", Value = 4 }
            };
        }
    }
}
