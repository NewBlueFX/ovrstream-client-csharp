using System.Xml;

namespace ovrstream_client_csharp
{
    /// <summary>
    /// This class represents a scene within OvrStream.
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// The name of the scene.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The local machine path to the scene file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The local machine path to the scene icon.
        /// </summary>
        public string IconPath { get; private set; }

        /// <summary>
        /// The image format of the icon.
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// A flag to indicate if this is a custom scene.
        /// </summary>
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
