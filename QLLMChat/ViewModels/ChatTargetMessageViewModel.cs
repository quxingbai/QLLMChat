using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.ViewModels
{
    public class ChatTargetMessageViewModel : ViewModelBase
    {
        public enum ChatMessageState
        {
            Sended,
            Writeing,
            Canceled,
        }
        private object _Message { get; set; }
        public object Message
        {
            get => _Message;
            set
            {
                _Message = value;
                OnPropertyChange();
                OnPropertyChange("MessageText");
            }
        }
        public String Sender { get; set; }
        public String MessageText => (Message ?? "").ToString();

        private ChatMessageState _State;

        public ChatMessageState State
        {
            get => _State;
            set
            {
                _State = value;
                OnPropertyChange();
            }
        }
    }
    public class ChatTargetMessage_User_ViewModel : ChatTargetMessageViewModel
    {

    }
    public class ChatTargetMessage_LLModel_ViewModel : ChatTargetMessageViewModel
    {

    }
}
