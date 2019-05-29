using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class MacroCommand : NewBlueCommand
    {
        public string Event { get; set; }

        public string Title { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "macro");
            parent.SetAttribute("event", Event);
            parent.SetAttribute("title", Title);
        }
    }
}
