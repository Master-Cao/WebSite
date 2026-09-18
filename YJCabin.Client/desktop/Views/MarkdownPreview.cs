using System.Net.Http;
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
using Markdown.Avalonia.Utils;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.Views;

public sealed class MarkdownPreview : MarkdownScrollViewer
{
    public MarkdownPreview()
    {
        var plugins = new MdAvPlugins
        {
            PathResolver = new SiteImagePathResolver()
        };
        plugins.Plugins.Add(new ReaderCodePlugin());
        Plugins = plugins;
        Classes.Add("md-preview");
    }
}

file sealed class SiteImagePathResolver : IPathResolver
{
    private static readonly HttpClient Http = CreateClient();

    public string? AssetPathRoot { private get; set; }

    public IEnumerable<string>? CallerAssemblyNames { private get; set; }

    public async Task<Stream?> ResolveImageResource(string relativeOrAbsolutePath)
    {
        if (LocalMediaStore.TryOpen(relativeOrAbsolutePath, out var local) && local is not null)
        {
            return local;
        }

        if (MediaImageCache.TryOpen(relativeOrAbsolutePath, out var cached) && cached is not null)
        {
            return cached;
        }

        foreach (var url in Candidates(relativeOrAbsolutePath))
        {
            if (MediaImageCache.TryOpen(url.AbsolutePath, out cached) && cached is not null)
            {
                return cached;
            }

            try
            {
                using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (!LooksLikeImage(bytes))
                {
                    continue;
                }

                MediaImageCache.Store(url.AbsolutePath, bytes);
                return new MemoryStream(bytes, writable: false);
            }
            catch
            {
                // try the next candidate
            }
        }

        return null;
    }

    private IEnumerable<Uri> Candidates(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute) &&
            absolute.Scheme is "http" or "https")
        {
            yield return absolute;
            foreach (var alt in Alternates(absolute))
            {
                yield return alt;
            }

            yield break;
        }

        if (!TryCreateBase(out var root))
        {
            yield break;
        }

        var relative = path.TrimStart('/');
        yield return new Uri(root, relative);
        foreach (var alt in Alternates(new Uri(root, relative)))
        {
            yield return alt;
        }

        var file = Path.GetFileName(path.TrimEnd('/'));
        if (!string.IsNullOrEmpty(file))
        {
            yield return new Uri(root, "api/uploads/" + file);
            yield return new Uri(root, "uploads/" + file);
        }
    }

    private static IEnumerable<Uri> Alternates(Uri url)
    {
        var text = url.ToString();
        if (text.Contains("/api/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            yield return new Uri(text.Replace("/api/uploads/", "/uploads/", StringComparison.OrdinalIgnoreCase));
            yield break;
        }

        if (text.Contains("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            yield return new Uri(text.Replace("/uploads/", "/api/uploads/", StringComparison.OrdinalIgnoreCase));
        }
    }

    private bool TryCreateBase(out Uri root)
    {
        var value = AssetPathRoot?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            root = null!;
            return false;
        }

        if (!value.EndsWith('/'))
        {
            value += "/";
        }

        return Uri.TryCreate(value, UriKind.Absolute, out root!);
    }

    private static bool LooksLikeImage(byte[] bytes) =>
        bytes.Length > 12 && (
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 ||
            bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF ||
            bytes[0] == (byte)'R' && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' ||
            bytes[0] == (byte)'B' && bytes[1] == (byte)'M');

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("YJCabin.Desktop");
        return client;
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
