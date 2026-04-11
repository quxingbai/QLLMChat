using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.ChatEntities
{
    public abstract class ChatBase : IChatModel
    {
        public abstract Task ChatAsync(ChatRequestMessageModel Message, Action<ChatResponseStreamMessageModel> Response, CancellationToken? CancelToken = null);
        public abstract Task<ChatResponseMessageModel> ChatAsync(ChatRequestMessageModel Message, CancellationToken? CancelToken = null);

        public abstract void Dispose();

    }
}
