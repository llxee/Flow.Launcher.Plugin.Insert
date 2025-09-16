using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.Insert.Model
{
    public class Settings
    {
        public string FormatStrings { get; set; } = "Hello, {name}! Today is {day}.";
        public bool UsingWhiteIcons { get; set; } = false;

    }
}