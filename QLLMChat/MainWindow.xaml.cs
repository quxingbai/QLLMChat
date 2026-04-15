using MultimodalSharp.Ollama.Clients;
using QLLMChat.Models.ChatEntities;
using QLLMChat.ViewModels;
using QLLMChat.Views.CControls;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLLMChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel ViewModel)
        {
            InitializeComponent();
            this.DataContext = ViewModel;
            Task.Delay(1000).Wait();
        }

        //private void WpfElementRenderBox_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var ele=(sender as CustomRenderMarkdownViewer);
        //    var c = "\"";
        //    string fragment = @"<Button  Content='Click Me'/>";
        //    ele.Markdown = $"# Hello World<QLLMRender Type=\"WPF\">{fragment}</QLLMRender>";
        //}
    }
}