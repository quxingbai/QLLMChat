using Microsoft.Extensions.DependencyInjection;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLLMChat.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _IsIniting;
        public bool IsIniting
        {
            get { return _IsIniting; }
            set { _IsIniting = value; OnPropertyChange(); }
        }
        public MainChatPageViewModel MainChatPageViewModel { get; set; }
        private IServiceProvider serviceProvider;
        public MainWindowViewModel(IServiceProvider ServideProvider)
        {
            this.serviceProvider = ServideProvider;
            var disparcher = serviceProvider.GetRequiredService<IDispatcherProvider>();

            MainChatPageViewModel = ServideProvider.GetRequiredService<MainChatPageViewModel>();
        }

    }
}
