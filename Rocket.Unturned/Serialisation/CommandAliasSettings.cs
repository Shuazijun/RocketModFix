using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rocket.Unturned.Serialisation
{
    public sealed class CommandAliasAliasEntry
    {
        [XmlAttribute("Target")]
        public string Target = "";

        [XmlAttribute("ExpandsTo")]
        public string ExpandsTo = "";

        [XmlText]
        public string Value = "";
    }

    public sealed class CommandAliasCommandEntry
    {
        [XmlAttribute("Name")]
        public string Name = "";

        [XmlAttribute("Class")]
        public string Class = "";

        [XmlAttribute("Syntax")]
        public string Syntax = "";

        [XmlAttribute("Permissions")]
        public string Permissions = "";

        [XmlAttribute("AllowedCaller")]
        public string AllowedCaller = "";

        [XmlArray("Aliases")]
        [XmlArrayItem("Alias")]
        public List<CommandAliasAliasEntry> Aliases = new List<CommandAliasAliasEntry>();
    }

    public sealed class CommandAliasSettings : IDefaultable
    {
        [XmlElement("Enabled")]
        public bool Enabled = true;

        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        public List<CommandAliasCommandEntry> Commands = new List<CommandAliasCommandEntry>();

        public void LoadDefaults()
        {
            Enabled = true;
            Commands = new List<CommandAliasCommandEntry>();
        }
    }
}
