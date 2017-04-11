using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using BookReader.Common;
using BookReader.Dialogs;

namespace BookReader
{
    public partial class MainWindow : Window
    {
        private static RoutedUICommand _BookmarkCmd =
           new RoutedUICommand("Bookmark", "BookmarkCmd", typeof(MainWindow));

        public static RoutedUICommand BookmarkCmd
        {
            get { return MainWindow._BookmarkCmd; }
        }

        private static RoutedUICommand _GotoBookmarkCmd =
           new RoutedUICommand("Goto Bookmark", "GotoBookmarkCmd", typeof(MainWindow));

        public static RoutedUICommand GotoBookmarkCmd
        {
            get { return MainWindow._GotoBookmarkCmd; }
        }

        private static RoutedUICommand _ClearBookmarkCmd =
           new RoutedUICommand("Clear Bookmark", "ClearBookmarkCmd", typeof(MainWindow));

        public static RoutedUICommand ClearBookmarkCmd
        {
            get { return MainWindow._ClearBookmarkCmd; }
        }

		private static RoutedUICommand _ReadCmd =
		   new RoutedUICommand("Read", "ReadCmd", typeof(MainWindow));

		public static RoutedUICommand ReadCmd
		{
			get { return MainWindow._ReadCmd; }
		}

        private static RoutedUICommand _MarkReadCmd =
           new RoutedUICommand("Mark as (Un)Read", "MarkReadCmd", typeof(MainWindow));

        public static RoutedUICommand MarkReadCmd
        {
            get { return MainWindow._MarkReadCmd; }
        }

        private static RoutedUICommand _ProtectCmd =
           new RoutedUICommand("(Un)Protect", "_ProtectCmd", typeof(MainWindow));

        public static RoutedUICommand ProtectCmd
        {
            get { return MainWindow._ProtectCmd; }
        }

		private static RoutedUICommand _ChangeThemeCmd =
		   new RoutedUICommand("Theme", "_ChangeThemeCmd", typeof(MainWindow));

		public static RoutedUICommand ChangeThemeCmd
		{
			get { return MainWindow._ChangeThemeCmd; }
		}

