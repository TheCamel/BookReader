using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BookReader.Common;

namespace BookReader.Controls
{
    public enum AutoFit
    {
        None, Width, Height
    }

    /// <summary>
    /// Interaction logic for PageViewer.xaml
    /// </summary>
    public partial class PageViewer : UserControl
    {
        #region -----------------private and constructor-----------------

        public PageViewer()
        {
            InitializeComponent();
        }

		// should we wait at the end of the page that we press down one more time 
		// to go to the next page
        private bool WaitAtBottom = true;

        // zooming
        private ScaleTransform scaleTransform = new ScaleTransform();
        private double _scale = 1.0;
		private const int FIT_BORDER = 30;


        // moving the image
        private Point _mouseDragStartPoint;
        private Point _scrollStartOffset;

        #endregion

        #region -----------------Properties-----------------

        /// <summary>
        /// The zoom scale
        /// </summary>
        public AutoFit AutoFitMode
        {
            get { return (AutoFit)Properties.Settings.Default.UseAutoFit; }
        }


		/// <summary>
		/// The zoom scale
		/// </summary>
		public double Scale
        {
            get { return _scale; }
			set { _scale = value; UpdateScale(); }
        }

        /// <summary>
        /// The image to display
        /// </summary>
        public ImageSource Source
        {
            get { return this.PageImage.Source; }
            set { this.PageImage.Source = value; }
        }

		/// <summary>
		/// Scroll the page to the top
		/// </summary>
        public void ScrollToHome()
        {
            this.PageContent.ScrollToHome();
            if (AutoFitMode != AutoFit.None)
                Fit();
        }

		/// <summary>
		/// Scroll the page to the bottom
		/// </summary>
        public void ScrollToBottom()
        {
            this.PageContent.ScrollToBottom();
            if (AutoFitMode != AutoFit.None)
                Fit();
        }
		#endregion

		#region -----------------loading and custom event-----------------

		/// <summary>
		/// Init the controls
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			this.PageImage.LayoutTransform = this.scaleTransform;

			//set the content of magnifier
			this.Magnifier.ContentToDisplay = this.PageImage;

			this.PageContent.Focus();
		}

		/// <summary>
		/// Page registered event
		/// </summary>
		public static readonly RoutedEvent PageChangedEvent = EventManager.RegisterRoutedEvent("PageChangedEvent",
		RoutingStrategy.Bubble,
		typeof(PageChangedEventHandler), typeof(PageViewer));

        /// <summary>
		/// page changed event handler delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PageChangedEventHandler(object sender, PageRoutedEventArgs e);

		/// <summary>
		/// Page chaned event handler
		/// </summary>
        public event PageChangedEventHandler PageChanged
        {
            add { AddHandler(PageChangedEvent, value); }
            remove { RemoveHandler(PageChangedEvent, value); }
        }

		/// <summary>
		/// Raise the page changed event
		/// </summary>
		/// <param name="offset"></param>
        protected void RaisePageChanged(int offset)
        {
            PageRoutedEventArgs args = new PageRoutedEventArgs(offset);
            args.RoutedEvent = PageChangedEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// PageRoutedEventArgs : a custom event argument class
        /// </summary>
        public class PageRoutedEventArgs : RoutedEventArgs
        {
            private int _PageOffset;

            public PageRoutedEventArgs(int offset)
            {
                this._PageOffset = offset;
            }

            public int PageOffset
            {
                get { return _PageOffset; }
            }
        }

		/// <summary>
		/// Zoom registered event 
		/// </summary>
		public static readonly RoutedEvent ZoomChangedEvent = EventManager.RegisterRoutedEvent("ZoomChangedEvent",
																RoutingStrategy.Bubble,
																typeof(ZoomChangedEventHandler), typeof(PageViewer));

		/// <summary>
		/// the event handler delegate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public delegate void ZoomChangedEventHandler(object sender, ZoomRoutedEventArgs e);

		/// <summary>
		/// zoom changed event handler
		/// </summary>
		public event ZoomChangedEventHandler ZoomChanged
		{
			add { AddHandler(ZoomChangedEvent, value); }
			remove { RemoveHandler(ZoomChangedEvent, value); }
		}

		/// <summary>
		/// Raise the zoom changed event
		/// </summary>
		protected void RaiseZoomChanged()
		{
			ZoomRoutedEventArgs args = new ZoomRoutedEventArgs(_scale);
			args.RoutedEvent = ZoomChangedEvent;
			RaiseEvent(args);
		}

		/// <summary>
		/// Zoom changed event arguments
		/// </summary>
		public class ZoomRoutedEventArgs : RoutedEventArgs
		{
			private double _Scale;

			public ZoomRoutedEventArgs(double scale)
			{
				this._Scale = scale;
			}

			public double Scale
			{
				get { return _Scale; }
			}
		}
		#endregion

		#region -----------------mouse events-----------------

		/// <summary>
        /// start moving the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageContent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Start moving on the left button, let the right for popup menu
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _mouseDragStartPoint = e.GetPosition(PageContent);
                _scrollStartOffset.X = PageContent.HorizontalOffset;
                _scrollStartOffset.Y = PageContent.VerticalOffset;

                // Update the cursor if scrolling is possible 
                PageImage.Cursor = (PageContent.ExtentWidth > PageContent.ViewportWidth) ||
                    (PageContent.ExtentHeight > PageContent.ViewportHeight) ?
                    Cursors.ScrollAll : Cursors.Arrow;

				this.PageImage.Focus();
				PageImage.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// update the magnifier or move/scroll the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageContent_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //manage the magnifier
            if (Keyboard.IsKeyDown(Key.LeftShift) )
            {
                Magnifier.Update(Mouse.GetPosition(PageViewerGrid));
                e.Handled = true;
            }
            else
                // else move the image
				if (PageImage.IsMouseCaptured)
                {
                    // Get the new mouse position. 
                    Point mouseDragCurrentPoint = e.GetPosition(PageContent);

                    // Determine the new amount to scroll. 
                    Point delta = new Point(
                        (mouseDragCurrentPoint.X > this._mouseDragStartPoint.X) ?
                        -(mouseDragCurrentPoint.X - this._mouseDragStartPoint.X) :
                        (this._mouseDragStartPoint.X - mouseDragCurrentPoint.X),
                        (mouseDragCurrentPoint.Y > this._mouseDragStartPoint.Y) ?
                        -(mouseDragCurrentPoint.Y - this._mouseDragStartPoint.Y) :
                        (this._mouseDragStartPoint.Y - mouseDragCurrentPoint.Y));

                    // Scroll to the new position. 
                    PageContent.ScrollToHorizontalOffset(this._scrollStartOffset.X + delta.X);
                    PageContent.ScrollToVerticalOffset(this._scrollStartOffset.Y + delta.Y);
                }
        }

