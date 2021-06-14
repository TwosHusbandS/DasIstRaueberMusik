using System;
using System.Collections.Generic;
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
		public string _artist { get; set; }
		public string _title { get; set; }
		public string _link { get; set; }
		public string _info { get; set; }

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
