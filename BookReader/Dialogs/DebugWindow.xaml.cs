using System.Windows;
using System.Windows.Input;
using System;
using BookReader.Dialogs;

namespace BookReader
{
	/// <summary>
	/// Interaction logic for DebugWindow.xaml
	/// </summary>
    public partial class DebugWindow : HeaderedDialogWindow
	{
		public DebugWindow()
		{
			InitializeComponent();
		}

		public string ExceptionContent
		{
			get { return this.tbContent.Text; }
			set { this.tbContent.Text += value; }
		}
	}
}
