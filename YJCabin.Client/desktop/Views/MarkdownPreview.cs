using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ColorDocument.Avalonia;
using ColorDocument.Avalonia.DocumentElements;
using Markdown.Avalonia;
using Markdown.Avalonia.Parsers;
using Markdown.Avalonia.Plugins;

namespace YJCabin.Desktop.Views;

public sealed class MarkdownPreview : MarkdownScrollViewer
{
    public MarkdownPreview()
    {
        var plugins = new MdAvPlugins();
        plugins.Plugins.Add(new ReaderCodePlugin());
        Plugins = plugins;
    }
}

file sealed class ReaderCodePlugin : IMdAvPlugin
{
    public void Setup(SetupInfo info)
    {
        info.EnablePreRenderingCodeBlock = true;
        info.Register(new ReaderFencedCodeOverride());
        info.Register(new ReaderIndentCodeOverride());
    }
}

file sealed class ReaderFencedCodeOverride : BlockOverride2
{
    public ReaderFencedCodeOverride() : base("CodeBlocksWithLangEvaluator")
    {
    }

    public override IEnumerable<DocumentElement>? Convert2(
        string text,
        Match match,
        ParseStatus status,
        IMarkdownEngine2 engine,
        out int parseTextBegin,
        out int parseTextEnd)
    {
        var lang = match.Groups[2].Value.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
        var fence = Regex.Escape(match.Groups[1].Value);
        var close = new Regex($@"\n[ ]*{fence}[ ]*(?:\n|$)").Match(text, match.Index + match.Length);

        parseTextBegin = match.Index;
        if (close.Success)
        {
            parseTextEnd = close.Index + close.Length;
            return Wrap(lang, text[(match.Index + match.Length)..close.Index]);
        }

        parseTextEnd = text.Length;
        return Wrap(lang, text[(match.Index + match.Length)..]);

        static DocumentElement[] Wrap(string language, string code) =>
            [new UnBlockElement(ReaderCodeBlock.Create(language, code))];
    }
}

file sealed class ReaderIndentCodeOverride : BlockOverride2
{
    public ReaderIndentCodeOverride() : base("CodeBlocksWithoutLangEvaluator")
    {
    }

    public override IEnumerable<DocumentElement>? Convert2(
        string text,
        Match match,
        ParseStatus status,
        IMarkdownEngine2 engine,
        out int parseTextBegin,
        out int parseTextEnd)
    {
        parseTextBegin = match.Index;
        parseTextEnd = match.Index + match.Length;
        var code = string.Join(
            "\n",
            match.Groups[1].Value.Split('\n').Select(static line =>
                line.StartsWith("    ", StringComparison.Ordinal) ? line[4..] :
                line.StartsWith('\t') ? line[1..] :
                line));
        code = Regex.Replace(code, @"^\n+|\n+\z", "");
        return new DocumentElement[] { new UnBlockElement(ReaderCodeBlock.Create("", code)) };
    }
}

file static class ReaderCodeBlock
{
    public static Control Create(string lang, string code)
    {
        var copy = new Button
        {
            Content = "复制",
            Classes = { "md-code-copy" },
            HorizontalAlignment = HorizontalAlignment.Right,
            MinHeight = 28
        };
        copy.Click += async (_, _) =>
        {
            var clipboard = TopLevel.GetTopLevel(copy)?.Clipboard;
            if (clipboard is null)
            {
                return;
            }

            await clipboard.SetTextAsync(code);
            copy.Content = "已复制";
            DispatcherTimer.RunOnce(() => copy.Content = "复制", TimeSpan.FromMilliseconds(1600));
        };

        var bar = new DockPanel();
        DockPanel.SetDock(copy, Dock.Right);
        bar.Children.Add(copy);
        bar.Children.Add(new TextBlock
        {
            Classes = { "md-code-lang" },
            Text = ReaderCodeHighlight.Label(lang).ToUpperInvariant(),
            VerticalAlignment = VerticalAlignment.Center
        });

        var body = new SelectableTextBlock
        {
            Classes = { "md-code-text" },
            TextWrapping = TextWrapping.NoWrap
        };
        foreach (var run in ReaderCodeHighlight.Tokenize(code))
        {
            body.Inlines!.Add(run);
        }

        return new Border
        {
            Classes = { "md-code" },
            Child = new StackPanel
            {
                Children =
                {
                    new Border
                    {
                        Classes = { "md-code-bar" },
                        Child = bar
                    },
                    new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        Content = body
                    }
                }
            }
        };
    }
}

