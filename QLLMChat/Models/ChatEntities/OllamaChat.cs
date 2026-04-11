using MultimodalSharp.Ollama.Clients;
using MultimodalSharp.Ollama.Models.Entities;
using QLLMChat.Models.Entities;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MultimodalSharp.Ollama.Models.Entities.OllamaRequests;

namespace QLLMChat.Models.ChatEntities
{
    public class OllamaChat : ChatBase, IMultiChatTypes
    {
        public static readonly HttpClient OllamaHttpClient = new();
        private OllamaChatClient OllamaClient = null;
        private OllamaServicesClient OllamaServicess = null;
        private String ModelName = "deepseek-r1";
        public OllamaChat(IPEndPoint IP, String ModelName)
        {
            OllamaClient = new(new()
            {
                HttpClient = OllamaHttpClient,
                ServerIP = IP,
                ModelName = ModelName,
            });
        }
        public OllamaChat()
        {
            IPEndPoint IP = new(IPAddress.Loopback, 12344);
            this.ModelName = "deepseek-r1";
            OllamaClient = new(new()
            {
                HttpClient = OllamaHttpClient,
                ServerIP = IP,
                ModelName = ModelName,
            });
            OllamaServicess = new(new()
            {
                HttpClient = OllamaHttpClient,
                ServerIP = IP,
            });
        }
        public override async Task ChatAsync(ChatRequestMessageModel Message, Action<ChatResponseStreamMessageModel> Response, CancellationToken? CancelToken = null)
        {
            OllamaResponses.OllamaServiceTagsResponseModel.TagModelInfo CustomModel = null;
            if (Message.CustomChatType.Data is OllamaResponses.OllamaServiceTagsResponseModel.TagModelInfo modelInfo)
            {
                CustomModel = modelInfo;
            }
            var msg = Message.Messages.Select(s => new OlllamaChatRoleMessage()
            {
                Content = s.Message,
                Role = s.Sender,
            }).ToList();
            msg.Add(new OlllamaChatRoleMessage()
            {
                Content = Message.SendContent,
                Role = "user",
            });
            await OllamaClient.RequestMessageAsync(new OllamaChatRequestModel()
            {
                Messages = msg,
                Model = CustomModel?.Model ?? ModelName,
                Stream = true,
            }, (msg) =>
            {
                Response(new()
                {
                    Message = msg.Message.Content
                });
            }, CancelToken);

        }

        public override Task<ChatResponseMessageModel> ChatAsync(ChatRequestMessageModel Message, CancellationToken? CancelToken = null)
        {

            throw new();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<ChatTypeItemModel>> GetSupportedTypes()
        {
            var data = await OllamaServicess.RequestTagsAsync();

            return data.Models.Select(s => new ChatTypeItemModel()
            {
                Data = s,
                Title = s.Name
            });
        }
    }
}
