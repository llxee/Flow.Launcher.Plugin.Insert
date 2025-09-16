using System;

namespace Flow.Launcher.Plugin.Insert
{
    public class WarningResult : Result
    {
        public WarningResult(string message, string subTitle)
        {
            Title = message;
            SubTitle = subTitle;
            //Although the warningIconPath is correct, it shows the icon from the other plugin "RollDice"
            //JUST READ IT BEFORE USING IT WILL FIX IT HOWWWWWW
            IcoPath = Insert.warningIconPath.Trim();
            Action = _ => false;
        }
    }
    public class CompletedResult : Result
    {
        public CompletedResult(string message, string subTitle, Func<ActionContext, bool> action)
        {
            Title = message;
            SubTitle = subTitle;
            IcoPath = Insert.iconPath;
            Action = action;
        }
    }
    public class UnfindResult : Result
    {
        public UnfindResult(string message, string subTitle)
        {
            Title = message;
            SubTitle = subTitle;
            IcoPath = Insert.iconPath;
            Action = _ => false;
        }
    }
}