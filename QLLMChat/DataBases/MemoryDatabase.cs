using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.DataBases
{
    internal class MemoryDatabase : IChatDataBase
    {
        private Dictionary<int, ChatTargetModel> ChatTargeets = new();
        private Dictionary<int, List<ChatTargetMessageModel>> ChatTargetMessages = new();

        private int NextChatTargetID = 1;
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
        public async Task<int> AddChatTargetAsync(String? Title = null)
        {
            var id = GetNextChatTargetID();
            return await Task.Run(() =>
            {
                ChatTargeets.Add(id, new() { ChatId = id, ChatText = "...", ChattName = Title ?? "新建对话" });
                return id;
            });
        }

        public async Task<ChatTargetModel?> GetChatTargetAsync(int ChatID)
        {
            return await Task.Run(() => ChatTargeets.ContainsKey(ChatID) ? ChatTargeets[ChatID] : null);
        }

        public async Task<IEnumerable<ChatTargetMessageModel>> GetChatTargetMessagesAsync(int ChatID)
        {
            return await Task.Run(() => (IEnumerable<ChatTargetMessageModel>)(ChatTargetMessages.ContainsKey(ChatID) ? ChatTargetMessages[ChatID] : new()));
        }

        public async Task<IEnumerable<ChatTargetModel>> GetChatTargetsAsync()
        {
            return await Task.Run(() => (IEnumerable<ChatTargetModel>)ChatTargeets.Values);
        }

        public async Task<int> RemoveChatTargetAsync(int ChatID)
        {
            return await Task.Run(() =>
            {
                if (ChatTargeets.ContainsKey(ChatID))
                {
                    ChatTargeets.Remove(ChatID);
                    return 1;
                }
                return -1;
            });
        }

        public async Task<int> AddChatMessageAsync(int ChatID,ChatTargetMessageModel Message)
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
            });
        }

        public async Task<int> RemoveChatMessageAsync(int ChatID,int MessageID)
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
            });
        }
    }
}