        internal void SetupCommandBinding()
        {
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, CloseCmdExecuted, CmdCanAllaysExecute));
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenBookCmdExecuted, CmdCanAllaysExecute));
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, DeleteCmdExecuted, CanExecuteWithItem));

			this.CommandBindings.Add(new CommandBinding(MainWindow.BookmarkCmd, BookmarkCmdExecuted, CanExecuteWithItem));
			this.CommandBindings.Add(new CommandBinding(MainWindow.GotoBookmarkCmd, GotoBookmarkCmdExecuted, CanExecuteWithItemBookmarked));
			this.CommandBindings.Add(new CommandBinding(MainWindow.ClearBookmarkCmd, ClearBookmarkCmdExecuted, CanExecuteWithItemBookmarked));

			this.CommandBindings.Add(new CommandBinding(MainWindow.ReadCmd, ReadCmdExecuted, CmdCanAllaysExecute));
            this.CommandBindings.Add(new CommandBinding(MainWindow.MarkReadCmd, MarkReadCmdExecuted, CanExecuteWithItem));
            this.CommandBindings.Add(new CommandBinding(MainWindow.ProtectCmd, ProtectCmdExecuted, CanExecuteWithItem));

			this.CommandBindings.Add(new CommandBinding(MainWindow.ChangeThemeCmd, ChangeThemeCmdExecuted, CmdCanAllaysExecute));
        }

        void CmdCanAllaysExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void CanExecuteWithItem(object sender, CanExecuteRoutedEventArgs e)
        {
			string parameter = e.Parameter as string;

			if (parameter != null)
			{
				if( parameter == "LIST" )
					e.CanExecute = CurrentListBoxBook != null ? true : false;
				else
				if( parameter == "VIEW" )
					e.CanExecute = Catalog.Instance.CurrentBook != null ? true : false;
			}
			else
				e.CanExecute = CurrentListBoxBook != null ? true : false;
		}

		void CanExecuteWithItemBookmarked(object sender, CanExecuteRoutedEventArgs e)
		{
			string parameter = e.Parameter as string;

			if (parameter != null)
			{
				if (parameter == "LIST")
					e.CanExecute = string.IsNullOrEmpty(CurrentListBoxBook.Bookmark) ? false : true;
				else
					if (parameter == "VIEW")
						e.CanExecute = string.IsNullOrEmpty(Catalog.Instance.CurrentBook.Bookmark) ? false : true;
			}
			else
				e.CanExecute = false;
		}

        void CloseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.ExplorerSize = this.Splitter.CatalogSize.Value;
                Properties.Settings.Default.Save();

                Catalog.Instance.Save();
                this.Close();
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("Main:CloseCmdExecuted", err);
            }
        }

        void OpenBookCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            try
            {
                using (System.Windows.Forms.OpenFileDialog browser = new System.Windows.Forms.OpenFileDialog())
                {
                    if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        LoadBook((IBook)Catalog.Instance.AddExternalBook(browser.FileName));
                    }
                }
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("Main:OpenBookCmdExecuted", err);
            }
        }

        void DeleteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            try
            {
                Catalog.Instance.Delete(CurrentListBoxBook);
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("Main:DeleteCmdExecuted", err);
            }
        }

		void BookmarkCmdExecuted(object target, ExecutedRoutedEventArgs e)
		{
			try
			{
				Catalog.Instance.SetBookmark(Catalog.Instance.CurrentBook);
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:BookmarkCmdExecuted", err);
			}
		}

		void GotoBookmarkCmdExecuted(object target, ExecutedRoutedEventArgs e)
		{
			string parameter = e.Parameter as string;

			if (parameter != null)
			try
			{
				if ( parameter == "LIST" && Catalog.Instance.CurrentBook != CurrentListBoxBook)
				{
					LoadBook(CurrentListBoxBook);
				}

				Catalog.Instance.CurrentBook.GotoMark();
				UpdateContent();
				this.SimplePageView.ScrollToHome();
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:GotoBookmarkCmdExecuted", err);
			}
		}

		void ClearBookmarkCmdExecuted(object target, ExecutedRoutedEventArgs e)
		{
			string parameter = e.Parameter as string;

			if (parameter != null)
			try
			{
				if ( parameter == "LIST" )
					Catalog.Instance.ClearBookmark(CurrentListBoxBook);
				else
					Catalog.Instance.ClearBookmark(Catalog.Instance.CurrentBook);
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:ClearBookmarkCmdExecuted", err);
			}
		}

		void ReadCmdExecuted(object target, ExecutedRoutedEventArgs e)
		{
			try
			{
				LoadBook(CurrentListBoxBook);
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:ReadCmdExecuted", err);
			}
		}

        void MarkReadCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            try
            {
				Catalog.Instance.MarkAsRead(CurrentListBoxBook, !CurrentListBoxBook.IsRead);
            }
            catch (Exception err)
            {
				ExceptionManagement.Manage("Main:MarkReadCmdExecuted", err);
            }
        }

        void ProtectCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            try
            {
                PasswordWindow Dlg = new PasswordWindow();
                Dlg.ShowDialog();

                if (Dlg.DialogResult == true)
                    Catalog.Instance.Protect(CurrentListBoxBook, !CurrentListBoxBook.IsSecured, Dlg.PassBox.Password);
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("Main:ProtectCmdExecuted", err);
            }
        }

		void ChangeThemeCmdExecuted(object target, ExecutedRoutedEventArgs e)
		{
			string parameter = e.Parameter as string;

			if (parameter != null)

			try
			{
				ThemeHelper.LoadTheme(parameter);
			}
			catch (Exception err)
			{
				ExceptionManagement.Manage("Main:ChangeThemeCmdExecuted", err);
			}
		}
    }
}
