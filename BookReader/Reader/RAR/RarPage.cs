using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace BookReader
{
	internal class RarPage : IBookItem
	{
		public RarPage(string filePath, int index, IBook parent)
		{
            Parent = parent;
            Index = index;
            FilePath = filePath;
            FileName = new FileInfo(FilePath).Name;
		}

        public IBook Parent
        {
            get;
            set;
        }
        
        public int Index
        {
            get;
            set;
        }

        public string FilePath
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        private BitmapImage _Image = null;
        public BitmapImage Image
        {
            get
            {
                if (_Image == null)
                    _Image = Parent.GetImageFromStream(FilePath);

                ImageLastAcces = DateTime.Now;
                return _Image;
            }
            set {_Image = value; }
        }

        public bool ImageExist
        {
            get
            {
                return _Image == null ? false : true;
            }
        }

        public DateTime ImageLastAcces
        {
            get;
            set;
        }
    }
}