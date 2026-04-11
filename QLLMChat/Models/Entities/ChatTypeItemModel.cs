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
            Data = "。。。。."
        };
        public String Title { get; set; }
        public String Text { get; set; }
        public object Data { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj is ChatTypeItemModel sm)
            {
                return sm.Title==Title&&sm.Data==Data;
            }
            return false;
        }
    }
}
