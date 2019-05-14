using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class OpenSceneCommand : NewBlueCommand
    {
        public string File { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "loadProject");
            parent.SetAttribute("file", File);
        }
    }
}
