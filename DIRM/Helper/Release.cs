using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM.Helper
{
	public enum ReleaseKinds
	{
		Album,
		Single
	}





	public class Release 
	{
		public string _artist { get; set; } = "";
		public string _title { get; set; } = "";
		public string _link { get; set; } = "";
		public string _info { get; set; } = "";

		public ReleaseKinds ReleaseKind { get; set; }

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
			}
		}

		public string Artist
		{
			get { return _artist; }
			set
			{
				_artist = value;
			}
		}

		public string Link
		{
			get
			{
				return _link;
			}
			set
			{
				_link = value;

			}
		}

		public string Info
		{
			get
			{
				return _info;
			}
			set
			{
				_info = value;

			}
		}


		public string SearchString
		{
			get
			{
				return this.Artist.Replace(" &", "&").Replace("&","") + " " + this.Title;
			}
		}





		public string GetInfoTabFromRelease()
		{
			string rtrn = "";
			string[] links = this.Link.Split(' ');

			foreach (string link in links)
			{
				if (!String.IsNullOrWhiteSpace(link))
				{
					string tmplink = link;
					tmplink = tmplink.Replace(" ", "");
					tmplink = tmplink.Replace(",", "");

					if (tmplink.ToLower().Contains("spotify"))
					{
						rtrn += "[Spotify](" + tmplink + ") - ";
					}
					else if (tmplink.ToLower().Contains("youtu"))
					{
						rtrn += "[Youtube](" + tmplink + ") - ";
					}
					else
					{
						rtrn += "[Link](" + tmplink + ") - ";
					}
				}
			}

			rtrn = rtrn.TrimEnd(' ').TrimEnd('-');

			if (!String.IsNullOrWhiteSpace(this.Info))
			{
				rtrn += this.Info;
			}

			if (String.IsNullOrEmpty(rtrn))
			{
				rtrn += " ";
			}

			return rtrn;
		}


		//public Release(string pArtist, string pTitle, ReleaseKinds pReleaseKind)
		//{
		//	this.Artist = pArtist;
		//	this.Title = pTitle;
		//	this.ReleaseKind = pReleaseKind;
		//}

		//public DataGridItem(string pName, bool pIsAwesome)
		//{
		//	this.Name = pName;
		//	this.IsAwesome = pIsAwesome;
		//}
	}
}
