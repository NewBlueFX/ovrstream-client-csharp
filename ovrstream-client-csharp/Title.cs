using System;
using System.Collections.Generic;
using System.Xml;

namespace ovrstream_client_csharp
{
    /// <summary>
    /// This class represents a title within an OvrStream <see cref="Scene"/>
    /// </summary>
    public class Title
    {
        /// <summary>
        /// The unique identifier of the title.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The name of the title.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The function of the title.
        /// TODO: Get more details on this property.
        /// </summary>
        public string Function { get; private set; }

        /// <summary>
        /// A flag to indicate if this title is currently rendering.
        /// </summary>
        public bool IsRendering { get; private set; }

        /// <summary>
        /// A flag to indicate of this title is currently playing.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// The current play status of this title.
        /// </summary>
        public PlayStatus Status { get; private set; }

        /// <summary>
        /// The progress of the this title.
        /// TODO: Get more details on this property.
        /// </summary>
        public long PlayProgress { get; private set; }

        /// <summary>
        /// The cursor of the this title.
        /// TODO: Get more details on this property.
        /// </summary>
        public long PlayCursor { get; private set; }

        /// <summary>
        /// This is an array of all the available variables for this title.
        /// </summary>
        public Variable[] Variables { get; private set; }

        /// <summary>
        /// The current input for this title.
        /// </summary>
        public string Input { get; private set; }

        internal Title()
        {
        }

        internal static Title Parse(XmlElement element)
        {
            var title = new Title
            {
                Id = element.GetAttribute("id"),
                Name = element.GetAttribute("name"),
                Function = element.GetAttribute("function"),
                IsRendering = XmlConvert.ToBoolean(element.GetAttribute("renderProgress")),
                IsPlaying = XmlConvert.ToBoolean(element.GetAttribute("playProgress")),
                Status = (PlayStatus)Enum.Parse(typeof(PlayStatus), element.GetAttribute("playStatus"), true),
                PlayProgress = XmlConvert.ToInt64(element.GetAttribute("playProgress")),
                PlayCursor = XmlConvert.ToInt64(element.GetAttribute("playCursor")),
                Input = element.GetAttribute("input"),
            };

            List<Variable> variables = new List<Variable>();
            foreach (XmlElement variableElement in element.SelectNodes("variable"))
            {
                variables.Add(Variable.Parse(variableElement));
            }

            title.Variables = variables.ToArray();

            return title;
        }
    }
}
