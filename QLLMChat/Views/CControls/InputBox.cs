using QLLMChat.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLLMChat.Views.CControls
{
    public class InputBox : Control
    {
        //public String Text {
        //    get => GetValue(TextBlock.TextProperty)?.ToString()??"";
        //    set=> SetValue(TextBlock.TextProperty, value);
        //}




        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(String), typeof(InputBox), new PropertyMetadata(null));




        public bool IsSending
        {
            get { return (bool)GetValue(IsSendingProperty); }
            set { SetValue(IsSendingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSending.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSendingProperty =
            DependencyProperty.Register(nameof(IsSending), typeof(bool), typeof(InputBox), new PropertyMetadata(null));



        public ICommand SendCommand
        {
            get { return (ICommand)GetValue(SendCommandProperty); }
            set { SetValue(SendCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SendCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SendCommandProperty =
            DependencyProperty.Register(nameof(SendCommand), typeof(ICommand), typeof(InputBox), new PropertyMetadata(null));




        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CancelCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(InputBox), new PropertyMetadata(null));



        static InputBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InputBox), new FrameworkPropertyMetadata(typeof(InputBox)));
        }
        public InputBox()
        {
        }


        public override void OnApplyTemplate()
        {
            var TEXT_INPUT = GetTemplateChild("TEXT_INPUIT") as TextBox;
            TEXT_INPUT.PreviewKeyDown += TEXT_INPUT_KeyDown;
            base.OnApplyTemplate();
        }

        private void TEXT_INPUT_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.Enter)
            {
                if (SendCommand.CanExecute(Text))
                    SendCommand.Execute(Text);
                e.Handled = true;
            }
        }
    }
}
