
namespace BookReader.Dialogs
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class PasswordWindow : HeaderedDialogWindow
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
