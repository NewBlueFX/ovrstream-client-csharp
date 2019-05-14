using System;
using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class DownloadImageCommand : NewBlueCommand
    {
        public Uri Uri { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "downloadImage");
            parent.SetAttribute("url", Uri.ToString());
        }
    }
}
