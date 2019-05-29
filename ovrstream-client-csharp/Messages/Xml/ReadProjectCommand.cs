using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class ReadProjectCommand : NewBlueCommand
    {
        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "readProject");
        }
    }
}
