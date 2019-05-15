using System;
using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class OpenTitleEditCommand : NewBlueCommand
    {
        public string Title { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "openEdit");
            parent.SetAttribute("id", Title);
        }
    }
}
