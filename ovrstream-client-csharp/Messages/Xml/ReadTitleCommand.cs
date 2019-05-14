using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class ReadTitleCommand : NewBlueCommand
    {
        public int Channel { get; set; }

        public string Title { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "readTitle");
            parent.SetAttribute("channel", Channel.ToString());
            parent.SetAttribute("title", Title);
        }
    }
}
