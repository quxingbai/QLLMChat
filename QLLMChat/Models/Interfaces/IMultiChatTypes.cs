using QLLMChat.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLLMChat.Models.Interfaces
{
    public interface IMultiChatTypes
    {
        public Task<IEnumerable<ChatTypeItemModel>> GetSupportedTypes();
    }
}