file static class ReaderCodeHighlight
{
    private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bash"] = "Bash",
        ["c"] = "C",
        ["cpp"] = "C++",
        ["csharp"] = "C#",
        ["cs"] = "C#",
        ["css"] = "CSS",
        ["dockerfile"] = "Dockerfile",
        ["go"] = "Go",
        ["html"] = "HTML",
        ["java"] = "Java",
        ["javascript"] = "JavaScript",
        ["js"] = "JavaScript",
        ["json"] = "JSON",
        ["jsx"] = "JSX",
        ["kotlin"] = "Kotlin",
        ["markdown"] = "Markdown",
        ["md"] = "Markdown",
        ["php"] = "PHP",
        ["plaintext"] = "Text",
        ["powershell"] = "PowerShell",
        ["ps1"] = "PowerShell",
        ["python"] = "Python",
        ["py"] = "Python",
        ["rust"] = "Rust",
        ["scss"] = "SCSS",
        ["sh"] = "Shell",
        ["shell"] = "Shell",
        ["sql"] = "SQL",
        ["text"] = "Text",
        ["ts"] = "TypeScript",
        ["tsx"] = "TSX",
        ["typescript"] = "TypeScript",
        ["xml"] = "XML",
        ["yaml"] = "YAML",
        ["yml"] = "YAML"
    };

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "and", "as", "async", "await", "break", "case", "catch", "class", "const", "continue",
        "def", "default", "defer", "del", "do", "elif", "else", "enum", "except", "export",
        "extends", "false", "False", "finally", "fn", "for", "from", "function", "go", "if",
        "impl", "import", "in", "interface", "is", "lambda", "let", "mod", "namespace", "new",
        "None", "not", "null", "or", "override", "package", "pass", "private", "protected",
        "pub", "public", "return", "self", "static", "struct", "switch", "this", "throw",
        "true", "True", "try", "type", "typeof", "undefined", "use", "using", "var", "void",
        "while", "with", "yield", "abstract", "base", "bool", "byte", "char", "decimal",
        "double", "float", "int", "internal", "long", "readonly", "ref", "sealed", "short",
        "string", "uint", "ulong", "virtual", "volatile", "where", "select", "join"
    };

    private static readonly Regex Tokens = new(
        """(?<comment>#[^\n]*|//[^\n]*|/\*[\s\S]*?\*/)|(?<string>"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|`(?:\\.|[^`\\])*`)|(?<number>\b\d+(?:\.\d+)?\b)|(?<word>\b[A-Za-z_][\w]*\b)|(?<other>[^#/`'"\w]+)""",
        RegexOptions.Compiled);

    public static string Label(string lang)
    {
        lang = lang.Trim();
        if (lang.Length == 0)
        {
            return "Code";
        }

        return Labels.TryGetValue(lang, out var label) ? label : lang;
    }

    public static IEnumerable<Run> Tokenize(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            yield return new Run("");
            yield break;
        }

        var index = 0;
        foreach (Match token in Tokens.Matches(code))
        {
            if (token.Index > index)
            {
                yield return new Run(code[index..token.Index]) { Foreground = Brush("#E8F4FF") };
            }

            var run = new Run(token.Value);
            if (token.Groups["comment"].Success)
            {
                run.Foreground = Brush("#6D86A8");
                run.FontStyle = FontStyle.Italic;
            }
            else if (token.Groups["string"].Success)
            {
                run.Foreground = Brush("#9EE4C6");
            }
            else if (token.Groups["number"].Success)
            {
                run.Foreground = Brush("#B8E7FF");
            }
            else if (token.Groups["word"].Success && Keywords.Contains(token.Value))
            {
                run.Foreground = Brush("#7ECFFF");
            }
            else
            {
                run.Foreground = Brush("#E8F4FF");
            }

            yield return run;
            index = token.Index + token.Length;
        }

        if (index < code.Length)
        {
            yield return new Run(code[index..]) { Foreground = Brush("#E8F4FF") };
        }
    }

    private static IBrush Brush(string hex) => SolidColorBrush.Parse(hex);
}
