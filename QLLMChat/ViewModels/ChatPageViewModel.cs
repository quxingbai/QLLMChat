using QLLMChat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static QLLMChat.ViewModels.ChatTargetMessageViewModel;

namespace QLLMChat.ViewModels
{
    public class ChatPageViewModel : ViewModelBase
    {
        public record StreamWriteChatTextMessage(Action<string> Write, Func<(ChatTargetMessageModel, ChatTargetMessageViewModel)> Complet)
        {
            public ChatTargetMessageModel Model { get; set; }
            public ChatTargetMessageViewModel ViewModel { get; set; }
        };
        public ObservableCollection<ChatTargetMessageViewModel> Messages { get; set; } = new ObservableCollection<ChatTargetMessageViewModel>();

        public ChatPageViewModel()
        {

        }
        public void AddMessage(ChatTargetMessageModel message) => Messages.Add(new ChatTargetMessageViewModel()
        {
            Message = message.Message,
            Sender = message.Sender,
            State = ChatMessageState.Sended
        });
        public StreamWriteChatTextMessage CreateStringStreamMessage(double UpdateSeconds = 1)
        {
            DateTime nextUpdateDate = DateTime.Now + (TimeSpan.FromSeconds(UpdateSeconds));
            ChatTargetMessageModel model = new() { Sender = "assistant" };
            ChatTargetMessageViewModel vModel = new() { Sender = "assistant", State = ChatMessageState.Writeing };
            Messages.Add(vModel);
            StringBuilder sb = new();
            StreamWriteChatTextMessage msg = new(text =>
            {
                sb.Append(text);
                if (UpdateSeconds == 0)
                {
                    vModel.Message = sb.ToString();
                }
                else
                {
                    if (DateTime.Now >= nextUpdateDate)
                    {
                        nextUpdateDate = DateTime.Now + (TimeSpan.FromSeconds(UpdateSeconds));
                    }
                }
            }, () =>
            {
                model.Message = sb.ToString();
                vModel.Message = sb.ToString();
                vModel.State = ChatMessageState.Sended;
                return (model, vModel);
            })
            {
                Model = model,
                ViewModel = vModel
            };
            return msg;
        }
    }
}
