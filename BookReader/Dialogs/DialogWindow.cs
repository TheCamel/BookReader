using System.Windows;
using System.Windows.Controls;
using BookReader.Common;

namespace BookReader.Dialogs
{
	public class DialogWindow : Window
	{
		static DialogWindow()
        {
            // set the key to reference the style for this control
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
			    typeof(DialogWindow), new FrameworkPropertyMetadata(typeof(DialogWindow)));
		}

        public bool CloseAble
        {
            get;
            set;
        }

		public DialogWindow()
		{
			if (!DesignHelper.IsInDesignMode())
				this.Owner = Application.Current.MainWindow;
			if (!DesignHelper.IsInDesignMode())
				this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Button close = this.Template.FindName("PART_Close", this) as Button;

			if (close != null)
				close.Click += new RoutedEventHandler(close_Click);
		}

		public void close_Click(object sender, RoutedEventArgs e)
		{
            if (CloseAble)
                this.Close();
            else
                this.Hide();
		}

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			this.DragMove();
		}
	}
}
