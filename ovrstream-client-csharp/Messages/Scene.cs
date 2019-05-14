using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ovrstream_client_csharp.Messages
{
    public class Scene
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string IconPath { get; private set; }
        public string Format { get; private set; }
        public bool IsCustom { get; private set; }

        internal Scene()
        {

        }

        internal static Scene Parse(XmlElement element)
        {
            return new Scene
            {
                Name = element.GetAttribute("name"),
                Path = element.GetAttribute("path"),
                IconPath = element.GetAttribute("iconPath"),
                Format = element.GetAttribute("format"),
                IsCustom = XmlConvert.ToBoolean(element.GetAttribute("custom")),
            };
        }
    }
}
