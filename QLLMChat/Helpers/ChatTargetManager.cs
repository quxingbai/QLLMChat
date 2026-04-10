using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Helpers
{
    internal class ChatTargetManager
    {
        private IChatDataBase ChatDataBase { get; set; }
        public ChatTargetManager(IChatDataBase ChatDataBase)
        {
            this.ChatDataBase = ChatDataBase;
        }
        public async Task<ChatTargetModel> GetChatTarget(int ChatID)
        {
            var data = await ChatDataBase.GetChatTargetAsync(ChatID);
            return data;
        }
        public async Task<IEnumerable<ChatTargetMessageModel>> GetChatTargetMessage(int ChatID)
        {
            var data= await ChatDataBase.GetChatTargetMessagesAsync(ChatID);
            return data;
        }
    }
}
