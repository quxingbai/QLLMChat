using QLLMChat.ViewModels;
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

namespace QLLMChat.Views.Pages
{
    /// <summary>
    /// MainChatPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainChatPage : UserControl
    {
        private bool IsLastScrollToBottom = true;
        public MainChatPage()
        {
            InitializeComponent();
        }
        private void SCROLL_ChatPage_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;

            if (e.VerticalChange == 0)
            {

                if (IsLastScrollToBottom)
                {
                    scrollViewer.ScrollToBottom();
                }

            }

            var vp = scrollViewer.ViewportHeight;
            var h1 = scrollViewer.ExtentHeight;
            //证明需要滚动了
            if (h1 > vp)
            {
                bool inBottom = scrollViewer.ContentVerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight;
                IsLastScrollToBottom = inBottom;
            }
        }

        private void SCROLL_ChatPage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
