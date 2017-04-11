using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace BookReader
{
	internal interface IBook : INotifyPropertyChanged
	{
		string FileName { get; }
		int NbPages { get; set; }
		long Size { get; set; }

        string Password { get; set; }
        bool IsSecured { get; set; }
        bool IsRead { get; set; }
		string Bookmark { get; set; }
		BitmapImage Cover { get; set; }
		IBookItem CurrentPage { get; set; }
		string FilePath { get; set; }
		List<IBookItem> Pages { get; set; }

        BitmapImage GetImageFromStream(string fileName);

		void GotoMark();
		bool GotoNextPage();
		bool GotoPage(IBookItem page);
		bool GotoPreviousPage();

		void Load();
        void ManageCache();
        string GetCacheInfo();
        void SetMark();
		void UnLoad();
	}
}
