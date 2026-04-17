using Microsoft.Extensions.DependencyInjection;
using QLLMChat.Helpers;
using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLLMChat.ViewModels
{
    public class FirstChatPageViewModel : ViewModelBase
    {

        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading; set
            {
                _IsLoading = value;
                OnPropertyChange();
            }
        }

        private ChatTypeItemModel _SelectedChatTypeItem;
        public ChatTypeItemModel SelectedChatTypeItem
        {
            get => _SelectedChatTypeItem;
            set
            {
                _SelectedChatTypeItem = value;
                OnPropertyChange();
            }
        }
        private ChatRoleDataModel? _SelectedChatRole;
        public ChatRoleDataModel? SelectedChatRole
        {
            get => _SelectedChatRole;
            set
            {
                _SelectedChatRole = value;
                OnPropertyChange();
            }
        }

        private ICommand _SendCommand;
        public ICommand SendCommand
        {
            get => _SendCommand;
            private set
            {
                _SendCommand = value;
                OnPropertyChange();
            }
        }

        private String _Text;
        public String Text
        {
            get => _Text;
            set
            {
                _Text = value;
                OnPropertyChange();
            }
        }


        private IServiceProvider ServiceProvider = null;
        private IChatModel ChatTarget = null;
        public ObservableCollection<ChatTypeItemModel> ChatTypes { get; set; } = new();
        public ObservableCollection<ChatRoleDataModel> ChatRoles { get; set; } = new();
        public FirstChatPageViewModel(IServiceProvider Service, Action<FirstChatPageViewModel> CreateChatPageAction)
        {
            this.ServiceProvider = Service;
            this.ChatTarget = Service.GetRequiredService<IChatModel>();
            _ = Init();
            SendCommand = new ActionCommand(arg =>
            {
                CreateChatPageAction(this);
            });
        }

        public async Task Init()
        {
            IsLoading = true;
            List<ChatTypeItemModel> items = new();
            if (ChatTarget is IMultiChatTypes multiType)
            {
                var data = await multiType.GetSupportedTypes();
                items.AddRange(data);
            }
            items.Add(ChatTypeItemModel.DefaultItem);
            foreach (var item in items)
            {
                ChatTypes.Add(item);
            }
            SelectedChatTypeItem = ChatTypes[0];
            foreach (var i in ChatRoleDataManager.Default.GetRoles())
            {
                this.ChatRoles.Add(i);
            }
            if (ChatRoles.Count != 0)
                SelectedChatRole = ChatRoles[0];
            IsLoading = false;
        }
    }
}
