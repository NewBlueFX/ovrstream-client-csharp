using System.Xml;

namespace ovrstream_client_csharp
{
    /// <summary>
    /// This class represents the video settings.
    /// </summary>
    public class VideoSettings
    {
        /// <summary>
        /// The current render width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The current render height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The current frame rate.
        /// </summary>
        public int FrameRate { get; private set; }

        /// <summary>
        /// A flag indicating if the video is interlaced.
        /// </summary>
        public bool IsInterlaced { get; private set; }
        
        internal VideoSettings()
        {
        }

        internal static VideoSettings Parse(XmlElement element)
        {
            return new VideoSettings
            {
                Width = XmlConvert.ToInt32(element.GetAttribute("width")),
                Height = XmlConvert.ToInt32(element.GetAttribute("height")),
                FrameRate = XmlConvert.ToInt32(element.GetAttribute("frameRate")),
                IsInterlaced = XmlConvert.ToBoolean(element.GetAttribute("interlaced")),
            };
        }
    }
}
