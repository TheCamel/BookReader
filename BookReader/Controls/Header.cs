using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BookReader.Controls
{
	public class Header : ContentControl
	{
		#region --------------------DEPENDENCY PROPERTIES--------------------

		public static readonly DependencyProperty TitleProperty =
			   DependencyProperty.Register("Title", typeof(string), typeof(Header));

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty DescriptionProperty =
			   DependencyProperty.Register("Description", typeof(string), typeof(Header));

		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty ImageProperty =
			   DependencyProperty.Register("Image", typeof(ImageSource), typeof(Header));

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public static readonly DependencyProperty HasSeparatorProperty =
			   DependencyProperty.Register("HasSeparator", typeof(Visibility), typeof(Header));

		public Visibility HasSeparator
		{
			get { return (Visibility)GetValue(HasSeparatorProperty); }
			set { SetValue(HasSeparatorProperty, value); }
		}
		#endregion
	}
}
