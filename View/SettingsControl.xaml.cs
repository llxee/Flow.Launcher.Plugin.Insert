using System.Windows.Controls;
using Flow.Launcher.Plugin.Insert.Model;

namespace Flow.Launcher.Plugin.Insert.View
{
	public partial class SettingsControl : UserControl
	{
		public Settings Settings { get; set; }

		public SettingsControl(Settings settings)
		{
			InitializeComponent();
			Settings = settings;
			// Bind directly to the Settings object so XAML can use simple paths like "FormatStrings"
			DataContext = Settings;
		}
	}
}
