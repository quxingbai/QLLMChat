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
        public Task<ChatTargetModel?> UpdateChatTargetAsync(int ChatID, ChatTargetModel Target);

        public Task<ChatTargetModel?> AddChatTargetAsync(ChatTargetModel Target);
        /// <summary>
        /// 返回成功执行数量
        /// </summary>
        public Task<int> RemoveChatTargetAsync(int ChatID);
        /// <summary>
        /// 返回成功执行数量
        /// </summary>
        public Task<int> AddChatMessageAsync(int ChatID, ChatTargetMessageModel Message);
        /// <summary>
        /// 返回成功执行数量
        /// </summary>
        public Task<int> RemoveChatMessageAsync(int ChatID, int MessageID);
    }
}
