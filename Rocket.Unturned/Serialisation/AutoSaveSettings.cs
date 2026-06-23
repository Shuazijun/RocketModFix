using Rocket.API;
using System.Xml.Serialization;

namespace Rocket.Unturned.Serialisation
{
    public sealed class AutoSaveSettings : IDefaultable
    {
        [XmlElement("Enabled")]
        public bool Enabled = true;

        [XmlElement("Interval")]
        public int Interval = 1800;

        [XmlElement("NotifyPlayers")]
        public bool NotifyPlayers;

        [XmlElement("MessageColor")]
        public string MessageColor = "yellow";

        [XmlElement("MessageIconUrl")]
        public string MessageIconUrl = "";

        [XmlElement("MinInterval")]
        public int MinInterval = 30;

        public void LoadDefaults()
        {
            Enabled = true;
            Interval = 1800;
            NotifyPlayers = false;
            MessageColor = "yellow";
            MessageIconUrl = "";
            MinInterval = 30;
        }
    }
}
