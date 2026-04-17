using Newtonsoft.Json;
using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QLLMChat.DataBases
{
    public class JsonDatabase : MemoryDatabase
    {
        private record struct SaveData
        {
            public ChatTargetModel ChatTarget { get; set; }
            public IEnumerable<ChatTargetMessageModel> Messages { get; set; }
        }
        private String FileName;
        public JsonDatabase(String FileName)
        {
            this.FileName = FileName;
            Load().ConfigureAwait(false);
        }
        private async Task Save()
        {
            var data = await GetSaveData();
            var text = JsonConvert.SerializeObject(data);

            File.Delete(FileName);
            var f = File.CreateText(FileName);
            f.WriteLine(text);
            f.Dispose();
        }
        private async Task Load()
        {
             await Task.Run(() =>
            {
                ChatTargeets.Clear();
                ChatTargetMessages.Clear();
                if (!File.Exists(FileName))
                {
                    return;
                }
                var text = File.ReadAllText(FileName);
                var data = JsonConvert.DeserializeObject<IEnumerable<SaveData>>(text);
                if (data == null) return;
                foreach (var item in data)
                {
                    base.ChatTargeets.Add(item.ChatTarget.ChatId, item.ChatTarget);
                    base.ChatTargetMessages.Add(item.ChatTarget.ChatId, item.Messages.ToList());
                    base.NextChatTargetID = item.ChatTarget.ChatId >= base.NextChatTargetID ? item.ChatTarget.ChatId + 1 : base.NextChatTargetID;
                }
            }).ConfigureAwait(false);
        }

        private async Task<IEnumerable<SaveData>> GetSaveData()
        {
            var saveData = new Dictionary<ChatTargetModel, IEnumerable<ChatTargetMessageModel>>();
            foreach (var chat in await base.GetChatTargetsAsync())
            {
                saveData.Add(chat, await base.GetChatTargetMessagesAsync(chat.ChatId));
            }

            return saveData.Select(kv => new SaveData
            {
                ChatTarget = kv.Key,
                Messages = kv.Value
            });
        }
        public override async Task<ChatTargetModel?> AddChatTargetAsync(ChatTargetModel Target)
        {
            var r= await base.AddChatTargetAsync(Target);
            await Save();
            return r;
        }
        public override async Task<int> AddChatMessageAsync(int ChatID, ChatTargetMessageModel Message)
        {
            var r = await base.AddChatMessageAsync(ChatID, Message);
            await Save();
            return r;
        }
        public override async Task<int> RemoveChatMessageAsync(int ChatID, int MessageID)
        {
            var r = await base.RemoveChatMessageAsync(ChatID, MessageID);
            await Save();
            return r;
        }
        public override async Task<int> RemoveChatTargetAsync(int ChatID)
        {
            var r = await base.RemoveChatTargetAsync(ChatID);
            await Save();
            return r;
        }
        public override async Task<ChatTargetModel?> UpdateChatTargetAsync(int ChatID, ChatTargetModel Target)
        {
            var r= await base.UpdateChatTargetAsync(ChatID, Target);
            await Save();
            return r;
        }
    }
}
