using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class GetTitleControlInfoCommand : NewBlueCommand
    {
        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "getTitleControlInfo");
        }
    }
}
