using QLLMChat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.ViewModels
{
    public class ChatTargetViewModel:ViewModelBase
    {
        private string _Title;
        public string Title
        {
            get => _Title; set
            {
                _Title = value;
                OnPropertyChange();
            }
        }
        private object _SubTitle;
        public object SubTitle
        {
            get => _SubTitle; set
            {
                _SubTitle = value;
                OnPropertyChange();
            }
        }
        private int _ChatId;
        public int ChatId
        {
            get => _ChatId; set
            {
                _ChatId = value;
                OnPropertyChange();
            }
        }
        private ChatTypeItemModel _ChatType;
        public ChatTypeItemModel ChatType
        {
            get => _ChatType; set
            {
                _ChatType = value;
                OnPropertyChange();
            }
        }
    }
}
