using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BookReader
{
	internal class BaseBook : IBook
	{
		#region -----------------constructors-----------------

		public BaseBook()
		{
		}

		public BaseBook(string filePath)
		{
			_filePath = filePath;
			RaisePropetyChanged("FilePath");
			RaisePropetyChanged("FileName");
		}
		#endregion

		#region -----------------properties-----------------

		private string _filePath = string.Empty;
		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}

		[NonSerialized()]
		private IBookItem _CurrentPage = null;
		public IBookItem CurrentPage
		{
			get { return _CurrentPage; }
			set { _CurrentPage = value; }
		}

		private List<IBookItem> _Pages = new List<IBookItem>();
		public List<IBookItem> Pages
		{
			get { if (_Pages == null) _Pages = new List<IBookItem>(); return _Pages; }
			set { _Pages = value; }
		}
		
		private string _Bookmark = string.Empty;
		public string Bookmark
		{
			get { return _Bookmark; }
			set { _Bookmark = value; RaisePropetyChanged("Bookmark"); }
		}

        private bool _IsRead= false;
        public bool IsRead
        {
            get { return _IsRead; }
            set { _IsRead = value; RaisePropetyChanged("IsRead"); }
        }

        private bool _IsSecured= false;
        public bool IsSecured
        {
            get { return _IsSecured; }
            set { _IsSecured = value; RaisePropetyChanged("IsSecured"); }
        }
        
        private string _Password= string.Empty;
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }


		private BitmapImage _Cover = null;
		public BitmapImage Cover
		{
			get { return _Cover; }
			set { _Cover = value; RaisePropetyChanged("Cover"); }
		}

		public string FileName
		{
			get
			{
				return Path.GetFileName(this._filePath);
			}
		}

		private int _NbPages;
		public int NbPages
		{
			get { return _NbPages; }
			set { _NbPages = value; }
		}

		private long _Size;
		public long Size
		{
			get { return _Size; }
			set { _Size = value; }
		}


		#endregion

		#region -----------------loading/unloading-----------------

		virtual public void Load()
		{
			_CurrentPage = Pages[0];
		}

		virtual public void UnLoad()
		{
			Pages.Clear();
		}
		#endregion

		#region -----------------virtual's-----------------

        virtual public BitmapImage GetImageFromStream(string fileName)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region -----------------bookmark-----------------

		public void SetMark()
		{
			Bookmark = _CurrentPage.FilePath;
		}

		public void GotoMark()
		{
			if (!string.IsNullOrEmpty(_Bookmark))
			{
				foreach (IBookItem pg in Pages)
				{
					if (pg.FilePath == _Bookmark)
					{
						_CurrentPage = pg;
						return;
					}
				}
			}
		}
		#endregion

		#region -----------------page navigation management-----------------

		public bool GotoPage( IBookItem page )
		{
			foreach (IBookItem pg in Pages)
			{
				if (pg == page)
				{
					_CurrentPage = pg;
					return true;
				}
			}
			return false;
		}

		public bool GotoNextPage()
		{
			int next = Pages.IndexOf(_CurrentPage);
			if (next >= Pages.Count-1)
				return false;
			else
			{
				next = next + 1;
				_CurrentPage = Pages[next];
				return true;
			}
		}

		public bool GotoPreviousPage()
		{
			int next = Pages.IndexOf(_CurrentPage);
			if (next == 0)
				return false;
			else
			{
				next = next - 1;
				_CurrentPage = Pages[next];
				return true;
			}
		}
		#endregion

		#region -----------------cache management-----------------

		internal IBookItem GetNextPage(IBookItem fromPage )
		{
			int next = Pages.IndexOf(fromPage);
			if (next >= Pages.Count-1)
				return null;
			else
			{
				next = next + 1;
				return Pages[next];
			}
		}

        private long _TotalSize;

		virtual public void ManageCache()
		{
            int counter = Pages.Where(p => p.ImageExist == true).Count();

            //select and delete if more than cache count
            IEnumerable<IBookItem> filter = Pages
                        .Where(p => p.ImageExist == true)
                        .OrderBy(p => p.Index).Take(counter - Properties.Settings.Default.ImageCacheCount);
 
            foreach (IBookItem item in filter)
                item.Image = null;

            //select and delete if over cache duration
            filter = Pages
                        .Where(p => p.ImageExist == true && 
                                p.ImageLastAcces < DateTime.Now.AddMinutes(-Properties.Settings.Default.ImageCacheDuration))
                        .OrderBy( p => p.Index );

            foreach (IBookItem item in filter)
                item.Image = null;

            _TotalSize = Pages.Where(p => p.ImageExist == true).Sum(f => f.Image.StreamSource.Length);
		}

        public string GetCacheInfo()
        {
            int cacheCount = Pages.Where(p => p.ImageExist == true).Count();

            return string.Format("Cache Info : Nb Image={0}/{1}; Size={2} Mo", cacheCount,
                Properties.Settings.Default.ImageCacheCount, _TotalSize / 1024 / 1024);
        }

		#endregion

		#region -----------------INotifyPropertyChanged-----------------

		private void RaisePropetyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
