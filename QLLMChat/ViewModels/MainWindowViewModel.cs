using Microsoft.Extensions.DependencyInjection;
using QLLMChat.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLLMChat.ViewModels
{
    public class MainWindowViewModel
    {
        public MainChatPageViewModel MainChatPageViewModel { get; set; }
        public MainWindowViewModel(IServiceProvider ServideProvider)
        {
            //string md = "##问题\r\n我的电脑**显卡内存只有8G**，但每次运行OpenClaw `ollama launch openclaw` 都会自动把上下文大小调整为**32768**，所导致的结果就是 Size超过了我显卡的内存，然后有**一部分任务分给了CPU**，导致输出内容很缓慢。\r\n\r\n被修改的 32768\r\n![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260407120601166-816448247.png)\r\n\r\n预想中的 16384\r\n![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260407120945744-298874653.png)\r\n\r\n问题是新装的OpenClaw **Skill很少** 根本不需要太多上下文，我必须优先保证输出顺畅 所以就得调整上下文减小体积。\r\n\r\n### 无效方案\r\n想到问题可能出在上下文就开始找了，结果网上说可以修改**openclaw.json配置文件**，Windows的话会在`C:\\Users\\用户名\\.openclaw`这个路径下。\r\n进去后发现确实有个**contextWindow：32768**，寻思这改了就应该能行了。\r\n结果 修改 保存 运行 又给我自动改成32768了....\r\n\r\n![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260407121641524-1000205270.png)\r\n\r\n再就是问AI，结果说的方案多少有些超模了，说什么创建一个16k的模型文件啥的，给我的代码一运行就错...最后也放弃问了。\r\n\r\n##解决方案\r\n`openclaw config set models.providers.ollama.models[0].contextWindow 16384`\r\n我发现好像 config **可以set Json路径**，就先get了一下 顺藤摸瓜找到这个模型的位置，然后set试了一下没想到还真给改成16k了...\r\n\r\n问了几句话 输出**流畅了**，\r\n![image](https://img2024.cnblogs.com/blog/1997690/202604/1...";
            
            //var chatDataBase= ServideProvider.GetRequiredService<IChatDataBase>();  
            //for (int i = 0; i < 10; i++)
            //{
            //    chatDataBase.AddChatTargetAsync().ContinueWith(w =>
            //    {
            //        var id = w.Result;
            //        for (int ii = 0; ii < Random.Shared.Next(1, 15); ii++)
            //        {
            //            string sender = Random.Shared.Next(0, 2) == 0 ? "User" : "LLModel";
            //            string msg = sender == "User" ? "用户消息..as.dasd." : md;
            //            chatDataBase.AddChatMessageAsync(id, new Models.Entities.ChatTargetMessageModel() { Message = msg, Sender =sender });
            //        }
            //    });
            //}
            MainChatPageViewModel = ServideProvider.GetRequiredService<MainChatPageViewModel>();
        }
    }
}