        /// <summary>
        /// Release after image moves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageContent_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //end image move
			if (PageImage.IsMouseCaptured)
            {
				PageImage.ReleaseMouseCapture();
				PageImage.Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region -----------------manage scrool, zoom and magnifier-----------------

		public void FitWidth()
		{
			_scale = (this.PageContent.ViewportWidth - FIT_BORDER) / this.PageImage.Source.Width;
			UpdateScale();
		}

		public void FitHeight()
		{
			_scale = (this.PageContent.ViewportHeight - FIT_BORDER) / this.PageImage.Source.Height;
			UpdateScale();
		}

        internal void Fit()
        {
            if (AutoFitMode == AutoFit.Height)
                FitHeight();
            else if (AutoFitMode == AutoFit.Width)
                FitWidth();
        }

		/// <summary>
		/// Update the zoom scale of the image control and raise the event
		/// </summary>
        private void UpdateScale()
        {
            this.scaleTransform.ScaleX = _scale;
            this.scaleTransform.ScaleY = _scale;

            this.scaleTransform.CenterX = 0.5;
            this.scaleTransform.CenterY = 0.5;

			RaiseZoomChanged();
        }

        /// <summary>
        /// Calculate the zoom scale of the page
        /// </summary>
        /// <param name="delta"></param>
        private void UpdateContent(bool delta)
        {
            _scale += delta ? 0.01 : -0.01;
            _scale = _scale < 0.01 ? 0.01 : _scale;
            _scale = _scale > 4 ? 4 : _scale;

			UpdateScale();
        }

        /// <summary>
        /// manage the zoom or scrolling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageContent_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //zooming
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
				UpdateContent(e.Delta > 0);
                e.Handled = true;
            }
            else
            {
                if (e.Delta > 0)
                {
                    ManageScroolUp();
                }
                else
                {
                    ManageScroolDown();
                }
            }
        }

        /// <summary>
        /// Display the magnifier if Key.LeftShift
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageContent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                Magnifier.Update(Mouse.GetPosition(PageViewerGrid));
                Magnifier.Display(Visibility.Visible);
				
				this.PageContent.CaptureMouse();

                e.Handled = true;
            }
        }

        /// <summary>
        /// manage the scroll or magnifier
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageContent_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                Magnifier.Display(Visibility.Hidden);
				
				this.PageContent.ReleaseMouseCapture();
				
				e.Handled = true;
                return;
            }

            if (e.Key == Key.PageDown || e.Key == Key.Down)
            {
                ManageScroolDown();
                e.Handled = true;
				return;
            }
            else if (e.Key == Key.PageUp || e.Key == Key.Up)
            {
                ManageScroolUp();
                e.Handled = true;
				return;
            }
        }

        /// <summary>
        /// Scrool up if at the top of the page
        /// </summary>
        private void ManageScroolUp()
        {
            try
            {
                if (this.PageContent.VerticalOffset == 0)
                {
                    RaisePageChanged(-1); ;
                }
            }
            catch (Exception err)
            {
				ExceptionManagement.Manage("PageViewer:ManageScroolUp", err);
            }
        }

        /// <summary>
        /// Scrool down if at the end of the page
        /// </summary>
        private void ManageScroolDown()
        {
            try
            {
                if (this.PageContent.VerticalOffset + this.PageContent.ViewportHeight >= this.PageContent.ExtentHeight)
                {
                    if (!WaitAtBottom)
                    {
                        WaitAtBottom = true;
                        return;
                    }
                    else WaitAtBottom = false;

                    RaisePageChanged(1);
				}
            }
            catch (Exception err)
            {
                ExceptionManagement.Manage("PageViewer:ManageScroolDown", err);
            }
        }
        #endregion
    }
}