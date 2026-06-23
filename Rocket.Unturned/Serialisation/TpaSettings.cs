using Rocket.API;
using System.Xml.Serialization;

namespace Rocket.Unturned.Serialisation
{
    public sealed class TpaSettings : IDefaultable
    {
        [XmlElement("Enabled")]
        public bool Enabled;

        [XmlElement("MessageColor")]
        public string MessageColor = "gray";

        [XmlElement("MessageIconUrl")]
        public string MessageIconUrl = "";

        [XmlElement("TpaCooldown")]
        public double TpaCooldown = 90;

        [XmlElement("TpaDelay")]
        public double TpaDelay = 3;

        [XmlElement("TpaDuration")]
        public double TpaDuration = 90;

        [XmlElement("AllowCombat")]
        public bool AllowCombat;

        [XmlElement("CombatDuration")]
        public double CombatDuration = 20;

        [XmlElement("AllowRaid")]
        public bool AllowRaid;

        [XmlElement("RaidDuration")]
        public double RaidDuration = 30;

        [XmlElement("AllowCave")]
        public bool AllowCave;

        [XmlElement("UseUnsafeTeleport")]
        public bool UseUnsafeTeleport;

        [XmlElement("CancelOnMove")]
        public bool CancelOnMove = true;

        [XmlElement("MoveMaxDistance")]
        public float MoveMaxDistance = 0.5f;

        public void LoadDefaults()
        {
            Enabled = false;
            MessageColor = "gray";
            MessageIconUrl = "";
            TpaCooldown = 90;
            TpaDelay = 3;
            TpaDuration = 90;
            AllowCombat = false;
            CombatDuration = 20;
            AllowRaid = false;
            RaidDuration = 30;
            AllowCave = false;
            UseUnsafeTeleport = false;
            CancelOnMove = true;
            MoveMaxDistance = 0.5f;
        }
    }
}
