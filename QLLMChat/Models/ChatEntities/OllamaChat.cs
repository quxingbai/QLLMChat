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
using System.Windows;
using static MultimodalSharp.Ollama.Models.Entities.OllamaRequests;

namespace QLLMChat.Models.ChatEntities
{
    public class OllamaChat : IChatModel, IMultiChatTypes
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
        public async Task ChatAsync(ChatRequestMessageModel Message, Action<ChatResponseStreamMessageModel> Response, CancellationToken? CancelToken = null)
        {
            CreateMessages(Message, out var msg, out var ModelName);
            await OllamaClient.RequestMessageAsync(new OllamaChatRequestModel()
            {
                Messages = msg,
                Model = ModelName,
                Stream = true,
            }, (msg) =>
            {
                Response(new()
                {
                    Message = msg.Message.Content
                });
            }, CancelToken);

        }

        public async Task<ChatResponseMessageModel> ChatAsync(ChatRequestMessageModel Message, CancellationToken? CancelToken = null)
        {
            CreateMessages(Message, out var msg, out var ModelName);
            var data = await OllamaClient.RequestMessageAsync(new OllamaChatRequestModel()
            {
                Messages = msg,
                Model = ModelName
            });
            return new()
            {
                Message = data.Message.Content
            };
        }
        private void CreateMessages(ChatRequestMessageModel Message, out List<OlllamaChatRoleMessage> CreatedMessage, out string ModelName)
        {
            var msg = Message.Messages.Select(s => new OlllamaChatRoleMessage()
            {
                Content = s.Message,
                Role = s.Sender,
            }).ToList();
            var text = Clipboard.GetText();
            msg.Add(new OlllamaChatRoleMessage()
            {
                Content = Message.SendContent,
                Role = GetUserMessageName(),
            });
            msg.Add(new OlllamaChatRoleMessage()
            {
                Content= "你是 WPF 专家。当你输出需要渲染的 WPF 代码时，必须用 <QLLMRender Type=\"WPF\"></QLLMRender> 包裹。\r\n\r\n核心规则：\r\n- 不允许直接输出无法独立呈现的元素（如 Style、Trigger、DataTemplate、ControlTemplate、Setter 等）。\r\n- 必须将它们附着在一个合法的、可直接渲染的 UI 元素上，例如 Border、Grid、StackPanel、Button、TextBlock 等。\r\n- **严禁使用 Window、Page 或任何顶级窗口元素作为父容器。**\r\n\r\n具体做法：\r\n1. 如果内容是颜色或渐变 → 放到 Border 的 Background 中。\r\n2. 如果内容是单个元素 → 直接写该元素。\r\n3. 如果内容是多个元素 → 用 Grid 或 StackPanel 等布局容器包裹。\r\n4. 如果内容是 Style / Template / 资源字典 → 必须内嵌在某个元素的资源中，并确保该元素本身存在且可见。例如：\r\n   - <Border><Border.Resources><Style TargetType=\"Button\">...</Style></Border.Resources><Button Content=\"测试\"/></Border>\r\n   - 或 <Grid><Grid.Resources>...</Grid.Resources><Button Content=\"应用样式的按钮\"/></Grid>\r\n5. 所有输出的根元素必须是以下之一（或类似的非窗口容器）：\r\n   - Border, Grid, StackPanel, WrapPanel, DockPanel, UniformGrid\r\n   - Canvas, ScrollViewer, GroupBox, Expander, TabControl（内部需包含有效内容）\r\n   - 任何 ContentControl 或 ItemsControl 的派生类（如 Button、Label、ListBox 等）\r\n\r\n不允许出现：\r\n- ❌ <Window>\r\n- ❌ <Page>\r\n- ❌ <NavigationWindow>\r\n- ❌ 没有视觉外壳的 XAML 片段\r\n\r\n所有输出必须能被直接放入渲染器并显示。\r\n\r\n示例：\r\n✅ 正确...",
                Role = "system",
            });
            CreatedMessage = msg;
            ModelName = Message.CustomChatType?.Model ?? this.ModelName;
        }


        public async Task<IEnumerable<ChatTypeItemModel>> GetSupportedTypes()
        {
            var data = await OllamaServicess.RequestTagsAsync();

            return data.Models.Select(s => new ChatTypeItemModel()
            {
                Size = s.Size,
                Model = s.Model,
                Name = s.Name,
                Title = s.Name,
                Text = ($"模型：{s.Model} | 大小：{s.Size}")
            });
        }

        public string GetUserMessageName()
        {
            return "user";
        }
    }
}
