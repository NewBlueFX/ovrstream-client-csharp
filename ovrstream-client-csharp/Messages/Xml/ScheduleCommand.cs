using System.Xml;

namespace ovrstream_client_csharp.Messages.Xml
{
    internal class ScheduleCommand : NewBlueCommand
    {
        public string Action { get; set; }

        public string Id { get; set; }

        public string Queue { get; set; }

        public XmlVariable[] Data { get; set; } = new XmlVariable[0];

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "schedule");
            parent.SetAttribute("action", Action);
            parent.SetAttribute("id", Id);
            parent.SetAttribute("queue", Queue);

            var data = parent.OwnerDocument.CreateElement("data");
            parent.AppendChild(data);

            foreach (var variable in Data)
            {
                var variableElement = parent.OwnerDocument.CreateElement("variable");
                data.AppendChild(variableElement);

                variableElement.SetAttribute("name", variable.Name);
                variableElement.SetAttribute("value", variable.Value);
            }
        }
    }

    internal class XmlVariable
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
