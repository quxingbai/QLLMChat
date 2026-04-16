using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Entities
{
    public class ChatRequestMessageModel
    {
        public string SendContent { get; set; }
        public IEnumerable<ChatTargetMessageModel> Messages { get; set; }
        public ChatTypeItemModel CustomChatType {  get; set; }
        public String SystemMessage { get; set; }= String.Empty;
    }
}
