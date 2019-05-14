using System;
using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class GetTitleIconCommand : NewBlueCommand
    {
        public string Path { get; set; }

        public string Title { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "getTitleIcon");
            parent.SetAttribute("title", Title);
            parent.SetAttribute("width", Width.ToString());
            parent.SetAttribute("height", Height.ToString());
        }
    }
}
