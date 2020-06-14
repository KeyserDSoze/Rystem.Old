using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Web
{
    public class SummerNote
    {
        public int Height { get; set; }
        public bool HasStyle { get; set; }
        public bool HasFont { get; set; }
        public bool HasParagraph { get; set; }
        public bool HasInsertion { get; set; }
        public bool HasMiscellaneous { get; set; }
        public static SummerNote Default => new SummerNote()
        {
            Height = 500,
            HasFont = true,
            HasInsertion = true,
            HasMiscellaneous = true,
            HasParagraph = true,
            HasStyle = true,
        };
        public static SummerNote DefaultWithHeight(int height)
            => new SummerNote()
            {
                Height = height,
                HasFont = true,
                HasInsertion = true,
                HasMiscellaneous = true,
                HasParagraph = true,
                HasStyle = true,
            };
    }
}