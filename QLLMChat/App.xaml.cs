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
            //这里是数据库的注入，可以根据需要替换成其他数据库实现
            //比如目前除了Json还有一个内存数据库MemoryDatabase，后续也可以添加其他数据库实现
            service.AddSingleton<IChatDataBase, QBlockDataDatabase>(p =>
            {
                //return new JsonDatabase("./chatdata.json");
                return new QBlockDataDatabase("./Database/chatdata.qdb",50);
            });
            //这里是聊天模型的注入，可以根据需要替换成其他聊天模型实现，实现了IMultiChatTypes接口的聊天模型会在界面上显示不同的模型，
            // public class OllamaChat : IChatModel, IMultiChatTypes
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
