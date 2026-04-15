using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Markdig.Wpf;

namespace QLLMChat.Views.CControls
{
    public class WpfElementRenderBox : ContentControl
    {
        private readonly MarkdownViewer _markdownViewer;
        private readonly StackPanel _elementsHost;

        static WpfElementRenderBox()
        {
            // 如需自定义样式模板可在 Generic.xaml 中提供并取消下面注释
            // DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfElementRenderBox), new FrameworkPropertyMetadata(typeof(WpfElementRenderBox)));
        }

        public WpfElementRenderBox()
        {
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _markdownViewer = new MarkdownViewer();
            Grid.SetRow(_markdownViewer, 0);

            _elementsHost = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(4)
            };
            Grid.SetRow(_elementsHost, 1);

            root.Children.Add(_markdownViewer);
            root.Children.Add(_elementsHost);

            this.Content = root;
        }

        // Markdown 依赖属性（仅显示非渲染块内容）
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(
                nameof(Markdown),
                typeof(string),
                typeof(WpfElementRenderBox),
                new PropertyMetadata(string.Empty, OnMarkdownChanged));

        public string Markdown
        {
            get => (string)GetValue(MarkdownProperty);
            set => SetValue(MarkdownProperty, value);
        }

        private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WpfElementRenderBox box)
            {
                box.UpdateFromMarkdown(e.NewValue as string ?? string.Empty);
            }
        }

        private void UpdateFromMarkdown(string markdown)
        {
            _elementsHost.Children.Clear();

            if (string.IsNullOrEmpty(markdown))
            {
                _markdownViewer.Markdown = string.Empty;
                return;
            }

            // 提取 <QLLMRender ...> ... </QLLMRender> 块
            var matches = Regex.Matches(markdown, @"<QLLMRender\b([^>]*)>(.*?)</QLLMRender>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var attrText = match.Groups[1].Value;
                var inner = match.Groups[2].Value?.Trim() ?? string.Empty;

                // 提取 Type 属性（支持单/双引号）
                var typeMatch = Regex.Match(attrText, @"\bType\s*=\s*(['""])(.*?)\1", RegexOptions.IgnoreCase);
                var type = typeMatch.Success ? typeMatch.Groups[2].Value : string.Empty;

                if (string.Equals(type, "WPF", StringComparison.OrdinalIgnoreCase))
                {
                    var element = TryParseXamlFragment(inner);
                    if (element != null)
                    {
                        _elementsHost.Children.Add(element);
                    }
                    else
                    {
                        _elementsHost.Children.Add(CreateFallbackTextBlock(inner));
                    }
                }
                else
                {
                    // 非 WPF 类型，作为提示文本显示（可扩展其他渲染类型）
                    var info = $"[QLLMRender Type=\"{type}\"] 内容（未渲染）";
                    _elementsHost.Children.Add(new TextBlock
                    {
                        Text = info,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(2),
                        Foreground = Brushes.Gray
                    });
                }
            }

            // 移除所有 QLLMRender 块，仅把剩余内容交给 MarkdownViewer 显示
            var sanitized = Regex.Replace(markdown, @"<QLLMRender\b[^>]*>.*?</QLLMRender>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            _markdownViewer.Markdown = sanitized;
        }

        // 尝试将 XAML 片段解析为 UIElement。若片段缺少默认 presentation 命名空间，会自动包装补上。
        private UIElement? TryParseXamlFragment(string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment))
                return null;

            try
            {
                // 检查片段中是否包含 presentation 命名空间声明（默认命名空间）
                var hasPresentationNs = Regex.IsMatch(fragment, @"http://schemas\.microsoft\.com/winfx/2006/xaml/presentation", RegexOptions.IgnoreCase);

                // 尝试直接解析（片段本身可能是完整单根元素并包含命名空间）
                if (hasPresentationNs)
                {
                    try
                    {
                        var obj = XamlReader.Parse(fragment);
                        if (obj is UIElement ui) return ui;
                        if (obj is Panel p)
                        {
                            var container = new StackPanel();
                            foreach (UIElement child in p.Children)
                                container.Children.Add(child);
                            return container;
                        }
                    }
                    catch
                    {
                        // 继续尝试包装解析
                    }
                }

                // 包装片段，确保默认命名空间和 x 命名空间存在
                var wrapper =
                    "<StackPanel xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                    "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
                    fragment +
                    "</StackPanel>";

                var wrappedObj = XamlReader.Parse(wrapper);
                if (wrappedObj is Panel wrappedPanel)
                {
                    // 如果只有一个子元素且本身是 UIElement，返回该元素（保持原意）
                    if (wrappedPanel.Children.Count == 1 && wrappedPanel.Children[0] is UIElement single)
                    {
                        wrappedPanel.Children.RemoveAt(0);
                        return single;
                    }

                    // 否则返回一个 StackPanel 容器包含这些子元素
                    var container = new StackPanel();
                    foreach (UIElement child in wrappedPanel.Children)
                        container.Children.Add(child);
                    return container;
                }
            }
            catch
            {
                // 解析失败回退 null
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
    }
}