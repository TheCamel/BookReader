using System.Windows.Input;
using System.Windows;

namespace BookReader.Dialogs
{
    /// <summary>
    /// Interaction logic for GotoPage.xaml
    /// </summary>
	public partial class GotoPageWindow : Window
    {
        public GotoPageWindow()
        {
            InitializeComponent();
        }

        internal IBookItem Page
        {
            get;
            set;
        }

        private void lbPages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Page = (IBookItem)lbPages.SelectedItem;
            this.Close();
        }
    }
}
