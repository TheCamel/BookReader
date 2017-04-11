using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SevenZip;
using BookReader.Common;

namespace BookReader
{
	internal class RarBook : BaseBook
	{
		#region -----------------constructors-----------------

		public RarBook(string filePath, bool makeCover) : base( filePath )
		{
			try
			{
                if (!_IsInitDone)
                {
                    _IsInitDone = true;
                    string sevenZip = Assembly.GetExecutingAssembly().Location.Replace("BookReader.exe", "Dependencies\\7z.dll");
                    SevenZipExtractor.SetLibraryPath(sevenZip);
                }
			}
			catch (SevenZipLibraryException err)
			{
				ExceptionManagement.Manage("RarBook:RarBook", err);
			}

			if (makeCover)
				GenerateCover();
		}
		#endregion

		#region -----------------properties-----------------

        private static bool _IsInitDone = false;

        internal SevenZipExtractor RarReader
        {
            get;
            set;
        }

		#endregion

		#region -----------------loading/unloading-----------------

		override public void Load()
		{
            RarReader = new SevenZipExtractor(base.FilePath);

            foreach (ArchiveFileInfo fil in RarReader.ArchiveFileData)
			{
                if (!fil.IsDirectory)
                {
                    if (Properties.Settings.Default.ImageFilter.Contains(new FileInfo(fil.FileName).Extension.ToUpper()))
                    {
                        RarPage item = new RarPage( fil.FileName, Pages.Count, this );
                        Pages.Add(item);
                    }
                }
			}

			base.Load();
		}

		override public void UnLoad()
		{
			base.UnLoad();
            RarReader.Dispose();
            RarReader = null;
		}
		#endregion

		#region -----------------generation of cover page-----------------

		private void GenerateCover()
		{
			Thread t = new Thread(new ParameterizedThreadStart(LoadCoverThread));
			t.IsBackground = true;
			t.Priority = ThreadPriority.Lowest;
			t.Start(base.FilePath);
		}

		private void LoadCoverThread(object o)
		{
            SevenZipExtractor temp = null;

            try
            {
                temp = new SevenZipExtractor((string)o);
				this.Size = temp.PackedSize / 1024 / 1024;
				this.NbPages = temp.ArchiveFileNames.Count;

                foreach (ArchiveFileInfo fil in temp.ArchiveFileData)
                {
                    if (!fil.IsDirectory && Properties.Settings.Default.ImageFilter.Contains(new FileInfo(fil.FileName).Extension.ToUpper()))
                    {
                        MemoryStream stream = new MemoryStream();
                        temp.ExtractFile(fil.FileName, stream);

                        MemoryStream stream2 = new MemoryStream();
                        stream.WriteTo(stream2);
                        stream.Flush();
                        stream.Close();
                        stream2.Position = 0;

                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                        {
                            BitmapImage myImage = new BitmapImage();
                            myImage.BeginInit();
                            myImage.StreamSource = stream2;
                            myImage.DecodePixelWidth = 70;
                            myImage.EndInit();

                            base.Cover = myImage;
                        });

                        stream2 = null;
                        return;
                    }
                }
            }
            catch (Exception err)
            {
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
				{
					ExceptionManagement.Manage("RarBook:LoadCoverThread", err);
				});
            }
            finally
            {
				if (temp != null)
				{
					temp.Dispose();
					temp = null;
				}
            }
		}
		#endregion

		#region -----------------manage page images-----------------

		override public BitmapImage GetImageFromStream(string fileName)
		{
			return GetImageFromStream(RarReader, fileName, 0);
		}

		private BitmapImage GetImageFromStream(SevenZipExtractor zipFile, string fileName, int resize)
		{
			MemoryStream stream = new MemoryStream();
			zipFile.ExtractFile(fileName, stream);

			return StreamToImage.GetImageFromStreamBug(stream, resize);
		}

		#endregion

		#region -----------------cache management-----------------

		override public void ManageCache()
		{
            //clear old cache
			base.ManageCache();

            //load the 3 next pages
            BitmapImage tmpImage = null;
			IBookItem tmpPage = GetNextPage(CurrentPage);
            if (tmpPage != null) tmpImage= tmpPage.Image;

            tmpPage = GetNextPage(tmpPage);
            if (tmpPage != null) tmpImage = tmpPage.Image;

            tmpPage = GetNextPage(tmpPage);
            if (tmpPage != null) tmpImage = tmpPage.Image;
		}

		#endregion
	}
}
