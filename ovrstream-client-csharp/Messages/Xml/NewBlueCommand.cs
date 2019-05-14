using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal abstract class NewBlueCommand
    {
        public override string ToString()
        {
            XmlDocument doc = new XmlDocument();
            var rootElement = doc.CreateElement("newblue_ext");

            WriteXml(rootElement);

            return rootElement.OuterXml;
        }

        protected abstract void WriteXml(XmlElement parent);
    }
}
