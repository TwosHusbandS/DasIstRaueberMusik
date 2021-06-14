using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM
{
	public class ViewModel
	{
		public IList<Helper.Release> Releases { get; set; }

		public Helper.Release SelectedRelease { get; set; }

		private DelegateCommand<Helper.Release> _deleteRow;

		public DelegateCommand<Helper.Release> DeleteRow =>
			_deleteRow ?? (_deleteRow = new DelegateCommand<Helper.Release>(DeleteCommandName));

		void DeleteCommandName(Helper.Release parameter)
		{
			MyRemove(parameter);
		}


		public bool DoesContainAlready(IList<Helper.Release> myList, Helper.Release potentialNewRelease)
		{
			foreach (Helper.Release existingRelease in myList)
			{
				if ((existingRelease.Artist == potentialNewRelease.Artist) && (existingRelease.Title == potentialNewRelease.Title))
				{
					return true;
				}
			}
			return false;
		}


		public void MyAdd(Helper.Release myRelease, bool SkipSaving = false)
		{
			if (!DoesContainAlready(this.Releases, myRelease))
			{
				this.Releases.Add(myRelease);
				if (!SkipSaving)
				{
					Helper.CSVHelper.Save();
				}
			}
		}


		public void MyAdd(IList<Helper.Release> newReleases)
		{
			foreach (Helper.Release myRelease in newReleases)
			{
				MyAdd(myRelease, true);
			}
			Helper.CSVHelper.Save();
		}



		public IList<Helper.Release> MyAdd(IList<Helper.Release> myExistingRelease, Helper.Release newRelease)
		{
			IList<Helper.Release> rtrn = myExistingRelease;

			if (!DoesContainAlready(rtrn, newRelease))
			{
				rtrn.Add(newRelease);
			}

			return rtrn;
		}


		public IList<Helper.Release> MyAdd(IList<Helper.Release> myExistingRelease, IList<Helper.Release> newReleases)
		{
			IList<Helper.Release> rtrn = myExistingRelease;

			foreach (Helper.Release myRelease in newReleases)
			{
				if (!DoesContainAlready(rtrn, myRelease))
				{
					rtrn.Add(myRelease);
				}
			}

			return rtrn;
		}


		public void MyRemove(Helper.Release newRelease)
		{
			if (newRelease is null)
				return;

			if (this.Releases.Contains(newRelease))
				this.Releases.Remove(newRelease);
		}

		public void Clear()
		{
			while (this.Releases.Count > 0)
			{
				this.Releases.RemoveAt(0);
			}
		}
	}
}
