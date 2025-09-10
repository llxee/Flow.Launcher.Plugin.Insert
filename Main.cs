using System;
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
        private string _selectedTemplate = string.Empty;

        public void Init(PluginInitContext context)
        {
            //throw new NotImplementedException(_settings.FormatStrings + "REMINDERRRRRRR");
            _context = context;
            _api = context.API;
            _settings = _api.LoadSettingJsonStorage<Settings>();
                    
            _templates = _settings.FormatStrings
            ?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

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
                            Action = _ =>
                            {
                                _selectedTemplate = t;
                                _api.ChangeQuery("is", false); // 清空查询，等待输入占位词
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

                // for {0}、{12} 
                if (Regex.IsMatch(preview, @"\{\d+\}"))
                {
                    // 使用捕获组提取数字索引
                    // 模式说明：\{(\d+)\}
                    //  - (\d+)：第 1 个捕获组，捕获 1 个或多个数字，作为占位符的索引
                    preview = Regex.Replace(preview, @"\{(\d+)\}", m =>
                    {
                        if (int.TryParse(m.Groups[1].Value, out var idx) && idx < words.Length)
                            return words[idx];
                        return m.Value; // keep placeholder if missing
                    });
                }
                else if (Regex.IsMatch(preview, @"\{[^}]*\}"))
                {
                    // for {}、{name}、{any text}
                    int seq = 0;
                    preview = Regex.Replace(preview, @"\{[^}]*\}", m =>
                    {
                        if (seq < words.Length)
                            return words[seq++];
                        return m.Value;
                    });
                }

                // Option 1: put the filled string into the query box
                results.Add(new Result
                {
                    Title = preview,
                    SubTitle = "Insert into query box",
                    Action = _ =>
                    {
                        _api.ChangeQuery(preview, false);
                        _selectedTemplate = string.Empty;
                        return false; // 保持窗口
                    }
                });

                // Option 2: copy the filled string to clipboard
                results.Add(new Result
                {
                    Title = preview,
                    SubTitle = "Copy to clipboard",
                    Action = _ =>
                    {
                        _api.CopyToClipboard(preview);
                        _selectedTemplate = string.Empty;
                        return true; // 关闭窗口
                    }
                });
                
                results.Add(new Result
                {
                    Title = "Cancel",
                    SubTitle = "Cancel and choose another template",
                    Action = _ =>
                    {
                        _selectedTemplate = string.Empty;
                        _api.ChangeQuery(string.Empty, false);
                        return false; // 保持窗口
                    }
                });
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
    }
}