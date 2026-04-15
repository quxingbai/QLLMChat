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
    public class CustomRenderCard : ContentControl
    {



        public ICommand COMMAND_ViewChange
        {
            get { return (ICommand)GetValue(COMMAND_ViewChangeProperty); }
            set { SetValue(COMMAND_ViewChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for COMMAND_ViewChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty COMMAND_ViewChangeProperty =
            DependencyProperty.Register(nameof(COMMAND_ViewChange), typeof(ICommand), typeof(CustomRenderCard), new PropertyMetadata(null));



        public ICommand COMMAND_Copy
        {
            get { return (ICommand)GetValue(COMMAND_CopyProperty); }
            set { SetValue(COMMAND_CopyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for COMMAND_Copy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty COMMAND_CopyProperty =
            DependencyProperty.Register(nameof(COMMAND_Copy), typeof(ICommand), typeof(CustomRenderCard), new PropertyMetadata(null));



        public String CodeLanguage
        {
            get { return (String)GetValue(CodeLanguageProperty); }
            set { SetValue(CodeLanguageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CodeLanguage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeLanguageProperty =
            DependencyProperty.Register(nameof(CodeLanguage), typeof(String), typeof(CustomRenderCard), new PropertyMetadata(""));



        public String SourceCode
        {
            get { return (String)GetValue(SourceCodeProperty); }
            set { SetValue(SourceCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceCodeProperty =
            DependencyProperty.Register(nameof(SourceCode), typeof(String), typeof(CustomRenderCard), new PropertyMetadata(""));




        public bool IsShowSourceCode
        {
            get { return (bool)GetValue(IsShowSourceCodeProperty); }
            set { SetValue(IsShowSourceCodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShowSourceCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowSourceCodeProperty =
            DependencyProperty.Register(nameof(IsShowSourceCode), typeof(bool), typeof(CustomRenderCard), new PropertyMetadata(false));



        static CustomRenderCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomRenderCard), new FrameworkPropertyMetadata(typeof(CustomRenderCard)));
        }

        //private String ViewStateIsRender = "CodeRender";//Code Render
        public CustomRenderCard()
        {
            COMMAND_Copy = new ActionCommand(arg =>
            {
                _ = CopyMethod();
            });
            COMMAND_ViewChange = new ActionCommand(arg =>
            {
                IsShowSourceCode = !IsShowSourceCode;
            });
        }

        //private void ViewChange()
        //{
        //    if (ViewStateIsRender == "CodeView")
        //    {
        //        ViewStateIsRender = "CodeRender";
        //        VisualStateManager.GoToState(this, "STATE_CodeRender", true);
        //    }
        //    else
        //    {
        //        ViewStateIsRender = "CodeView";
        //        VisualStateManager.GoToState(this, "STATE_CodeView", true);
        //    }
        //}
        private async Task CopyMethod()
        {
            string code = SourceCode;
            await Task.Run(() =>
           {
               try
               {
                   Dispatcher.Invoke(() =>
                   {
                       Clipboard.SetText(code);
                       VisualStateManager.GoToState(this, "STATE_CopySuccess", true);
                   });
               }
               catch
               {
                   Dispatcher.Invoke(() =>
                   {
                       VisualStateManager.GoToState(this, "STATE_CopyError", true);
                   });
               }
           });
            await Task.Delay(1000);
            VisualStateManager.GoToState(this, "STATE_Default", true);
        }
    }
}
