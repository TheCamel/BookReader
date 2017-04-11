using System.Windows;
using System.Windows.Media;

namespace BookReader.Dialogs
{
	public class HeaderedDialogWindow : DialogWindow
	{
		static HeaderedDialogWindow()
        {
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
						typeof(HeaderedDialogWindow), new FrameworkPropertyMetadata(typeof(HeaderedDialogWindow),
							FrameworkPropertyMetadataOptions.Inherits));
        }

		public HeaderedDialogWindow() : base()
		{
		}

		#region --------------------DEPENDENCY PROPERTIES--------------------

		public static readonly DependencyProperty DialogDescriptionProperty =
			   DependencyProperty.Register("DialogDescription", typeof(string), typeof(HeaderedDialogWindow));

		public string DialogDescription
		{
			get { return (string)GetValue(DialogDescriptionProperty); }
			set { SetValue(DialogDescriptionProperty, value); }
		}

		public static readonly DependencyProperty DialogImageProperty =
			   DependencyProperty.Register("DialogImage", typeof(ImageSource), typeof(HeaderedDialogWindow));

		public ImageSource DialogImage
		{
			get { return (ImageSource)GetValue(DialogImageProperty); }
			set { SetValue(DialogImageProperty, value); }
		}

		#endregion
	}
}
