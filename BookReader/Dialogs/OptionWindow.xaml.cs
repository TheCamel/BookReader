using System;
using System.Windows;
using System.Windows.Input;
using BookReader.Dialogs;

namespace BookReader
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
    public partial class OptionWindow : HeaderedDialogWindow
	{
		public OptionWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
            LoadSettings();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();
		}

		private bool _NeedToReload = false;
		public bool NeedToReload
		{
			get { return _NeedToReload; }
			set { _NeedToReload = value; }
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			_NeedToReload = false;

			Properties.Settings.Default.ImageCacheCount = Convert.ToInt32(this.textBoxCache.Text);
			Properties.Settings.Default.ImageCacheDuration = Convert.ToInt32(this.sliderDurationCache.Value);

			if( Properties.Settings.Default.Catalog != this.textBoxPath.Text )
				_NeedToReload = true;

			Properties.Settings.Default.Catalog = this.textBoxPath.Text;
			Properties.Settings.Default.UseDebug = this.chkUseDebug.IsChecked == true ? true : false;

            if( this.rbNone.IsChecked == true )
                Properties.Settings.Default.UseAutoFit = 0;
            else
                if( this.rbWidth.IsChecked == true )
                    Properties.Settings.Default.UseAutoFit = 1;
            else
                if( this.rbHeight.IsChecked == true )
                    Properties.Settings.Default.UseAutoFit = 2;


			Properties.Settings.Default.Save();
			Properties.Settings.Default.Reload();

			this.DialogResult = true;
			this.Close();
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			using (System.Windows.Forms.FolderBrowserDialog browser = new System.Windows.Forms.FolderBrowserDialog())
			{
				browser.ShowNewFolderButton = false;
				browser.Description = "Select a folder containing your book...";
				browser.RootFolder = Environment.SpecialFolder.MyComputer;
				if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.textBoxPath.Text = browser.SelectedPath;
				}
			}
		}

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            LoadSettings();
        }

        private void LoadSettings()
        {
            this.textBoxPath.Text = Properties.Settings.Default.Catalog;
            this.textBoxCache.Text = Properties.Settings.Default.ImageCacheCount.ToString();
            this.sliderDurationCache.Value = Properties.Settings.Default.ImageCacheDuration;
            this.chkUseDebug.IsChecked = Properties.Settings.Default.UseDebug;
            if (Properties.Settings.Default.UseAutoFit == 1)
                this.rbWidth.IsChecked = true;
            else if (Properties.Settings.Default.UseAutoFit == 2)
                this.rbHeight.IsChecked = true;
            else
                this.rbNone.IsChecked = true;
        }
	}
}
