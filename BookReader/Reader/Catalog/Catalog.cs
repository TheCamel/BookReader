using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using BookReader.Common;

namespace BookReader
{
	internal class Catalog
	{
        #region ----------------SINGLETON----------------
		public static readonly Catalog Instance = new Catalog();

		/// <summary>
		/// Private constructor for singleton pattern
		/// </summary>
        private Catalog()
		{
		}
		#endregion

		#region -----------------PROPERTIES-----------------

        private const string _Version = "3.0";

        internal string ApplicationPath
        {
            get { return System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); }
        }

        internal string BookStore
        {
            get { return Path.Combine( ApplicationPath, "BookStore.bin" ) ; }
        }

        internal string CoverStore
        {
            get { return Path.Combine(ApplicationPath, "CoverStore.bin"); }
        }

		private string _bookPath = string.Empty;
		public string BookPath
		{
			get { return _bookPath; }
			set { _bookPath = value; }
		}
		private ObservableCollection<IBook> _Books = new ObservableCollection<IBook>();
		public ObservableCollection<IBook> Books
		{
		  get { return _Books; }
		  set { _Books = value; }
		}

        public IBook CurrentBook
        {
            get;
            set;
        }

        public bool IsChanged
        {
            get;
            set;
        }
        public bool IsCoverChanged
        {
            get;
            set;
        }


		#endregion

        #region -----------------SHORCUT TO BOOK-----------------

        public void LoadBook(IBook bk)
        {
            if (CurrentBook != null)
                CurrentBook.UnLoad();

            CurrentBook = bk;
            CurrentBook.Load();
        }

        public void SetBookmark(IBook bk)
        {
            if (bk!= null)
            {
                bk.SetMark();
                IsChanged = true;
            }
        }

        public void ClearBookmark(IBook bk)
        {
            if (bk != null)
            {
                bk.Bookmark = string.Empty;
                IsChanged = true;
            }
        }

        public void MarkAsRead(IBook bk, bool status)
        {
            if (bk != null)
            {
                bk.IsRead = status;
                IsChanged = true;
            }
        }

        public void Delete(IBook bk)
        {
            if (bk != null)
            {
                File.Delete(bk.FileName);
                Books.Remove(bk);
                IsChanged = true;
                IsCoverChanged = true;
            }
        }

        public void Protect(IBook bk, bool status, string password)
        {
            if (bk != null)
            {
                if (status == false) //remove protection
                {
                    if( bk.Password != password )
                        return;
                }
                bk.Password = password;
                bk.IsSecured = status;
                IsChanged = true;
            }
        }

        public string GetCacheInfo()
        {
            if (CurrentBook != null)
                return CurrentBook.GetCacheInfo();
            else
                return string.Empty;
        }


        #endregion

        #region -----------------refresh-----------------

        public void Refresh()
        {
            try
            {
                Thread t = new Thread(new ParameterizedThreadStart(RefreshThread));
                t.IsBackground = true;
                t.Priority = ThreadPriority.BelowNormal;
                t.Start(_bookPath);
            }
            catch (Exception err)
            {
				ExceptionManagement.Manage("Catalog:Refresh", err);
            }
        }

        internal void RefreshThread(object o)
        {
            try
            {
                //first, remove unfounded books
                List<IBook> temp = new List<IBook>();
                foreach (IBook book in this._Books)
                {
                    if (!File.Exists(book.FilePath))
                        temp.Add( book );
                }

                foreach (IBook book in temp)
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        _Books.Remove(book);
                        IsCoverChanged = true;
                    });

