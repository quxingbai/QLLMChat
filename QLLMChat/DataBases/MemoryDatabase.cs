using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.DataBases
{
    public class MemoryDatabase : IChatDataBase
    {
        protected Dictionary<int, ChatTargetModel> ChatTargeets = new();
        protected Dictionary<int, List<ChatTargetMessageModel>> ChatTargetMessages = new();

        protected int NextChatTargetID = 1;
        private object _lock = new object();
        private int GetNextChatTargetID()
        {
            var id = -1;
            lock (_lock)
            {
                id = NextChatTargetID;
                NextChatTargetID += 1;
            }
            return id;
        }
        public virtual async Task<ChatTargetModel> AddChatTargetAsync(ChatTargetModel Target)
        {
            return await Task.Run(() =>
            {
                Target.ChatId= GetNextChatTargetID();
                ChatTargeets.Add(Target.ChatId, Target);
                return Target;
            }).ConfigureAwait(false);
        }

        public virtual async Task<ChatTargetModel?> GetChatTargetAsync(int ChatID)
        {
            return await Task.Run(() => ChatTargeets.ContainsKey(ChatID) ? ChatTargeets[ChatID] : null).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<ChatTargetMessageModel>> GetChatTargetMessagesAsync(int ChatID)
        {
            return await Task.Run(() => (IEnumerable<ChatTargetMessageModel>)(ChatTargetMessages.ContainsKey(ChatID) ? ChatTargetMessages[ChatID] : new())).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<ChatTargetModel>> GetChatTargetsAsync()
        {
            return await Task.Run(() => (IEnumerable<ChatTargetModel>)ChatTargeets.Values).ConfigureAwait(false);
        }

        public virtual async Task<int> RemoveChatTargetAsync(int ChatID)
        {
            return await Task.Run(() =>
            {
                if (ChatTargeets.ContainsKey(ChatID))
                {
                    ChatTargeets.Remove(ChatID);
                    return 1;
                }
                return -1;
            }).ConfigureAwait(false);
        }

        public virtual async Task<int> AddChatMessageAsync(int ChatID, ChatTargetMessageModel Message)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!ChatTargetMessages.ContainsKey(ChatID))
                    {
                        ChatTargetMessages.Add(ChatID, new());

                    }
                    ChatTargetMessages[ChatID].Add(Message);
                }
                return 1;
            }).ConfigureAwait(false);
        }

        public virtual async Task<int> RemoveChatMessageAsync(int ChatID, int MessageID)
        {
            return await Task.Run(() =>
            {
                var target = ChatTargetMessages[ChatID];
                var query = target.Where(w => w.MessageID == MessageID);
                if (query.Any())
                {
                    target.Remove(query.First());
                    return 1;
                }
                else
                {
                    return 0;
                }
            }).ConfigureAwait(false);
        }

        public virtual async Task<ChatTargetModel?> UpdateChatTargetAsync(int ChatID, ChatTargetModel Target)
        {
            return await Task.Run(() =>
            {
                if (ChatTargeets.ContainsKey(ChatID))
                {
                    ChatTargeets[ChatID] = Target;
                    return Target;
                }
                else
                {
                    return null;
                }
            }).ConfigureAwait(false);

        }

    }
}
