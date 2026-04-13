using Microsoft.Extensions.DependencyInjection;
using MultimodalSharp.Ollama.Clients;
using QLLMChat.DataBases;
using QLLMChat.Models.ChatEntities;
using QLLMChat.Models.Interfaces;
using QLLMChat.ViewModels;
using System.Configuration;
using System.Data;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace QLLMChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            var service = new ServiceCollection();

            service.AddSingleton<IChatDataBase, JsonDatabase>(p =>
            {
                return new JsonDatabase("./chatdata.json");
            });
            service.AddSingleton<IChatModel, OllamaChat>();
            service.AddSingleton<IDispatcherProvider, AppDispatcher>();
            service.AddTransient<MainWindowViewModel>();
            service.AddTransient<MainChatPageViewModel>();
            service.AddTransient<MainWindow>();

            ServiceProvider serviceProvider = service.BuildServiceProvider();
            var window = serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
            base.OnStartup(e);
        }


        public class AppDispatcher : IDispatcherProvider
        {
            public Dispatcher GetDispatcher()
            {
                return App.Current.Dispatcher;
            }
        }

    }

}
