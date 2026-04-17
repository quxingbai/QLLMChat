## 这是什么

<div style="width: 90%;   padding: 20px;">
    <div style="display: inline-block; background-color: whitesmoke; margin: 5px; padding: 5px 15px; text-align: center;  border-radius: 8px;">
        Ollama本地模型
    </div>
    <div style="display: inline-block; background-color: whitesmoke; margin: 5px; padding: 5px 15px; text-align: center;  border-radius: 8px;">
        本地大模型对话客户端
    </div>
    <div style="display: inline-block; background-color: whitesmoke; margin: 5px; padding: 5px 15px; text-align: center;  border-radius: 8px;">
        WPF代码 生成+预览
    </div>
    <div style="display: inline-block; background-color: whitesmoke; margin: 5px; padding: 5px 15px; text-align: center;  border-radius: 8px;">
        自定义System提示词
    </div>
    <div style="display: inline-block; background-color: whitesmoke; margin: 5px; padding: 5px 15px; text-align: center;  border-radius: 8px;">
        自定义富文本
    </div>
</div>

https://github.com/quxingbai/QLLMChat

###示例
测试下来Coder模型更好用一些
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417111647713-1725215055.png)

##使用
目前支持**直接输出WPF UI** 也可以切换到代码查看
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417112623345-185298032.png)
前提是模型输出的内容是正确的。
它们训练可能使用了大量的Html，然后让它输出WPF控件的时候就很容易搞混某些属性，圆角 或者默认显示的问题本之类的。

它的原理就是通过**提示词** 让模型必须输出那些指定的**格式**
就像这样 它每次输出需要渲染的Xaml代码都会被 **QLLMRender** 包裹，然后Type指定代码类型
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417112843862-255653998.png)

![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417114644266-1348615423.png)

比如说到一些难实现的功能，它就有很大概率失败...
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417115139785-318670186.png)
因为代码是这么写的....
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417115158596-1857297370.png)
就因为提示词里的这一句。所以说目前的难点在于提示词的作用
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417115943413-1897734803.png)

所以很多时候都需要一些**模板**来约束输出的内容
就像个颜色输出模板
不过模板多了又会消耗上下文的Token数量...
![image](https://img2024.cnblogs.com/blog/1997690/202604/1997690-20260417124335298-832182738.png)


### 具体实现上
由于模型输出都是Markdown，所以引用了 `Markdig.Wpf.MarkdownViewer` 的Markdown渲染。
但是由于需要渲染WPF 或者其他的什么东西，所以就得重写某一部分功能。
这里的重写后的主体代码是Copilot写的 很好用，稍微有些细节 渲染失败后怎么办，渲染出来的东西如何放置稍微改一下就跑起来了。

```c#
   private string HandleMarkdownChanged(string markdown)
   {
       CancelPendingInject();

       if (markdown is null)
           markdown = string.Empty;

       // 匹配所有完整的 <QLLMRender ...>...</QLLMRender> 块
       var matches = Regex.Matches(markdown, @"<QLLMRender\b([^>]*)>(.*?)</QLLMRender>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
       var renderItems = new List<(string Id, RenderItem Item)>();
       var sanitizedBuilder = markdown;
       int indexShift = 0; // not strictly needed since we do Replace with Regex later

       int idCounter = 0;
       foreach (Match match in matches)
...
```



###配置
在APP的Startup里进行了依赖注入，实现几个关键接口就能替换目前的数据源

```c#
        protected override void OnStartup(StartupEventArgs e)
        {
            var service = new ServiceCollection();
            //这里是数据库的注入，可以根据需要替换成其他数据库实现
            //比如目前除了Json还有一个内存数据库MemoryDatabase，后续也可以添加其他数据库实现
            service.AddSingleton<IChatDataBase, JsonDatabase>(p =>
            {
                return new JsonDatabase("./chatdata.json");
            });
            //这里是聊天模型的注入，可以根据需要替换成其他聊天模型实现，实现了IMultiChatTypes接口的聊天模型会在界面上显示不同的模型，
            // public class OllamaChat : IChatModel, IMultiChatTypes
            service.AddSingleton<IChatModel, OllamaChat>();
            service.AddSingleton<IDispatcherProvider, AppDispatcher>();
            service.AddTransient<MainWindowViewModel>();
            service.AddTransient<MainChatPageViewModel>();
            service.AddTransient<MainWindow>();

            ServiceProvider serviceProvider = service.BuildServiceProvider();
            var window = serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
            base.OnStartup(e);
        }
```
