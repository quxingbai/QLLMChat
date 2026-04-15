using Markdig;
using Markdig.Wpf;
using QLLMChat.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace QLLMChat.Views.CControls
{
    // 直接继承 Markdig.Wpf.MarkdownViewer，操作其 Document 内容以注入由 <QLLMRender Type="WPF">...</QLLMRender> 生成的 WPF UI 元素。
    public class CustomRenderMarkdownViewer : MarkdownViewer
    {





        // 跟踪注入到 Document 的 BlockUIContainer
        private readonly List<BlockUIContainer> _injectedBlockContainers = new();

        // 跟踪注入到 Document 的 InlineUIContainer（以及它所在的 Paragraph）
        private readonly List<(Paragraph Parent, Inline InlineContainer)> _injectedInlineContainers = new();

        // 缓存：content -> 原型 UIElement（LRU）
        private readonly Dictionary<string, CacheEntry> _cache = new(StringComparer.Ordinal);
        private readonly LinkedList<string> _lru = new();
        private const int CacheCapacity = 50;

        private int _lastProcessedCompleteCount;
        private string _lastSanitizedMarkdown = string.Empty;
        private DispatcherOperation? _pendingInjectOperation;

        static CustomRenderMarkdownViewer()
        {
            // 如需自定义样式可取消以下行并在 Generic.xaml 中提供样式
            // DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfElementRenderBox), new FrameworkPropertyMetadata(typeof(WpfElementRenderBox)));
            MatchCollection OldTextQLLMRenderCodes = null;
            //string oldText = string.Empty;
            MarkdownProperty.OverrideMetadata(typeof(CustomRenderMarkdownViewer), new FrameworkPropertyMetadata("", new PropertyChangedCallback((d, t) =>
            {

            }), (coerce, c) =>
            {
                //var self = coerce as CustomRenderMarkdownViewer;
                //string newText = c.ToString();
                //if (newText.Length < oldText.Length)
                //{
                //    return self.HandleMarkdownChanged(newText);
                //}
                //else
                //{

                //    //string changeText = oldText.Length == 0 ? newText : oldText[0] != newText[0] ? newText : newText.Substring(oldText.Length, newText.Length - oldText.Length);
                //    var nl = newText.Length - oldText.Length;
                //    string changeText = oldText.Length == 0 ? newText : oldText[0] != newText[0] ? newText : newText[^nl..];

                //    if (!changeText.Contains('>'))
                //    {
                //        oldText = newText;
                //        return self.Markdown + changeText;
                //    }
                //    else
                //    {
                //        if (OldTextQLLMRenderCodes == null) OldTextQLLMRenderCodes = GetQLLMRenderCodes(oldText);


                //        var newQLLrenderCodes = GetQLLMRenderCodes(newText);
                //        var isCustomRenderTargetChanged = OldTextQLLMRenderCodes.Count != newQLLrenderCodes.Count;

                //        if (!isCustomRenderTargetChanged)
                //            for (int i = 0; i < OldTextQLLMRenderCodes.Count; i++)
                //            {
                //                var Omath = OldTextQLLMRenderCodes[i];
                //                var Nmath = newQLLrenderCodes[i];
                //                if (Omath.Value != Nmath.Value)
                //                {
                //                    isCustomRenderTargetChanged = true;
                //                    break;
                //                }
                //            }

                //        oldText = newText;
                //        //如果真的绘制项目变化了就重新绘制
                //        if (isCustomRenderTargetChanged)
                //        {
                //            OldTextQLLMRenderCodes = null;
                //            return self.HandleMarkdownChanged(newText);
                //        }
                //        else
                //        {
                //            //否则直接追加
                //            return self.Markdown + changeText;
                //        }
                //}

                //}

                var self = coerce as CustomRenderMarkdownViewer;
                ControlHelper.SetObjectToSelfDictionary(self, "NewText", c.ToString());

                string newText = ControlHelper.GetObjectInSelfDictionary(self, "NewText")?.ToString() ?? string.Empty;
                string oldText = ControlHelper.GetObjectInSelfDictionary(self, "OldText")?.ToString() ?? string.Empty;

                string change = "";

                if (oldText == "") change = newText;
                else if (newText.Length < oldText.Length) change = newText;
                else if (oldText[..Math.Min(1, oldText.Length)] == newText[..Math.Min(1, newText.Length)]) change = newText[(oldText.Length)..];

                ControlHelper.SetObjectToSelfDictionary(self, "OldText", newText);

                string changeWith = oldText[^Math.Min(13, oldText.Length)..] + change;
                //Debug.WriteLine(changeWith);
                //if (changeWith.Contains("</QLLMRender>"))


                //这就说明最后一次写入了
                if (newText == oldText)
                {
                    return self.HandleMarkdownChanged(newText);
                    //var a = 1;
                }

                if (change.Contains('>') || newText.Length < oldText.Length)
                {
                    return self.HandleMarkdownChanged(newText);
                }
                else
                {
                    if (self.Document == null)
                    {
                        return self.Markdown + change;
                    }
                    else
                    {
                        if (self.Document.Blocks.LastBlock is Paragraph par)
                        {
                            if (par.Inlines.LastInline is Run run)
                            {
                                run.Text += change;
                            }
                            else
                            {
                                par.Inlines.Add(new Run()
                                {
                                    Text = change
                                });
                            }
                        }
                        else
                        {
                            var par1 = new Paragraph();
                            par1.Inlines.Add(new Run() { Text = change });
                            self.Document.Blocks.Add(par1);
                        }
                        return self.Markdown;
                    }

                }
                //void mathChange()
                //{
                //    ControlHelper.SetObjectToSelfDictionary(self, "OldText",);
                //}
            }));
        }

        public CustomRenderMarkdownViewer()
        {
            // 默认构造

        }

        // 对外提供清理缓存的接口
        public void ClearRenderCache()
        {
            CancelPendingInject();
            _cache.Clear();
            _lru.Clear();
        }
        private static MatchCollection GetQLLMRenderCodes(string markdown)
        {
            var matches = Regex.Matches(markdown, @"<QLLMRender\b([^>]*)>(.*?)</QLLMRender>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return matches;
        }
        // 处理 Markdown 变更：抽取 QLLMRender 块、在 Markdown 中用占位符替换，基类渲染后把 UI 元素按占位符位置注入
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
            {
                var attrText = match.Groups[1].Value;
                var inner = match.Groups[2].Value?.Trim() ?? string.Empty;
                var typeMatch = Regex.Match(attrText, @"\bType\s*=\s*(['""])(.*?)\1", RegexOptions.IgnoreCase);
                var type = typeMatch.Success ? typeMatch.Groups[2].Value : string.Empty;

                var id = $"QLLM_RENDER_{++idCounter}_{Guid.NewGuid():N}";
                renderItems.Add((id, new RenderItem { Type = type, Content = inner }));

                // 使用唯一占位符替换原始块（使用可识别且不太可能冲突的格式）
                // 我们统一用 [[ID]] 形式，Markdig 会作为普通文本渲染出 Run
            }

            // 用 Regex.Replace 把每个匹配替换为占位符（按顺序）
            int replaceIndex = 0;
            sanitizedBuilder = Regex.Replace(markdown, @"<QLLMRender\b([^>]*)>(.*?)</QLLMRender>", m =>
            {
                replaceIndex++;
                var id = renderItems[replaceIndex - 1].Id;
                return "{{" + id + "}}";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var sanitized = sanitizedBuilder;
            // 始终把“已移除渲染块”的 Markdown 交给基类渲染（保证 Markdown 区域尽可能实时）
            //base.Markdown = sanitized;
            var currentCompleteCount = matches.Count;

            // 快速路径：无完整块且之前也无注入，直接更新记录并返回
            if (currentCompleteCount == 0 && _lastProcessedCompleteCount == 0)
            {
                _lastSanitizedMarkdown = sanitized;
                return sanitized;
            }

            // 如果完整块数减少，马上移除旧注入（保持文档一致）
            if (currentCompleteCount < _lastProcessedCompleteCount)
            {
                RemoveInjectedItems();
                _lastProcessedCompleteCount = currentCompleteCount;
                _lastSanitizedMarkdown = sanitized;
            }

            // 只有在完整块数量或清理后的 Markdown 发生变化时才安排注入
            var needScheduleInject = currentCompleteCount != _lastProcessedCompleteCount || sanitized != _lastSanitizedMarkdown;

            if (currentCompleteCount > 0 && needScheduleInject)
            {
                // 调度注入，确保在基类渲染 Document 后执行
                _pendingInjectOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    try
                    {
                        // 首先移除旧注入（如果有）
                        RemoveInjectedItems();

                        // 对每个占位符，按其在 Document 中的位置注入对应元素
                        foreach (var pair in renderItems)
                        {
                            TryInjectAtPlaceholder(pair.Id, pair.Item);
                        }

                        _lastProcessedCompleteCount = currentCompleteCount;
                        _lastSanitizedMarkdown = sanitized;
                    }
                    finally
                    {
                        _pendingInjectOperation = null;
                    }
                }));

            }
            else
            {
                _lastSanitizedMarkdown = sanitized;
            }
            return sanitized;
        }

        private void CancelPendingInject()
        {
            if (_pendingInjectOperation != null && _pendingInjectOperation.Status == DispatcherOperationStatus.Pending)
            {
                try
                {
                    _pendingInjectOperation.Abort();
                }
                catch
                {
                    // 忽略
                }
                finally
                {
                    _pendingInjectOperation = null;
                }
            }
        }

        // 根据占位符 id 在 Document 中查找对应 Run 并注入元素（保持原始位置）
        private void TryInjectAtPlaceholder(string placeholderId, RenderItem item)
        {
            var doc = this.Document;
            if (doc == null)
                return;

            var placeholderText = "{{" + placeholderId + "}}";

            // 遍历段落寻找 Run 包含占位符
            foreach (var block in doc.Blocks.ToList())
            {
                if (block is Paragraph para)
                {
                    // 找到包含占位符的 Run（可能被拆分为多个 Run，先用 string search）
                    Run targetRun = null;
                    Inline foundInline = null;

                    foreach (var inline in para.Inlines.ToList())
                    {
                        if (inline is Run run && run.Text != null && run.Text.Contains(placeholderText))
                        {
                            targetRun = run;
                            foundInline = inline;
                            break;
                        }
                    }
                    if (targetRun != null)
                    {
                        // 如果 run.Text 包含其他文本，需要拆分 Run：prefix, placeholder, suffix
                        var txt = targetRun.Text;
                        var idx = txt.IndexOf(placeholderText, StringComparison.Ordinal);
                        var prefix = txt.Substring(0, idx);
                        var suffix = txt.Substring(idx + placeholderText.Length);

                        // 构造新的运行序列
                        var beforeRun = string.IsNullOrEmpty(prefix) ? null : new Run(prefix);
                        var afterRun = string.IsNullOrEmpty(suffix) ? null : new Run(suffix);

                        // 插入 beforeRun (如果有)
                        if (beforeRun != null)
                            para.Inlines.InsertBefore(targetRun, beforeRun);

                        // 决定注入 InlineUIContainer 还是 BlockUIContainer：
                        // - 如果该 Paragraph 中除了占位符没有其他内容(即原 run 是唯一 Inline 或前后都为空)，将其替换为 Block（块级）；
                        // - 否则以 Inline 形式插入保持行内位置。
                        var onlyThisInline = (para.Inlines.Count == 1) ||
                                             (para.Inlines.Count == 2 && beforeRun != null && afterRun == null) ||
                                             (para.Inlines.Count == 2 && beforeRun == null && afterRun != null) ||
                                             (para.Inlines.Count == 3 && beforeRun != null && afterRun != null);

                        var element = CreateOrGetElement(item.Content);
                        CustomRenderCard renderCard = new();
                        renderCard.CodeLanguage = item.Type;
                        renderCard.SourceCode = item.Content;

                        if (element != null)
                        {
                            if (onlyThisInline && string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
                            {
                                renderCard.Content = element;
                                // 整段替换为块级 UI
                                var blockContainer = new BlockUIContainer
                                {
                                    Child = renderCard,
                                    Margin = new Thickness(4)
                                };

                                // 插入块并移除段落
                                doc.Blocks.InsertAfter(para, blockContainer);
                                doc.Blocks.Remove(para);

                                _injectedBlockContainers.Add(blockContainer);
                            }
                            else
                            {
                                renderCard.Content = element;
                                // 以 InlineUIContainer 插入到当前段落的位置
                                var inlineUi = new InlineUIContainer
                                {
                                    Child = renderCard,
                                };

                                para.Inlines.InsertBefore(targetRun, inlineUi);
                                // 记录以便后续移除
                                _injectedInlineContainers.Add((para, inlineUi));

                                // 插入 afterRun 并移除原 run
                                if (afterRun != null)
                                    para.Inlines.InsertAfter(inlineUi, afterRun);
                            }
                        }
                        else
                        {
                            // 解析失败，则用文本提示代替
                            var tb = CreateFallbackTextBlock(item.Content);
                            renderCard.Content = tb;
                            var blockContainer = new BlockUIContainer
                            {
                                Child = renderCard,
                                Margin = new Thickness(4)
                            };

                            doc.Blocks.InsertAfter(para, blockContainer);
                            doc.Blocks.Remove(para);
                            _injectedBlockContainers.Add(blockContainer);
                        }
                        //if (element != null)
                        //{
                        //    if (onlyThisInline && string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
                        //    {
                        //        // 整段替换为块级 UI
                        //        var blockContainer = new BlockUIContainer
                        //        {
                        //            Child = element,
                        //            Margin = new Thickness(4)
                        //        };

                        //        // 插入块并移除段落
                        //        doc.Blocks.InsertAfter(para, blockContainer);
                        //        doc.Blocks.Remove(para);

                        //        _injectedBlockContainers.Add(blockContainer);
                        //    }
                        //    else
                        //    {
                        //        // 以 InlineUIContainer 插入到当前段落的位置
                        //        var inlineUi = new InlineUIContainer
                        //        {
                        //            Child = element,
                        //        };

                        //        para.Inlines.InsertBefore(targetRun, inlineUi);
                        //        // 记录以便后续移除
                        //        _injectedInlineContainers.Add((para, inlineUi));

                        //        // 插入 afterRun 并移除原 run
                        //        if (afterRun != null)
                        //            para.Inlines.InsertAfter(inlineUi, afterRun);
                        //    }
                        //}
                        //else
                        //{
                        //    // 解析失败，则用文本提示代替
                        //    var tb = CreateFallbackTextBlock(item.Content);
                        //    var blockContainer = new BlockUIContainer
                        //    {
                        //        Child = tb,
                        //        Margin = new Thickness(4)
                        //    };

                        //    doc.Blocks.InsertAfter(para, blockContainer);
                        //    doc.Blocks.Remove(para);
                        //    _injectedBlockContainers.Add(blockContainer);
                        //}

                        // 移除原始 targetRun（不论如何都要移除）
                        para.Inlines.Remove(targetRun);

                        // 如果我们插入了 before/after runs 但 later doc manipulations changed them, it's OK.
                        // 只注入第一个匹配（防止同段落内重复占位符需要单独逻辑）
                        // 若段落中可能包含多个相同占位符，可在此继续循环查找；当前实现按占位符逐个处理。
                        break;
                    }
                }
                // 也可能占位符渲染在其他 Block 类型中（ListItem 等），这里仅处理 Paragraph。可按需扩展。
            }
        }

        // 移除上次注入的 BlockUIContainer 与 InlineUIContainer 并保留缓存原型
        private void RemoveInjectedItems()
        {
            var doc = this.Document;
            if (doc != null)
            {
                foreach (var b in _injectedBlockContainers.ToList())
                {
                    doc.Blocks.Remove(b);
                }
            }
            _injectedBlockContainers.Clear();

            foreach (var pair in _injectedInlineContainers.ToList())
            {
                try
                {
                    pair.Parent.Inlines.Remove(pair.InlineContainer);
                }
                catch
                {
                    // 忽略可能的错误
                }
            }
            _injectedInlineContainers.Clear();
        }

        // 创建或从缓存中拿到用于注入的 UIElement（若缓存中已有原型且未被挂载则直接复用；若已被挂载则尝试克隆）
        private UIElement? CreateOrGetElement(string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment))
                return null;

            // 使用 fragment 字符串作为缓存键（也可以用 hash）
            var key = fragment;

            if (_cache.TryGetValue(key, out var entry))
            {
                // 更新 LRU
                TouchCacheKey(key);

                var prototype = entry.Prototype;
                // 若原型当前未被挂载（没有 Parent），可以直接复用
                if (prototype != null && VisualTreeHelper.GetParent(prototype) == null && (prototype as FrameworkElement)?.Parent == null)
                {
                    return prototype;
                }

                // 否则尝试克隆原型（通过序列化/反序列化）
                try
                {
                    var xaml = XamlWriter.Save(prototype);
                    var clone = (UIElement)XamlReader.Parse(xaml);
                    return clone;
                }
                catch
                {
                    // 克隆失败，退回到直接解析 fragment
                }
            }

            // 缓存中不存在或克隆失败：尝试把 fragment 解析为 UIElement
            var parsed = TryParseXamlFragment(fragment, out var parsedError);
            if (parsedError != null)
            {

            }
            if (parsed != null)
            {
                // 将第一个解析出的实例作为缓存原型（后续复用/克隆基准）
                AddToCache(key, parsed);
                return parsed;
            }

            return null;
        }

        private void AddToCache(string key, UIElement prototype)
        {
            // 如果已存在，不替换，只 Touch
            if (_cache.ContainsKey(key))
            {
                TouchCacheKey(key);
                return;
            }

            // 控制容量（LRU）
            if (_lru.Count >= CacheCapacity)
            {
                var lastKey = _lru.Last!.Value;
                _lru.RemoveLast();
                _cache.Remove(lastKey);
            }

            var node = _lru.AddFirst(key);
            _cache[key] = new CacheEntry { Prototype = prototype, Node = node };
        }

        private void TouchCacheKey(string key)
        {
            if (_cache.TryGetValue(key, out var e))
            {
                _lru.Remove(e.Node);
                var node = _lru.AddFirst(key);
                e.Node = node;
                _cache[key] = e;
            }
        }

        // 尝试将 XAML 片段解析为 UIElement；若片段缺少默认 presentation 命名空间，会自动包装补上。
        private UIElement? TryParseXamlFragment(string fragment, out Exception? Error)
        {
            Error = null;
            if (string.IsNullOrWhiteSpace(fragment))
                return null;

            try
            {
                // 检查是否包含 presentation 命名空间声明
                var hasPresentationNs = Regex.IsMatch(fragment, @"http://schemas\.microsoft\.com/winfx/2006/xaml/presentation", RegexOptions.IgnoreCase);

                // 如果包含命名空间，尝试直接解析
                if (hasPresentationNs)
                {
                    try
                    {
                        var obj = XamlReader.Parse(fragment);
                        if (obj is UIElement ui) return ui;
                        if (obj is Panel panel)
                        {
                            var container = new StackPanel();
                            foreach (UIElement child in panel.Children)
                                container.Children.Add(child);
                            return container;
                        }
                    }
                    catch
                    {
                        // 继续尝试包装解析
                    }
                }

                // 包装片段，确保默认命名空间和 x 命名空间存在（允许简写元素如 Button）
                var wrapper =
                    "<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                    "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
                    fragment +
                    "</StackPanel>";

                var wrappedObj = XamlReader.Parse(wrapper);
                if (wrappedObj is Panel wrappedPanel)
                {
                    if (wrappedPanel.Children.Count == 1 && wrappedPanel.Children[0] is UIElement single)
                    {
                        wrappedPanel.Children.RemoveAt(0);
                        return single;
                    }

                    var container = new StackPanel();
                    foreach (UIElement child in wrappedPanel.Children)
                        container.Children.Add(child);
                    return container;
                }
            }
            catch (Exception er)
            {
                Error = er;
                TextBox ErrorTextBlock = new();
                ErrorTextBlock.BorderThickness = new(0);
                ErrorTextBlock.Foreground = Brushes.Red;
                ErrorTextBlock.Text = er.Message;
                ErrorTextBlock.TextWrapping = TextWrapping.Wrap;
                return ErrorTextBlock;

                // 解析失败返回 null
            }

            return null;
        }

        private TextBlock CreateFallbackTextBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2),
                Foreground = Brushes.Black
            };
        }

        // 简单的数据结构用于在 Markdown 更改时传递解析结果
        private sealed class RenderItem
        {
            public string Type { get; init; } = string.Empty;
            public string Content { get; init; } = string.Empty;
        }

        private sealed class CacheEntry
        {
            public UIElement Prototype { get; set; } = default!;
            public LinkedListNode<string> Node { get; set; } = default!;
        }
    }
}