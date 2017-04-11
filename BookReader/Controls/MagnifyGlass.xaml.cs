using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace BookReader.Controls
{
    /// <summary>
    /// Interaction logic for MagnifyGlass.xaml
    /// </summary>
    public partial class MagnifyGlass : UserControl
    {
        public MagnifyGlass()
        {
            InitializeComponent();
        }

		/// <summary>
		/// Define the content that is displayed by the magnifier
		/// </summary>
        public Visual ContentToDisplay
        {
            get { return ((VisualBrush)this.MagnifierRectangle.Fill).Visual; }
            set { ((VisualBrush)this.MagnifierRectangle.Fill).Visual = value; }
        }

		/// <summary>
		/// Show or not the magnifier
		/// </summary>
		/// <param name="show"></param>
		public void Display(Visibility show)
		{
			magnifierCanvas.Visibility = show; 
		}

		/// <summary>
		/// Update the position and the content of the magnifier
		/// </summary>
		/// <param name="pos"></param>
        public void Update( Point pos )
        {
            VisualBrush b = (VisualBrush)MagnifierRectangle.Fill;

            Rect viewBox = b.Viewbox;
            double xoffset = viewBox.Width / 2.0;
            double yoffset = viewBox.Height / 2.0;
            viewBox.X = pos.X - xoffset;
            viewBox.Y = pos.Y - yoffset;
            b.Viewbox = viewBox;

			Canvas.SetLeft(magnifierCanvas, pos.X - MagnifierRectangle.Width / 2);
			Canvas.SetTop(magnifierCanvas, pos.Y - MagnifierRectangle.Height / 2);
		}
    }
}
