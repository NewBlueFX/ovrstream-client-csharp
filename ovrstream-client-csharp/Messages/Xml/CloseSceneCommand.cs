using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class CloseSceneCommand : NewBlueCommand
    {
        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "closeProject");
            parent.SetAttribute("saveProject", "1");
            parent.SetAttribute("closeProject", "1");
        }
    }
}
