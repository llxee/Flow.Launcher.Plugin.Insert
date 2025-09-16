//#define UsingDebugBlock
#pragma warning disable 1591
using System;
using System.Linq;
using System.Collections.Generic;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.Insert.Model;
using Flow.Launcher.Plugin.SharedCommands;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.Insert.View;

namespace Flow.Launcher.Plugin.Insert
{
    public class Insert : IPlugin, ISettingProvider, IReloadable
    {
        private PluginInitContext _context;
        private IPublicAPI _api;
        private Settings _settings;
        private string[] _templates;
        private string _pluginLocation ;
        //private string template = string.Empty;
        public static string iconPath ;
        public static string warningIconPath ;
        private PluginMetadata Metadata { get; set; }

        public void Init(PluginInitContext context)
        {
            //throw new NotImplementedException(_settings.FormatStrings + "ForTest");
            _context = context;
            _api = context.API;
            _settings = _api.LoadSettingJsonStorage<Settings>();
            Metadata = context.CurrentPluginMetadata;
            //get plugin location
            _pluginLocation = Metadata.PluginDirectory;
            iconPath = @$"{this._pluginLocation}\Images\icon.png";
            warningIconPath = @$"{this._pluginLocation}\Images\warning.png";
            _templates = _settings.FormatStrings
                ?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

#if UsingDebugBlock
            var rs = warningIconPath == @"C:\Users\25563\AppData\Local\FlowLauncher\app-2.0.0\Plugins\Flow.Launcher.Plugin.Insert\Images\warning.png";
            results.Add(new Result
            {
                Title = $"{warningIconPath} {iconPath} {rs}",
                SubTitle = $"SelectedTemplate:'{_selectedTemplate}' Input:'{query.Search}' TemplatesCount:{_templates.Length}",
                IcoPath = iconPath,
                Action = _ => false
            });
#endif
            WarningResult? warning = null;
            var words = string.IsNullOrWhiteSpace(query.Search)
                ? []
                : SplitArgsRespectQuotes(query.Search);
            if (words.Count <= 1)
            {
                var input = query.Search?.Trim() ?? string.Empty;

                foreach (var t in _templates)
                {
                    if (string.IsNullOrEmpty(input) || t.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new Result
                        {
                            Title = t,
                            SubTitle = "Select this template to start filling placeholders",
                            IcoPath = iconPath,
                            Action = _ =>
                            {
                                _api.ChangeQuery($"is \"{t}\"", false); 
                                _api.ReQuery();
                                return false;
                            }
                        });
                    }
                }

                if (results.Count == 0)
                {
                    results.Add(new Result
                    {
                        Title = "No matching templates found",
                        SubTitle = "Please try a different keyword",
                        Action = _ => false
                    });
                }
            }

            else
            {


                string preview = words.First();
                words = words[1..];
                warning = ReplacePlaceholders( words, ref preview);

                #region 生成选项
                // 插入
                results.Add(new Result
                {
                    Title = preview,
                    SubTitle = "Insert into query box",
                    IcoPath = iconPath,
                    Action = _ =>
                    {
                        _api.ChangeQuery(preview, false);
                        return false;
                    }
                });

                // 复制到剪贴板
                results.Add(new Result
                {
                    Title = preview,
                    IcoPath = iconPath,
                    SubTitle = "Copy to clipboard",
                    Action = _ =>
                    {
                        _api.CopyToClipboard(preview);
                        _api.ChangeQuery(string.Empty, false);
                        _api.ReQuery();
                        return true;
                    }
                });
                // 取消
                results.Add(new Result
                {
                    Title = "Cancel",
                    SubTitle = "Cancel and choose another template",
                    IcoPath = iconPath,
                    Action = _ =>
                    {
                        _api.ChangeQuery("is ", false);
                        return false;
                    }
                });
                if (warning is not null)
                {
                    _ = warning.IcoPath == warningIconPath;
                    results.Add(warning);
                }
                #endregion
            }
            return results;
        }
        private static List<string> SplitArgsRespectQuotes(string input)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
                return tokens;

            // 模式说明："(?<q>(?:\\.|[^"])*)"|(?<w>\S+)
            //  - "                                            匹配开头的双引号
            //  - (?<q>(?:\\.|[^"])*)                         具名捕获组q：匹配引号内的内容
            //        \\.                                      匹配转义字符，如 \"、\\ 等
            //        | [^"]                                   或匹配除双引号以外的任意字符
            //        以上作为一个非捕获分组 (?:...)，重复 * 次
            //  - "                                            匹配结束的双引号
            //  - |                                            或者
            //  - (?<w>\S+)                                   具名捕获组w：匹配连续的非空白字符（未被引号包裹的普通词）
            var pattern = "\"(?<q>(?:\\\\.|[^\"])*)\"|(?<w>\\S+)";

            foreach (Match m in Regex.Matches(input, pattern))
            {
                if (m.Groups["q"].Success)
                {
                    // 引号内的内容：还原转义的引号与反斜杠
                    var val = m.Groups["q"].Value
                        .Replace("\\\"", "\"")
                        .Replace("\\\\", "\\");
                    tokens.Add(val);
                }
                else if (m.Groups["w"].Success)
                {
                    tokens.Add(m.Groups["w"].Value);
                }
            }

            return tokens;
        }

        private WarningResult? ReplacePlaceholders(List<string> words, ref string preview)
        {
            WarningResult? warning = null;
            if (Regex.IsMatch(preview, @"\{\d+\}"))
            {
                // 获取数字占位符并排序
                var matches = Regex.Matches(preview, @"\{(\d+)\}");
                var numbers = new SortedSet<int>();
                if (matches.Count == 0)
                    warning = new WarningResult("No valid placeholders found in the template.", "Please check the template format.");
                if (numbers.Count != preview.Count(c => c == '{'))
                    warning = new WarningResult("Dont mix number placeholders with word placeholders.", "Word placeholders will be ignored");

                foreach (Match mm in matches)
                {
                    if (int.TryParse(mm.Groups[1].Value, out var n))
                        numbers.Add(n);
                }
                var map = new Dictionary<int, string>();// 编号与输入词的映射
                int wIndex = 0;
                foreach (var n in numbers)
                {
                    if (wIndex >= words.Count) break;
                    map[n] = words[wIndex++];
                }

                // 替换
                preview = Regex.Replace(preview, @"\{(\d+)\}", m =>
                {
                    if (int.TryParse(m.Groups[1].Value, out var n) && map.TryGetValue(n, out var val))
                        return val;
                    return m.Value; //有就替换没有就保留
                });

            }
            // {}、{name}、{any text}
            else if (Regex.IsMatch(preview, @"\{[^}]*\}"))
            {

                int seq = 0;
                preview = Regex.Replace(preview, @"\{[^}]*\}", m =>
                {
                    if (seq < words.Count)
                        return words[seq++];
                    return m.Value;
                });
            }
            return warning;
        }

        
        public System.Windows.Controls.Control CreateSettingPanel()
        {

            return new View.SettingsControl(_settings);
        }

        public void ReloadData()
        {
            _templates = _settings.FormatStrings
            ?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        }
        public class WarningResult : Result
        {
            public WarningResult(string message, string subTitle)
            {
                Title = message;
                SubTitle = subTitle;
                //Although the warningIconPath is correct, it shows the icon from the other plugin "RollDice"
                //JUST READ IT BEFORE USING IT WILL FIX IT HOWWWWWW
                IcoPath = warningIconPath.Trim();
                Action = _ => false;
            }
        }
    }
}