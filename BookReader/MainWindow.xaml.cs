using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using BookReader.Common;
using BookReader.Controls;
using BookReader.Dialogs;
using BookReader.Reader.Common;
using SevenZip;

namespace BookReader
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region -----------------private and constructor-----------------

		private DispatcherTimer _TimerClock;

		/// <summary>
		/// full screen flag, internal
		/// </summary>
		private bool _isFullSreen = false;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			// create and bind the commands
			SetupCommandBinding();
		}

		/// <summary>
		/// The current book in the cover list
		/// </summary>
        internal IBook CurrentListBoxBook
        {
            get { return (IBook)CatalogListBox.SelectedValue; }
        }

		#endregion

		#region -----------------loading/closing and timer-----------------
		/// <summary>
		/// On loading, create the timer and catalog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
                this.Splitter.CatalogSize = new GridLength(Properties.Settings.Default.ExplorerSize);
                SwapFullScreenMode(false);

                ////create a dispatch timer to load the image cache
				_TimerClock = new DispatcherTimer();
				_TimerClock.Interval = new TimeSpan(0, 0, 5);
				_TimerClock.IsEnabled = true;
				_TimerClock.Tick += new EventHandler(TimerElapse);

                this.DataContext = Catalog.Instance;

                //load the catalog of books
                Catalog.Instance.Load(Properties.Settings.Default.Catalog);

                ICollectionView view = CollectionViewSource.GetDefaultView(Catalog.Instance.Books);
                new SearchFilter(view, tbSearch);

			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:Window_Loaded", err);
			}
		}

		/// <summary>
		/// Save the catalog on closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
                Properties.Settings.Default.ExplorerSize = this.Splitter.CatalogSize.Value;
                Properties.Settings.Default.Save();

                Catalog.Instance.Save();
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:Window_Closing", err);
			}
		}

        ///// <summary>
        ///// Ask the book to manage his cache
        ///// </summary>
        ///// <param name="tag"></param>
        ///// <param name="args"></param>
		public void TimerElapse(object tag, EventArgs args)
		{
			try
			{
				if (Catalog.Instance.CurrentBook != null)
				{
					Catalog.Instance.CurrentBook.ManageCache();
					this.CacheInfo.Content = Catalog.Instance.GetCacheInfo();
				}

				this.BookInfo.Content = String.Format( "{0} Book(s)", Catalog.Instance.Books.Count);
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:TimerElapse", err);
			}
		}
		#endregion

        #region --------------------TITLE BAR--------------------

        /// <summary>
        /// title bar maximize button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// title bar minimize button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Move the window in response to mose move on the title bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion

		#region -----------------manage zoom and page changes-----------------

		/// <summary>
		/// Update the Page viewer regarding the slider value
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if( this.SimplePageView != null )
				this.SimplePageView.Scale = e.NewValue/100;
		}

		/// <summary>
		/// Update the slider regarding the page viewer scale
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SimplePageView_ZoomChanged(object sender, PageViewer.ZoomRoutedEventArgs e)
		{
			this.zoomSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.Slider_ValueChanged);
			this.zoomSlider.Value = Math.Round( e.Scale * 100, 0);
			this.zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.Slider_ValueChanged);
		}

		/// <summary>
		/// manage page changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SimplePageView_PageChanged(object sender, PageViewer.PageRoutedEventArgs e)
		{
			if (e.PageOffset == -1) //we go down
			{
                if (Catalog.Instance.CurrentBook.GotoPreviousPage())
				{
                    UpdateContent();
					this.SimplePageView.ScrollToBottom();
				}
			}
			else
				if (e.PageOffset == 1) //we go up
				{
                    if (Catalog.Instance.CurrentBook.GotoNextPage())
					{
                        UpdateContent();
                        this.SimplePageView.ScrollToHome();
					}
				}
		}

        private void UpdateContent()
		{
            this.SimplePageView.Source = Catalog.Instance.CurrentBook.CurrentPage.Image;
			this.PageInfo.Content = string.Format("Page {0} : {1}/{2}", Catalog.Instance.CurrentBook.CurrentPage.FilePath,
																	Catalog.Instance.CurrentBook.CurrentPage.Index,
                                                                    Catalog.Instance.CurrentBook.NbPages );
        }
		#endregion

		#region -----------------menu item events-----------------

		/// <summary>
		/// Load a given book
		/// </summary>
		/// <param name="book"></param>
		private void LoadBook( IBook book )
		{
			try
			{
                if (book.IsSecured)
                {
                    PasswordWindow Dlg = new PasswordWindow();
                    Dlg.ShowDialog();

                    if (Dlg.DialogResult != true)
                        return;
                }
                Catalog.Instance.LoadBook(book);

                this.SimplePageView.Scale = 1.0;
                UpdateContent();
                this.SimplePageView.ScrollToHome();
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:LoadBook", err);
			}
		}
		#endregion

		#region -----------------toolbar events-----------------

        private void btnFitWidth_Click(object sender, RoutedEventArgs e)
        {
			this.SimplePageView.FitWidth();
        }

        private void btnFitHeight_Click(object sender, RoutedEventArgs e)
        {
			this.SimplePageView.FitHeight();
        }

		/// <summary>
		/// Display the about box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnAbout_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow dlg = new AboutWindow();
			dlg.ShowDialog();
		}

		/// <summary>
		/// Refresh the covers and books
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
            Catalog.Instance.Refresh();
		}

		/// <summary>
		/// Close the soft, nothing special
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnQuit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Display the options dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOptions_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OptionWindow dlg = new OptionWindow();

				//load the catalog of books if we change significative values 
				if (dlg.ShowDialog() == true)
				{
					if (dlg.NeedToReload)
					{
                        Catalog.Instance.Load(Properties.Settings.Default.Catalog);
					}
				}
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:btnOptions_Click", err);
			}
		}

		/// <summary>
		/// Swap the full screen state
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFullScreen_Click(object sender, RoutedEventArgs e)
		{
            SwapFullScreenMode(true);
		}

        private void SwapFullScreenMode(bool reduceExpander)
        {
            try
            {
                if (_isFullSreen)
                {
                    this.WindowState = WindowState.Normal;
                    _isFullSreen = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    _isFullSreen = true;
                }

                if( reduceExpander )
                   Splitter.IsExpanded = _isFullSreen;
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("Main:SwapFullScreenMode", err);
            }
        }
        
        private void btnGoto_Click(object sender, RoutedEventArgs e)
        {
            GotoPageWindow Dlg = new GotoPageWindow();
            Dlg.DataContext = Catalog.Instance.CurrentBook;
            Dlg.ShowDialog();

            if (Dlg.Page != null)
            {
                Catalog.Instance.CurrentBook.GotoPage(Dlg.Page);
                UpdateContent();
                this.SimplePageView.ScrollToHome();
            }
        }

		/// <summary>
		/// Convert the PDF to ZIP/RAR
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bntConvertPDF_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string pdfFile;

				using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
				{
					if (browser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
						return;
					else
						pdfFile = browser.FileName;
				}
				
				string pdf2image = Assembly.GetExecutingAssembly().Location.Replace("BookReader.exe", "Dependencies\\pdftohtml.exe");

				FileInfo fi = new FileInfo(pdfFile);

				ProcessStartInfo p = new ProcessStartInfo(pdf2image);
				p.Arguments = string.Format("\"{0}\" ", fi.FullName); //pdf file
				p.Arguments += string.Format("\"{0}\\{1}", fi.DirectoryName, fi.Name.Replace(fi.Extension, ""));

				string output = Path.Combine(fi.DirectoryName, fi.Name.Replace(fi.Extension, "") );
				if (!Directory.Exists(output) )
					Directory.CreateDirectory( output );

				p.Arguments += string.Format("\\{0}", "export.html");
				p.UseShellExecute = false;
				p.WindowStyle = ProcessWindowStyle.Hidden;
				p.CreateNoWindow = true;
				Process bat = System.Diagnostics.Process.Start(p);
				bat.WaitForExit();

				string sevenZip = Assembly.GetExecutingAssembly().Location.Replace("BookReader.exe", "Dependencies\\7z.dll");
                SevenZipCompressor.SetLibraryPath(sevenZip);
				SevenZipCompressor zip = new SevenZipCompressor();
				zip.ArchiveFormat = OutArchiveFormat.Zip;
				zip.CompressionLevel = CompressionLevel.Normal;
				zip.CompressionMethod = CompressionMethod.Default;
				zip.CompressionMode = CompressionMode.Create;
				zip.DirectoryStructure = true;
				zip.CompressDirectory( output, fi.Name.Replace(fi.Extension, ".zip"), "*.jpg", false );

				Directory.Delete( output, true );
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:bntConvertPDF_Click", err);
			}
		}

		#endregion

        private void CatalogListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LoadBook(CurrentListBoxBook);
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("Main:CatalogListBox_MouseDoubleClick", err);
            }
        }

		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			MaxiHelper.Manage(this);
		}
	}
}
