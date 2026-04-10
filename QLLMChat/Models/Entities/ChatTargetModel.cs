using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Entities
{
    public class ChatTargetModel
    {
        public String ChattName { get; set; }
        public String ChatText { get; set; }
        public int ChatId { get; set; }
        public String ChatTargetType { get; set; } = "LLModel";
    }
}
