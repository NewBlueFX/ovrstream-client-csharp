using System.Xml;

namespace ovrstream_client_csharp
{
    /// <summary>
    /// This class represents one variable in an OvrStream title.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The current value of the variable.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// The type of variable.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// A flag to indicate if this variable is dynamic.
        /// </summary>
        public bool IsDynamic { get; private set; }

        /// <summary>
        /// The variable's group.
        /// </summary>
        public string Group { get; private set; }

        /// <summary>
        /// TODO: Get more info on this property.
        /// </summary>
        public string Field { get; private set; }

        internal Variable()
        {
        }

        internal static Variable Parse(XmlElement element)
        {
            return new Variable
            {
                Name = element.GetAttribute("variable"),
                Value = element.GetAttribute("values"),
                Type = element.GetAttribute("type"),
                IsDynamic = XmlConvert.ToBoolean(element.GetAttribute("dynamic")),
                Group = element.HasAttribute("group") ? element.GetAttribute("group") : string.Empty,
                Field = element.HasAttribute("field") ? element.GetAttribute("field") : string.Empty,
            };
        }
    }
}
