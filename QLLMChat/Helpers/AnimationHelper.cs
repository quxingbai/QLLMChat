using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace QLLMChat.Helpers
{
    public class AnimationHelp
    {
        public static DoubleAnimation CreateDoubleAnimation(double? From=null, double? To = null,Duration? AnimaDurtion=null, Action? Executed = null)
        {
            DoubleAnimation Anima = new();
            if (From != null) Anima.From = From.Value;
            if (To != null) Anima.To= To.Value;
            if (AnimaDurtion != null) Anima.Duration = AnimaDurtion.Value; else Anima.Duration = new Duration(TimeSpan.Parse("0:0:0.25"));
            if(Executed != null)
            {
                Anima.Completed += (s, e) => Executed();
            }
            return Anima;
        }
    }
}
