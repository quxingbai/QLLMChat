using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Entities
{
    public class ChatTypeItemModel
    {
        public static readonly ChatTypeItemModel DefaultItem = new()
        {
            Title = "默认选项",
            Text = "使用默认模型进行聊天",
        };
        public String Title { get; set; }
        public String Text { get; set; }
        public long Size { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj is ChatTypeItemModel sm)
            {
                return sm.Title==Title&&sm.Size==Size&&sm.Model==Model;
            }
            return false;
        }
    }
}
