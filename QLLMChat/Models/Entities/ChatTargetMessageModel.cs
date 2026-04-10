using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Entities
{
    public class ChatTargetMessageModel
    {
        public int MessageID { get; set; }
        public String Message { get; set; }
        public String Sender { get; set; }
    }
}
