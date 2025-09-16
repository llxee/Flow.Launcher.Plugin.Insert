using System;

namespace Flow.Launcher.Plugin.Insert
{
    public class WarningResult : Result
    {
        public WarningResult(string message, string subTitle)
        {
            Title = message;
            SubTitle = subTitle;
            IcoPath = Insert.WarningIconPath;
            Action = _ => false;
        }
    }
    public class IconResult : Result
    {
        public IconResult(string message, string subTitle, Func<ActionContext, bool> action)
        {
            Title = message;
            SubTitle = subTitle;
            IcoPath = Insert.IconPath;
        
            Action = action;
        }
    }
    public class MissingResult : Result
    {
        public MissingResult(string message, string subTitle)
        {
            Title = message;
            SubTitle = subTitle;
            IcoPath = Insert.MissingIconPath;
            Action = _ => false;
        }
    }
}