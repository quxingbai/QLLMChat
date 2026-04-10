using MultimodalSharp.Ollama.Clients;
using QLLMChat.Models.ChatEntities;
using QLLMChat.ViewModels;
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
        }
    }
}