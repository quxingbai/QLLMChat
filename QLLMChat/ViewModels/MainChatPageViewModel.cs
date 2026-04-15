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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace QLLMChat.ViewModels
{
    public class MainChatPageViewModel : ViewModelBase
    {



        private IChatDataBase ChatDataBase = null;
        private IDispatcherProvider DispatcherProvider = null;
        private IChatModel ChatTarget = null;
        private IServiceProvider ServiceProvider = null;
        private Dictionary<ChatTargetViewModel, ChatPageViewModel> ChatPages = new();
        private bool CanInput = true;
        public ObservableCollection<ChatTargetViewModel> ChatTargets { get; set; } = new();
        public Lazy<IEnumerable<ChatTypeItemModel>> ChatTypeItems { get; set; }

        private ChatTypeItemModel _SelectedChatType = null;
        public ChatTypeItemModel SelectedChatType
        {
            get => _SelectedChatType; set
            {
                _SelectedChatType = value;
                OnPropertyChange();
            }
        }

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

        public ICommand COMMAND_FirstChatSendCommand { get; set; }

        public MainChatPageViewModel(IServiceProvider Service)
        {
            this.ChatDataBase = Service.GetRequiredService<IChatDataBase>();
            this.DispatcherProvider = Service.GetRequiredService<IDispatcherProvider>();
            this.ChatTarget = Service.GetRequiredService<IChatModel>();
            this.ServiceProvider = Service;
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
                    bool isFirstMessage = page.Messages.Count == 0, isChangedAutoTitle = false;

                    string text = arg.ToString();

                    var selfMessage = new ChatTargetMessageModel()
                    {
                        Message = text,
                        Sender = "user"
                    };
                    page.AddMessage(selfMessage);

                    var context = await ChatDataBase.GetChatTargetMessagesAsync(nowChatTarget.ChatId).ConfigureAwait(true);
                    var vmMessage = page.CreateStringStreamMessage(0);
                    CanCacnelMessage = (vmMessage.Model, vmMessage.ViewModel);
                    var msg = new ChatRequestMessageModel()
                    {
                        Messages = context,
                        SendContent = text,
                        CustomChatType = nowChatTarget.ChatType
                    };
                    if (isFirstMessage)
                    {
                        nowChatTarget.Title = "思考中";
                    }
                    await this.ChatTarget.ChatAsync(msg, response =>
                    {
                        DispatcherProvider.GetDispatcher().Invoke(() =>
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
                                    if (nowChatTarget.Title.ToString().Length < 30)
                                    {
                                        nowChatTarget.Title += (response.Message.Trim());
                                    }
                                    else
                                    {
                                        isFirstMessage = false;
                                    }
                                }
                            }
                        });
                    }, SendTaskCancelSource.Token);

                   _= await ChatDataBase.UpdateChatTargetAsync(nowChatTarget.ChatId, new ChatTargetModel()
                    {
                        ChatId = nowChatTarget.ChatId,
                        ChatTargetType = nowChatTarget.ChatType,
                        ChattName = nowChatTarget.Title
                    }).ConfigureAwait(true);

                    var response = vmMessage.Complet();

                    await ChatDataBase.AddChatMessageAsync(nowChatTarget.ChatId, selfMessage);
                    await ChatDataBase.AddChatMessageAsync(nowChatTarget.ChatId, response.Item1);
                    //for (int i = 0; i < 100; i++)
                    //{
                    //    page.AddMessage(selfMessage);
                    //    page.AddMessage(response.Item1);
                    //}
                }
                catch (Exception error)
                {
                    var t = error.GetType();

                    if (error is OperationCanceledException cancelError)
                    {
                        CanCacnelMessage.Value.Item2.State = ChatTargetMessageViewModel.ChatMessageState.Canceled;
                    }
                    else
                    {
                        CanCacnelMessage.Value.Item2.State = ChatTargetMessageViewModel.ChatMessageState.Canceled;
                        MessageBox.Show("发送消息时发生错误：" + error.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    //else throw error;
                }
                IsSending = false;
                DispatcherProvider.GetDispatcher().Invoke(() =>
                {
                    (COMMAND_Send as ActionCommand).OnCanExecuteChanged();
                });
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
                await CreateChatTaargetAsync("新的对话", SelectedChatType);
            });
            COMMAND_FirstChatSendCommand = new ActionCommand(async arg => {
                SelectedChatTargetChanged(this.SelectedChatTarget);
                if (COMMAND_Send.CanExecute(arg))
                {
                    COMMAND_Send.Execute(arg);
                }
            } );
            _ = InitAsync();
        }


        private async Task CreateChatTaargetAsync(String Title, ChatTypeItemModel CustomChatType = null)
        {
            var add= await this.ChatDataBase.AddChatTargetAsync(new ChatTargetModel { ChattName = Title });
            var targetId = add.ChatId;
            var target = await this.ChatDataBase.GetChatTargetAsync(targetId);
            var vm = CreateChatTarget(target);
            //ChatTargets.Add(vm);
            ChatTargets.Insert(0,vm);


            this.SelectedChatTarget = vm;

            ChatTargetViewModel CreateChatTarget(ChatTargetModel Model)
            {
                return new ChatTargetViewModel()
                {
                    Title = Model.ChattName,
                    SubTitle = SelectedChatType?.Text,
                    ChatId = Model.ChatId,
                    ChatType = CustomChatType
                };
            }
        }
        private async Task InitAsync()
        {
            var data = await ChatDataBase.GetChatTargetsAsync();
            DispatcherProvider.GetDispatcher().Invoke(() =>
            {
                foreach (var i in data.OrderByDescending(d=>d.ChatId))
                {
                    ChatTargets.Add(new ChatTargetViewModel()
                    {
                        Title = i.ChattName,
                        ChatId = i.ChatId,
                        ChatType=i.ChatTargetType
                    });
                }
            });

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
                var dt= this.ChatDataBase.GetChatTargetMessagesAsync(ChatTarget.ChatId).Result;
                if (dt.Any())
                {
                    Page=new ChatPageViewModel();
                    needInit = true;
                }

            }
            needInit = Page == null ? false : Page.Messages.Count == 0;
            ChatPageContent = Page == null ? new FirstChatPageViewModel(this.ServiceProvider, create =>
            {
                ChatTarget.ChatType = create.SelectedChatTypeItem;
                ChatPageViewModel chatPageVm = new();
                ChatPages.Add(ChatTarget, chatPageVm);
                SelectedChatTargetChanged(ChatTarget);
                ChatDataBase.UpdateChatTargetAsync(ChatTarget.ChatId, new()
                {
                    ChatId=ChatTarget.ChatId,
                    ChatTargetType=ChatTarget.ChatType,
                    ChattName=ChatTarget.Title.ToString()
                }); ;


                string text = create.Text;
                this.Text = text;
                if (COMMAND_Send.CanExecute(text))
                {
                    COMMAND_Send.Execute(text);
                }
            }) : Page;


            if (Page != null&&needInit)
            {
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
        private void InputTextChanged(String OldText, String NewText)
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
