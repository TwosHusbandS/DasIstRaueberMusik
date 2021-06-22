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
			AddParagraph(rtb_About, "");

			rtb_About.Document.Blocks.Remove(rtb_About.Document.Blocks.FirstBlock);

			AddParagraph(rtb_About, "Version: \"" + Globals.ProjectVersion.ToString() + "\", BuildInfo: \"" + Globals.BuildInfo + "\"");

			AddParagraph(rtb_About, "Tool to manage Rap Releases, save them, export them for reddit, scrape Releases from DeinUpdate.");

			AddParagraph(rtb_About, "You can RightClick the DeinUpdate-Button to take a look at the source. Might help if the scraper fucked up.");

			AddParagraph(rtb_About, "You can get Links for the Releases from Spotify and Youtube. The first 100 Youtube Releases will be quick, everything after that will be a little bit slow. This resets daily");

			AddParagraph(rtb_About, "You can export / import Releases for the loaded day via the Buttons visible on the Main Screen. You can import / export all Releases for all days via the Settings");

			AddParagraph(rtb_About, "");
		}


		/// <summary>
		/// Adding a Paragraph to a RichTextBox
		/// </summary>
		/// <param name="rtb"></param>
		/// <param name="Paragraph"></param>
		private void AddParagraph(RichTextBox rtb, string Paragraph)
		{
			Paragraph para = new Paragraph();
			para.Margin = new Thickness(10);
			para.Inlines.Add(Paragraph);
			para.TextAlignment = TextAlignment.Center;
			rtb.Document.Blocks.Add(para);
		}
	}
}
