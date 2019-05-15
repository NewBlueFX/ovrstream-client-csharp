using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ovrstream_client_csharp.Messages
{
    public class VideoSettings
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FrameRate { get; private set; }
        public bool IsInterlaced { get; private set; }
        
        internal VideoSettings()
        {
        }

        //<newblue_ext command="getCurrentVideoSettings" reply="getCurrentVideoSettings" width="1920" height="1080" frameRate="30" interlaced="0" success="1" />
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
