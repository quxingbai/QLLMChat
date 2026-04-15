using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

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
                parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent });
            }
        }


        public static readonly DependencyProperty MouseHoverChangeOpacityTargetProperty =
            DependencyProperty.RegisterAttached("MouseHoverChangeOpacityTarget", typeof(UIElement), typeof(ControlHelper), new PropertyMetadata(null, (prop, change) =>
            {
                if (prop is not UIElement) throw new("只能附加到UIElement上");

                var Element = prop as UIElement;
                var Target = change.NewValue as UIElement;
                if (Target != null)
                {
                    Element.MouseEnter += Element_MouseEnter;
                    Element.MouseLeave += Element_MouseLeave;
                }
                else
                {
                    Element.MouseEnter -= Element_MouseEnter;
                    Element.MouseLeave -= Element_MouseLeave;
                }

                void Element_MouseEnter(object sender, MouseEventArgs e)
                {
                    if (Target == null) throw new("Target不能为null");
                    if (sender is UIElement element)
                    {
                        var anima = AnimationHelp.CreateDoubleAnimation(null, 1);
                        Target.BeginAnimation(UIElement.OpacityProperty, anima);
                    }
                }

                void Element_MouseLeave(object sender, MouseEventArgs e)
                {
                    if (Target == null) throw new("Target不能为null");
                    if (sender is UIElement element)
                    {
                        var anima = AnimationHelp.CreateDoubleAnimation(null, 0);
                        anima.BeginTime = TimeSpan.FromSeconds(1.5);
                        Target.BeginAnimation(UIElement.OpacityProperty, anima);
                    }
                }
            }));


        public static UIElement? GetMouseHoverChangeOpacityTarget(UIElement Element)
        {
            return (UIElement?)Element.GetValue(MouseHoverChangeOpacityTargetProperty);
        }
        public static void SetMouseHoverChangeOpacityTarget(UIElement Element, UIElement? Target)
        {
            Element.SetValue(MouseHoverChangeOpacityTargetProperty, Target);

        }

        public static readonly DependencyProperty ObjectDictionaryProperty = DependencyProperty.RegisterAttached("ObjectDictionary", typeof(Dictionary<string, object>), typeof(ControlHelper), new PropertyMetadata(null));
        public static void SetObjectDictionary(UIElement Element, Dictionary<string, object> Value)
        {
            Element.SetValue(ObjectDictionaryProperty, Value);
        }
        public static Dictionary<string, object>? GetObjectDictionary(UIElement Element)
        {
            return (Dictionary<string, object>?)Element.GetValue(ObjectDictionaryProperty);
        }
        public static void SetObjectToSelfDictionary(this UIElement Element, string Key, object Value)
        {
            var dict = GetObjectDictionary(Element);
            if (dict == null)
            {
                dict = new Dictionary<string, object>();
                SetObjectDictionary(Element, dict);
            }
            if(dict.ContainsKey(Key)) dict.Remove(Key);
            dict[Key] = Value;
        }
        public static object? GetObjectInSelfDictionary(this UIElement Element,string Key)
        {
            var dict = GetObjectDictionary(Element);
            if (dict == null)
            {
                return null;
            }
            return dict.ContainsKey(Key) ? dict[Key] : null;
        }
    }
}
