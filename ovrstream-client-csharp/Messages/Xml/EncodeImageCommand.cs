using System;
using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class EncodeImageCommand : NewBlueCommand
    {
        public string Path { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "encodeImage");
            parent.SetAttribute("file", Path);
        }
    }
}
