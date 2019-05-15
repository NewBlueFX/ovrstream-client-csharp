using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class OpenPlayoutCommand : NewBlueCommand
    {
        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "openPlayout");
        }
    }
}
