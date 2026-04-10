using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLLMChat.Helpers
{
    public static class ControlHelper
    {

        public static void SetMouseWhellRaiseToParent(UIElement Element, bool State)
        {
            if (State)
                Element.PreviewMouseWheel += Element_PreviewMouseWheel;
            else
                Element.PreviewMouseWheel -= Element_PreviewMouseWheel;
        }
        public static bool GetSetMouseWhellRaiseToParent(UIElement Element) => false;

        private static void Element_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var parent = (sender as FrameworkElement)?.Parent as UIElement;
            if (parent != null)
            {
                parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent=UIElement.MouseWheelEvent});
            }
        }
    }
}
