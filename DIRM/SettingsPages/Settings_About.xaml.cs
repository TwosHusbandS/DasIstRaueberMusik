using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DIRM.SettingsPages
{
	/// <summary>
	/// Interaction logic for Settings_About.xaml
	/// </summary>
	public partial class Settings_About : Page
	{
		public Settings_About()
		{
			InitializeComponent();
		}





		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			MainWindow.AddParagraph(rtb_About, "");

			rtb_About.Document.Blocks.Remove(rtb_About.Document.Blocks.FirstBlock);

			MainWindow.AddParagraph(rtb_About, "Version: \"" + Globals.ProjectVersion.ToString() + "\", BuildInfo: \"" + Globals.BuildInfo + "\"");

			MainWindow.AddParagraph(rtb_About, "Tool to manage Rap Releases, save them, export them for reddit, scrape Releases from DeinUpdate.");

			MainWindow.AddParagraph(rtb_About, "You can RightClick the DeinUpdate-Button to take a look at the source. Might help if the scraper fucked up.");

			MainWindow.AddParagraph(rtb_About, "You can get Links for the Releases from Spotify and Youtube. The first 100 Youtube Releases will be quick, everything after that will be a little bit slow. This resets daily");

			MainWindow.AddParagraph(rtb_About, "You can export / import Releases for the loaded day via the Buttons visible on the Main Screen. You can import / export all Releases for all days via the Settings");

			MainWindow.AddParagraph(rtb_About, "");
		}

	}
}
