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

        internal Variable()
        {
        }

        internal static Variable Parse(XmlElement element)
        {
            return new Variable
            {
                Name = element.GetAttribute("name"),
                Value = element.GetAttribute("value"),
            };
        }
    }
}
