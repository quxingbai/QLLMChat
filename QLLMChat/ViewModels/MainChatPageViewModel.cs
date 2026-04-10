using Microsoft.Extensions.DependencyInjection;
using QLLMChat.Helpers;
using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLLMChat.ViewModels
{
    public class MainChatPageViewModel : ViewModelBase
    {



        private IChatDataBase ChatDataBase = null;
        private IDispatcherProvider DispatcherProvider = null;
        private IChatModel ChatTarget = null;
        private Dictionary<ChatTargetViewModel, ChatPageViewModel> ChatPages = new();
        private bool CanInput = false;
        public ObservableCollection<ChatTargetViewModel> ChatTargets { get; set; } = new();



        private String _Text { get; set; }
        public String Text
        {
            get => _Text;
            set
            {
                var old = _Text;
                _Text = value;
                OnPropertyChange();
                InputTextChanged(old, value);
            }
        }

        private object _ChatPageContent { get; set; }
        public object ChatPageContent
        {
            get => _ChatPageContent; set
            {
                _ChatPageContent = value;
                OnPropertyChange();
            }
        }

        private ChatTargetViewModel _SelectedChatTarget;
        public ChatTargetViewModel SelectedChatTarget
        {
            get => _SelectedChatTarget; set
            {
                _SelectedChatTarget = value;
                OnPropertyChange();
                SelectedChatTargetChanged(value);
            }
        }

        private bool _IsSending = false;
        public bool IsSending
        {
            get => _IsSending;
            set
            {
                _IsSending = value;
                OnPropertyChange();
            }
        }

        public ICommand COMMAND_Send { get; set; }
        public ICommand COMMAND_Cancel { get; set; }
        public ICommand COMMAND_CreateNewChatTarget { get; set; }


        public MainChatPageViewModel(IServiceProvider Service)
        {
            this.ChatDataBase = Service.GetRequiredService<IChatDataBase>();
            this.DispatcherProvider = Service.GetRequiredService<IDispatcherProvider>();
            this.ChatTarget = Service.GetRequiredService<IChatModel>();
            CancellationTokenSource SendTaskCancelSource = null;
            (ChatTargetMessageModel, ChatTargetMessageViewModel)? CanCacnelMessage = null;

            COMMAND_Send = new ActionCommand(async arg =>
            {
                IsSending = true;
                try
                {
                    var nowChatTarget = SelectedChatTarget;
                    SendTaskCancelSource = new();
                    Text = "";
                    var page = ChatPageContent as ChatPageViewModel;
                    bool isFirstMessage = page.Messages.Count == 0,isChangedAutoTitle=false;

                    string text = arg.ToString();

                    var selfMessage = new ChatTargetMessageModel()
                    {
                        Message = text,
                        Sender = "user"
                    };
                    page.AddMessage(selfMessage);

                    var context = await ChatDataBase.GetChatTargetMessagesAsync(nowChatTarget.ChatId);
                    var vmMessage = page.CreateStringStreamMessage(0);
                    CanCacnelMessage = (vmMessage.Model, vmMessage.ViewModel);
                    var msg = new ChatRequestMessageModel()
                    {
                        Messages = context,
                        SendContent = text,
                    };
                    if (isFirstMessage)
                    {
                        nowChatTarget.Title = "思考中";
                    }
                    await this.ChatTarget.ChatAsync(msg, response =>
                    {
                        if (response.Message != "")
                        {
                            vmMessage.Write(response.Message);
                            //如果是第一条消息，设置标题为前几个字符
                            if (isFirstMessage)
                            {
                                if (!isChangedAutoTitle)
                                {
                                    isChangedAutoTitle = true;
                                    nowChatTarget.Title = "";
                                }
                                if(nowChatTarget.Title.ToString().Length < 30)
                                {
                                    nowChatTarget.Title += (response.Message.Trim());
                                }
                            }
                        }
                    }, SendTaskCancelSource.Token);

                    var response = vmMessage.Complet();
                    await ChatDataBase.AddChatMessageAsync(nowChatTarget.ChatId, selfMessage);
                    await ChatDataBase.AddChatMessageAsync(nowChatTarget.ChatId, response.Item1);
                }
                catch (Exception error)
                {
                    var t = error.GetType();

                    if (error is OperationCanceledException cancelError)
                    {
                        CanCacnelMessage.Value.Item2.State = ChatTargetMessageViewModel.ChatMessageState.Canceled;
                    }
                    else throw error;
                }
                IsSending = false;
                (COMMAND_Send as ActionCommand).OnCanExecuteChanged();
            }, arg =>
            {
                return !IsSending && CanInput;
            });
            COMMAND_Cancel = new ActionCommand(arg =>
            {
                SendTaskCancelSource?.Cancel();
                SendTaskCancelSource = null;
                IsSending = false;
                (COMMAND_Send as ActionCommand).OnCanExecuteChanged();
            });
            COMMAND_CreateNewChatTarget = new ActionCommand(async arg =>
            {
                await CreateChatTaargetAsync("新的对话");
            });
            _ = InitAsync();
        }

        private async Task CreateChatTaargetAsync(String Title)
        {
            var targetId = await this.ChatDataBase.AddChatTargetAsync();
            var target = await this.ChatDataBase.GetChatTargetAsync(targetId);
            var vm = CreateChatTarget(target);
            ChatTargets.Add(vm);

            this.SelectedChatTarget = vm;

            ChatTargetViewModel CreateChatTarget(ChatTargetModel Model)
            {
                return new ChatTargetViewModel()
                {
                    Title = Model.ChattName,
                    SubTitle = Model.ChatText,
                    ChatId = Model.ChatId,
                };
            }
        }
        private async Task InitAsync()
        {
            var data = await ChatDataBase.GetChatTargetsAsync();
            foreach (var i in data)
            {
                ChatTargets.Add(new ChatTargetViewModel()
                {
                    Title = i.ChattName,
                    SubTitle = i.ChatText,
                    ChatId = i.ChatId,
                });
            }
        }

        private void SelectedChatTargetChanged(ChatTargetViewModel ChatTarget)
        {
            bool needInit = false;
            ChatPageViewModel Page = null;
            if (ChatPages.ContainsKey(ChatTarget))
            {
                Page = ChatPages[ChatTarget];
            }
            else
            {
                Page = new ChatPageViewModel();
                needInit = true;
            }
            ChatPageContent = Page;

            if (needInit)
            {
                ChatPages.Add(ChatTarget, Page);
                ChatDataBase.GetChatTargetMessagesAsync(ChatTarget.ChatId).ContinueWith(w =>
                {
                    if (ChatPageContent == Page)
                        DispatcherProvider.GetDispatcher().BeginInvoke(() =>
                        {
                            foreach (var i in w.Result)
                            {
                                Page.AddMessage(i);
                            }
                        });
                });
            }
        }
        private void InputTextChanged(String OldText,String NewText)
        {
            if (NewText.Trim() == "")
            {
                CanInput = false;
            }
            else
            {
                CanInput = true;
            }
             (COMMAND_Send as ActionCommand).OnCanExecuteChanged();
        }
    }
}
