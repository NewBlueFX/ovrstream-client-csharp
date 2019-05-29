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
        /// </summary>
        public string Function { get; private set; }

        /// <summary>
        /// This is an array of all the available variables for this title.
        /// </summary>
        public Variable[] Variables { get; private set; }

        /// <summary>
        /// Indicates if the title's input is enabled.
        /// </summary>
        public bool IsInputActive { get; private set; }

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
                // PlayCursor = XmlConvert.ToInt64(element.GetAttribute("playCursor")),
                IsInputActive = XmlConvert.ToBoolean(element.GetAttribute("inputActive")),
                Input = element.GetAttribute("input"),
            };

            List<Variable> variables = new List<Variable>();
            foreach (XmlElement variableElement in element.SelectNodes("variables/variable"))
            {
                variables.Add(Variable.Parse(variableElement));
            }

            title.Variables = variables.ToArray();

            return title;
        }
    }
}
