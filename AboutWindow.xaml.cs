using System.Windows;

namespace CardFilePBX
{
	public partial class AboutWindow : Window
	{
		public string Version { get; set; } = CardFilePBX.Properties.Settings.Default.Version;
		public AboutWindow()
		{
			this.DataContext = this;
			InitializeComponent();
		}
	}
}
