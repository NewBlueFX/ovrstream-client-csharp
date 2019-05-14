using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ovrstream_client_csharp.Messages
{
    public enum PlayStatus
    {
        Off,
        Rendering,
        Paused,
    }

    public class Title
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Function { get; private set; }
        public bool IsRendering { get; private set; }
        public bool IsPlaying { get; private set; }
        public PlayStatus Status { get; private set; }
        public long PlayProgress { get; private set; }
        public long PlayCursor { get; private set; }

        public Variable[] Variables { get; private set; }

        // TODO:
        // public string Input { get; private set; }

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

    public class Variable
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Type { get; private set; }
        public bool IsDynamic { get; private set; }
        public string Group { get; private set; }
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
