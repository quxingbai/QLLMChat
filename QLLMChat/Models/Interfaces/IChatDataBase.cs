using QLLMChat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Interfaces
{
    public interface IChatDataBase
    {
        public Task<IEnumerable<ChatTargetModel>> GetChatTargetsAsync();
        public Task<ChatTargetModel?> GetChatTargetAsync(int ChatID);        
        public Task<IEnumerable<ChatTargetMessageModel>> GetChatTargetMessagesAsync(int ChatID);

        public Task<int> AddChatTargetAsync(String? Title=null);
        public Task<int> RemoveChatTargetAsync(int ChatID);
        public Task<int> AddChatMessageAsync(int ChatID,ChatTargetMessageModel Message);
        public Task<int> RemoveChatMessageAsync(int ChatID,int MessageID);

    }
}
