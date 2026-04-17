using QBlockData.DataStructs;
using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QLLMChat.DataBases
{
    public class QBlockDataDatabase : IChatDataBase
    {
        private QBlockData.BaseTypeFileBlockDataShell blockDataShell = null;
        protected int NextChatTargetID = 1;
        private object _lock = new object();
        private readonly string ChatTargetIDKey = "NextChatTargetID";
        private readonly string NextMessageIDKey = "NextChatMessageID";

        private int GetNextChatTargetID()
        {
            int data = blockDataShell.QueryInt(ChatTargetIDKey) ?? 1;
            blockDataShell.Delete(ChatTargetIDKey);
            blockDataShell.AddInt32(ChatTargetIDKey, data + 1);
            return data;
        }
        private int GetNextChatMessageID()
        {
            int data = blockDataShell.QueryInt(NextMessageIDKey) ?? 1;
            blockDataShell.Delete(NextMessageIDKey);
            blockDataShell.AddInt32(NextMessageIDKey, data + 1);
            return data;
        }
        public QBlockDataDatabase(string FileName, int BlockSize = 50)
        {
            blockDataShell = new QBlockData.BaseTypeFileBlockDataShell(new QBlockData.XmlFileBlockData(FileName, BlockSize));
        }
        public async Task<int> AddChatMessageAsync(int ChatID, ChatTargetMessageModel Message)
        {
            string key = CreateChatMessageKey(ChatID);
            var queue = await GetChatTargetMessagesAsync(ChatID);
            UpdateChatMessages(ChatID, queue.Append(new ChatTargetMessageModel() { MessageID = GetNextChatMessageID(), Message = Message.Message, Sender = Message.Sender }));
            return 1;
        }
        private void UpdateChatMessages(int ChatID, IEnumerable<ChatTargetMessageModel> Messages)
        {
            string key = CreateChatMessageKey(ChatID);
            List<TLVData> ts = new();
            foreach (var item in Messages)
            {
                ts.Add(new TLVData(TLVDataTags.Int, item.MessageID));
                ts.Add(new TLVData(TLVDataTags.String, item.Message));
                ts.Add(new TLVData(TLVDataTags.String, item.Sender));
            }
            blockDataShell.AddOrUpdateTlvDatas(key, ts);
        }

        public async Task<ChatTargetModel?> AddChatTargetAsync(ChatTargetModel Target)
        {
            return await AddChatTargetAsync(Target, true).ConfigureAwait(false);
        }
        private async Task<ChatTargetModel?> AddChatTargetAsync(ChatTargetModel Target, bool IsAutoChatID)
        {
            Target.ChatId = IsAutoChatID ? GetNextChatTargetID() : Target.ChatId;
            string key = CreateChatTargetKey(Target.ChatId);
            if (Target.ChatTargetType == null) Target.ChatTargetType = ChatTypeItemModel.DefaultItem;
            return await Task.Run(async () =>
            {
                blockDataShell.AddOrUpdateTlvDatas(key, new List<TLVData>() {
                    new TLVData( TLVDataTags.Int, Target.ChatId),
                    new TLVData( TLVDataTags.String, Target.ChatRoleName??""),
                    new TLVData( TLVDataTags.String, Target.ChattName??""),
                    new TLVData( TLVDataTags.String, Target.ChatTargetType.Model??""),
                    new TLVData( TLVDataTags.String, Target.ChatTargetType.Name??""),
                    new TLVData( TLVDataTags.Long, Target.ChatTargetType.Size),
                    new TLVData( TLVDataTags.String, Target.ChatTargetType.Text??""),
                    new TLVData( TLVDataTags.String, Target.ChatTargetType.Title??""),
                 });
                return await GetChatTargetAsync(Target.ChatId).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<ChatTargetModel?> GetChatTargetAsync(int ChatID)
        {
            string key = CreateChatTargetKey(ChatID);
            return await Task.Run(() =>
            {
                var queue = blockDataShell.QueryTlvDatasQueue(key);
                return new ChatTargetModel()
                {
                    ChatId = queue.Dequeue().ReadToInt(),
                    ChatRoleName = queue.Dequeue().ReadToString(),
                    ChattName = queue.Dequeue().ReadToString(),
                    ChatTargetType = new ChatTypeItemModel()
                    {
                        Model = queue.Dequeue().ReadToString(),
                        Name = queue.Dequeue().ReadToString(),
                        Size = queue.Dequeue().ReadToLong(),
                        Text = queue.Dequeue().ReadToString(),
                        Title = queue.Dequeue().ReadToString(),
                    }
                };
            }).ConfigureAwait(false);

        }

        public async Task<IEnumerable<ChatTargetMessageModel>> GetChatTargetMessagesAsync(int ChatID)
        {
            string key = CreateChatMessageKey(ChatID);
            return await Task.Run(() =>
            {
                var queue = blockDataShell.QueryTlvDatasQueue(key);
                if (queue == null) return Enumerable.Empty<ChatTargetMessageModel>();
                List<ChatTargetMessageModel> rs = new();
                while (queue.Any())
                {
                    rs.Add(new ChatTargetMessageModel() { MessageID =queue.Dequeue().ReadToInt(),Message= queue.Dequeue().ReadToString(), Sender = queue.Dequeue().ReadToString() });
                }
                return rs;
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ChatTargetModel>> GetChatTargetsAsync()
        {
            return await Task.Run(async () =>
            {
                List<ChatTargetModel> rs = new();
                var ks = blockDataShell.GetControllerTarget().GetKeys();
                foreach (var i in ks)
                {
                    if (IsChatTargetKey(i, out var ChatID))
                    {
                        var data = await GetChatTargetAsync(ChatID);
                        if (data != null) rs.Add(data);
                    }
                }
                return rs;
            }).ConfigureAwait(false);
        }

        public async Task<int> RemoveChatMessageAsync(int ChatID, int MessageID)
        {
            string key = CreateChatMessageKey(ChatID);
            var ms = await GetChatTargetMessagesAsync(ChatID).ConfigureAwait(false);
            List<ChatTargetMessageModel> rs = new();
            int count = 0;
            foreach (var i in ms)
            {
                if (i.MessageID == MessageID)
                {
                    count++;
                }
                else
                {
                    rs.Add(i);
                }
            }
            if (count != 0)
            {
                UpdateChatMessages(ChatID, rs);
            }
            return count;

        }


        public async Task<int> RemoveChatTargetAsync(int ChatID)
        {
            return await RemoveChatTargetAsync(ChatID, true).ConfigureAwait(false);

        }
        private async Task<int> RemoveChatTargetAsync(int ChatID, bool IsRemoveChatMessage = true)
        {
            return await Task.Run(() =>
            {
                int count = 0;
                var key1 = CreateChatTargetKey(ChatID);
                if (blockDataShell.GetControllerTarget().HasKey(key1))
                {
                    blockDataShell.Delete(key1);
                    count++;
                }
                var key2 = CreateChatMessageKey(ChatID);
                if (IsRemoveChatMessage && blockDataShell.GetControllerTarget().HasKey(key2))
                {
                    blockDataShell.Delete(key2);
                    count++;
                }
                return count;
            }).ConfigureAwait(false);

        }

        public async Task<ChatTargetModel?> UpdateChatTargetAsync(int ChatID, ChatTargetModel Target)
        {
            return await Task.Run(async () =>
               {
                   await RemoveChatTargetAsync(ChatID, false);
                   Target.ChatId = ChatID;
                   return await AddChatTargetAsync(Target, false);
               }).ConfigureAwait(false);
        }

        private static string CreateChatMessageKey(int ChatID)
        {
            return $"Chat_{ChatID}_Messages";
        }
        private static string CreateChatTargetKey(int ChatID)
        {
            return $"Chat_{ChatID}_Target";
        }
        private static bool IsChatTargetKey(String Key, out int OutChatID)
        {
            string pattern2 = @"^Chat_.+_Target$";
            bool isMatch2 = Regex.IsMatch(Key, pattern2);
            OutChatID = GetChatTargetKeyID(Key) ?? -1;
            return isMatch2;
        }
        private static int? GetChatTargetKeyID(String Key)
        {
            string pattern = @"^Chat_(.+)_Target$";
            Match match = Regex.Match(Key, pattern);
            return match.Success ? int.Parse(match.Groups[1].Value) : null;
        }
    }
}