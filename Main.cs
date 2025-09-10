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
        private string _selectedTemplate = string.Empty;
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
            WarningResult? warning = null;
            if (string.IsNullOrEmpty(_selectedTemplate))
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
                                _selectedTemplate = t;
                                _api.ChangeQuery("is", false); // 不会关闭窗口
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
                var words = string.IsNullOrWhiteSpace(query.Search)
                ? Array.Empty<string>()
                : query.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string preview = _selectedTemplate;
                #region 找占位符

                // 找到所有数字占位符{1}
                if (Regex.IsMatch(preview, @"\{\d+\}"))
                {
                    // 获取数字占位符并排序
                    var matches = Regex.Matches(preview, @"\{(\d+)\}");
                    var numbers = new SortedSet<int>();
                    if (matches.Count == 0)
                        warning = new WarningResult("No valid placeholders found in the template.", "Please check the template format.");
                    if (numbers.Count != _selectedTemplate.Count(c => c == '{'))
                        warning = new WarningResult("Numbered placeholders with word placeholders.", "Word placeholders will be ignored");

                    foreach (Match mm in matches)
                    {
                        if (int.TryParse(mm.Groups[1].Value, out var n))
                            numbers.Add(n);
                    }
                    var map = new Dictionary<int, string>();// 编号与输入词的映射
                    int wIndex = 0;
                    foreach (var n in numbers)
                    {
                        if (wIndex >= words.Length) break;
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
                        if (seq < words.Length)
                            return words[seq++];
                        return m.Value;
                    });
                }
                #endregion

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
                        _selectedTemplate = string.Empty;
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
                        _selectedTemplate = string.Empty;
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
                        _selectedTemplate = string.Empty;
                        _api.ChangeQuery(string.Empty, false);
                        return false;
                    }
                });
                if (warning is not null)
                    results.Add(warning);
                #endregion
            }
            return results;
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
                IcoPath = warningIconPath;
                Action = _ => false;
            }
        }
    }
}