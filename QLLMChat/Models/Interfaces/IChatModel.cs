using QLLMChat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Interfaces
{
    public interface IChatModel
    {
        /// <summary>
        /// 流对话
        /// </summary>
        public Task ChatAsync(ChatRequestMessageModel Message, Action<ChatResponseStreamMessageModel> Response, CancellationToken? CancelToken = null);
        /// <summary>
        /// 文本对话
        /// </summary>
        public Task<ChatResponseMessageModel> ChatAsync(ChatRequestMessageModel Message, CancellationToken? CancelToken = null);
        public String GetUserMessageName();
    }
}