                //then add the new ones
                ParseDirectoryRecursiveWithCheck(_bookPath);
            }
            catch (Exception err)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    ExceptionManagement.Manage("Catalog:RefreshThread", err);
                });
            }
        }

        #endregion

		public IBook AddExternalBook(string book)
		{
			FileInfo file = new FileInfo(book);
			
			IBook bk = (IBook)new RarBook(file.FullName, true);
			bk.Size = file.Length;
			Books.Add(bk);
			IsChanged = true;
            IsCoverChanged = true;

			return bk;
		}

        #region -----------------load/save-----------------

        public void Load(string path)
		{
			try
			{
				_bookPath = path;
				Load();
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Catalog:LoadPath", err);
			}
		}

		private void Load()
		{
			try
			{
				if (File.Exists(BookStore))
				{
					//load the book name and bookmark
                    if (LoadBooks(BookStore))
					{
						if (File.Exists(CoverStore))
						{
							// load the cover images
							Thread t = new Thread(new ParameterizedThreadStart(LoadCovers));
							t.IsBackground = true;
							t.Priority = ThreadPriority.BelowNormal;
                            t.Start(CoverStore);
						}

						//then refresh the book
                        //Refresh();
					}
					else
						ParseDirectoryThread();
				}
				else //binary files does not exist, parse the directory
				{
					ParseDirectoryThread();
				}
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Catalog:Load", err);
			}
		}

		public void Save()
		{
			try
			{
				if (IsChanged)
				{
					//remove the book without covers
					RemoveDirtyBooks();

					// save the books name and bookmarks
					SaveBooks(BookStore);

                    if( IsCoverChanged ) //save the covers
					    SaveCovers(CoverStore);
				}
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Catalog:Save", err);
			}
		}

		internal void RemoveDirtyBooks()
		{
			try
			{
				//remove books without covers
				List<IBook> temp = new List<IBook>();
				foreach (IBook book in this._Books)
				{
					if ( book.Cover == null )
						temp.Add(book);
				}

                foreach (IBook book in temp)
                {
                    _Books.Remove(book);
                    IsChanged = true;
                    IsCoverChanged = true;
                }
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Catalog:RefreshThread", err);
			}
		}

		#endregion

		#region -----------------directory parsing-----------------

		internal void ParseDirectoryThread()
		{
			try
			{
				Books.Clear();
                IsChanged = IsCoverChanged = true;

                Thread t = new Thread(new ParameterizedThreadStart(ParseDirectoryRecursive));
				t.IsBackground = true;
				t.Priority = ThreadPriority.BelowNormal;
				t.Start(_bookPath);
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Catalog:ParseDirectoryThread", err);
			}
		}

		internal void ParseDirectoryRecursive(object path)
		{
			try
			{
				DirectoryInfo directory = new DirectoryInfo((string)path);
				if (!directory.Exists)
				{
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        MessageBox.Show("Catalog path does not exist! Please check the options box");
                    });
					return;
				}
				foreach (FileInfo file in directory.GetFiles("*.*"))
				{
                    if (Properties.Settings.Default.BookFilter.Contains(file.Extension.ToUpper()))
					{
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
						{
                            IBook bk = (IBook)new RarBook(file.FullName, true);
							bk.Size = file.Length;
							Books.Add(bk);
							IsChanged = IsCoverChanged = true;
						});
					}
				}
				foreach (DirectoryInfo dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
				{
					ParseDirectoryRecursive(dir.FullName);
				}
			}
			catch (Exception err)
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
				{
					ExceptionManagement.Manage("Catalog:ParseDirectoryRecursive", err);
				});
				return;
			}

			return;
		}

        internal void ParseDirectoryRecursiveWithCheck(object path)
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo((string)path);
                if (!directory.Exists)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        MessageBox.Show("Catalog path does not exist! Please check the options box");
                    });
                    return;
                }
                foreach (FileInfo file in directory.GetFiles("*.*"))
                {
                    if (Properties.Settings.Default.BookFilter.Contains(file.Extension.ToUpper()))
                    {
                        if( !BookExist( file.FullName ) )
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
                            {
                                IBook bk = (IBook)new RarBook(file.FullName, true);
                                bk.Size = file.Length;
                                Books.Add(bk);
                                IsChanged = IsCoverChanged = true;
                            });
                    }
                }
                foreach (DirectoryInfo dir in directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
					ParseDirectoryRecursiveWithCheck(dir.FullName);
                }
            }
            catch (Exception err)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
					ExceptionManagement.Manage("Catalog:ParseDirectoryRecursiveWithCheck", err);
                });
                return;
            }

            return;
        }

        internal bool BookExist(string filepath)
        {
            foreach (IBook bk in _Books)
            {
                if (bk.FilePath == filepath)
                    return true;
            }
            return false;
        }
		#endregion

		#region -----------------load/save books collection-----------------

		private bool LoadBooks(string fileName)
		{
			bool result = true;

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(fileName,
				FileMode.Open,
				FileAccess.Read,
				FileShare.None);

			try
			{
                //binary version
                string version = (string)formatter.Deserialize(stream);

				//the catalog path
				string booksFrom = (string)formatter.Deserialize(stream);

				//not on the same folder, restart from null
				if (this._bookPath != booksFrom || !Directory.Exists(this._bookPath))
				{
					//this._Books = new ObservableCollection<IBook>();
					this._Books.Clear();
					result = false;
				}
				else
				{
					//the book count
					int count = (int)formatter.Deserialize(stream);

					for ( int i = 0; i < count; i++ )
					{
						//each properties
						string filePath = (string)formatter.Deserialize(stream);
						long size = (long)formatter.Deserialize(stream);
						int nbPages = (int)formatter.Deserialize(stream);
						string bookmark = (string)formatter.Deserialize(stream);
                        bool isread = (bool)formatter.Deserialize(stream);
                        bool issecured= (bool)formatter.Deserialize(stream);

                        FileInfo file = new FileInfo(filePath);
						if( file.Exists )
						{
							IBook bk = null;

                            if (Properties.Settings.Default.BookFilter.Contains(file.Extension.ToUpper()))
                                bk = (IBook)new RarBook(file.FullName, false);

							bk.Bookmark = bookmark;
							bk.Size = size;
							bk.NbPages = nbPages;
                            bk.IsRead = isread;
                            bk.IsSecured = issecured;

							this._Books.Add(bk); 
						}
					}
				}
			}
			catch( Exception err )
			{
				ExceptionManagement.Manage("Catalog:LoadBooks", err);
			}
			finally
			{
				stream.Close();
			}
			return result;
		}

		private void SaveBooks(string fileName)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
			try
			{
                //binary version
                formatter.Serialize(stream, _Version);
                
                //the catalog path
				if (this._bookPath != null)
					formatter.Serialize(stream, this._bookPath);

				//the book count
				formatter.Serialize(stream, this._Books.Count);

				foreach (IBook book in this._Books)
				{
					//file path
					formatter.Serialize(stream, book.FilePath);
					//file size
					formatter.Serialize(stream, book.Size);
					//Nb pages
					formatter.Serialize(stream, book.NbPages);
					//Bookmark
					formatter.Serialize(stream, book.Bookmark);
                    //IsRead
                    formatter.Serialize(stream, book.IsRead);
                    //IsSecured
                    formatter.Serialize(stream, book.IsSecured);
                }
			}
			catch( Exception err )
			{
				ExceptionManagement.Manage("Catalog:SaveBooks", err);
			}
			finally
			{
				stream.Close();
			}
		}
		#endregion

		#region -----------------load/save covers-----------------

		public void LoadCovers(object fileName)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream streamBin = new FileStream((string)fileName, FileMode.Open, FileAccess.Read, FileShare.None);

			try
			{
				//book count
				int count = (int)formatter.Deserialize(streamBin);

				for (int i = 0; i < count; i++)
				{
					string filePath = (string)formatter.Deserialize(streamBin);

                    //allways read the stream even if does not exist anymore
                    MemoryStream coverStream = (MemoryStream)formatter.Deserialize(streamBin);

					foreach (IBook book in this._Books)
					{
						if (book.FilePath == filePath)
						{
							MemoryStream stream2 = new MemoryStream();
							coverStream.WriteTo(stream2);
							coverStream.Flush();
							coverStream.Close();

							stream2.Position = 0;

							Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
							{
								BitmapImage myImage = new BitmapImage();
								myImage.BeginInit();
								myImage.StreamSource = stream2;
								myImage.DecodePixelWidth = 70;
								myImage.EndInit();

								book.Cover = myImage;
							});
							coverStream = null;
							stream2 = null;
						}
					}
				}
			}
			catch (Exception err)
			{
				Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
				{
					ExceptionManagement.Manage("Catalog:LoadCovers", err);
				});
			}
			finally
			{
				streamBin.Close();
			}
		}

		private void SaveCovers(string fileName)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
			try
			{
				//book count
				formatter.Serialize(stream, this._Books.Count);

				foreach (IBook book in this._Books)
				{
					//each book path and cover
					formatter.Serialize(stream, book.FilePath);
					formatter.Serialize(stream, StreamToImage.GetStreamFromImage(book.Cover));
				}
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Catalog:SaveCovers", err);
			}
			finally
			{
				stream.Close();
			}
		}
		
		#endregion
	}
}
