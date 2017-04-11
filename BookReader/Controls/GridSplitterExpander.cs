using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Data;

namespace BookReader.Controls
{
	public class GridSplitterExpander : ContentControl
	{
		#region --------------------DEPENDENCY PROPERTIES--------------------

		public static readonly DependencyProperty OrientationProperty =
			   DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GridSplitterExpander));

		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			   DependencyProperty.Register("Title", typeof(string), typeof(GridSplitterExpander));

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty GridIndexProperty =
			   DependencyProperty.Register("GridIndex", typeof(int), typeof(GridSplitterExpander));

		public int GridIndex
		{
			get { return (int)GetValue(GridIndexProperty); }
			set { SetValue(GridIndexProperty, value); }
		}

        public bool IsExpanded
        {
            get { return _btnToggle.IsChecked == true ? true : false; }
            set { _btnToggle.IsChecked = value; }
        }

		private GridLength _CatalogSize;
        public GridLength CatalogSize
        {
            get { return _CatalogSize; }
            set
            {
                _CatalogSize = value;

                if (Orientation == Orientation.Vertical)
                    this._ParentGrid.ColumnDefinitions[GridIndex].Width = _CatalogSize;
                else
                    this._ParentGrid.RowDefinitions[GridIndex].Height = _CatalogSize;
            }
        }
		#endregion

		private Grid _ParentGrid;
		private GridSplitter _gridSplitter;
		private ToggleButton _btnToggle;
		private TextBlock _TitleBlock;

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			if( System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) ) return;
			//if (DesignHelper.IsInDesignMode()) return;

			_ParentGrid = ((Grid)this.Parent);

			_btnToggle = new ToggleButton();
			_btnToggle.SetValue(Panel.ZIndexProperty, 1);
			_btnToggle.Checked += new RoutedEventHandler(OnToggleChecked);
			_btnToggle.Unchecked += new RoutedEventHandler(OnToggleUnchecked);

			_gridSplitter = new GridSplitter();
			_gridSplitter.SetValue(Panel.ZIndexProperty, 0);
			_gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
			_gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
			_gridSplitter.DragCompleted += new DragCompletedEventHandler(OnDragCompleted);
			_gridSplitter.Background = (Brush)FindResource("ToolBarToggleButtonVerticalBackground");

			_TitleBlock = new TextBlock();
			_TitleBlock.SetValue(Panel.ZIndexProperty, 1);
			_TitleBlock.FontWeight = FontWeights.Bold;

			Binding bind = new Binding("Title");
			bind.Source = this;
			_TitleBlock.SetBinding(TextBlock.TextProperty, bind);

			if (Orientation == Orientation.Vertical)
			{
				_btnToggle.Height = _btnToggle.Width;
				_btnToggle.SetValue(Grid.ColumnProperty, 1);

				_btnToggle.HorizontalAlignment = HorizontalAlignment.Stretch;
				_btnToggle.VerticalAlignment = VerticalAlignment.Top;
				_btnToggle.Style = (Style)FindResource("GridSplitterExpanderVerticalButton");

				_gridSplitter.SetValue(Grid.ColumnProperty, 1);

				_TitleBlock.SetValue(Grid.ColumnProperty, 1);
				_TitleBlock.HorizontalAlignment = HorizontalAlignment.Center;
				_TitleBlock.VerticalAlignment = VerticalAlignment.Bottom;
				_TitleBlock.Margin = new Thickness(0, 0, 0, 5);
				_TitleBlock.LayoutTransform = new RotateTransform(-90, 0, 0);
			}
			else
			{
				_btnToggle.Width = _btnToggle.Height;
				_btnToggle.SetValue(Grid.RowProperty, 1);

				_btnToggle.HorizontalAlignment = HorizontalAlignment.Right;
				_btnToggle.VerticalAlignment = VerticalAlignment.Stretch;
				_btnToggle.Style = (Style)FindResource("GridSplitterExpanderHorizontalButton");

				_gridSplitter.SetValue(Grid.RowProperty, 1);

				_TitleBlock.SetValue(Grid.RowProperty, 1);
				_TitleBlock.HorizontalAlignment = HorizontalAlignment.Left;
				_TitleBlock.VerticalAlignment = VerticalAlignment.Center;
				_TitleBlock.Margin = new Thickness(5, 0, 0, 0);
			}
			_ParentGrid.Children.Add(_btnToggle);
			_ParentGrid.Children.Add(_gridSplitter);
			_ParentGrid.Children.Add(_TitleBlock);
		}

		private void OnToggleChecked(object sender, RoutedEventArgs e)
		{
			if (Orientation == Orientation.Vertical)
			{
                _CatalogSize = this._ParentGrid.ColumnDefinitions[GridIndex].Width;
				this._ParentGrid.ColumnDefinitions[GridIndex].Width = new GridLength(0);
			}
			else
			{
                _CatalogSize = this._ParentGrid.RowDefinitions[GridIndex].Height;
				this._ParentGrid.RowDefinitions[GridIndex].Height = new GridLength(0);
			}
		}

		private void OnToggleUnchecked(object sender, RoutedEventArgs e)
		{
			if (Orientation == Orientation.Vertical)
                this._ParentGrid.ColumnDefinitions[GridIndex].Width = _CatalogSize;
			else
                this._ParentGrid.RowDefinitions[GridIndex].Height = _CatalogSize;
		}

		private void OnDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			if (Orientation == Orientation.Vertical)
                _CatalogSize = this._ParentGrid.ColumnDefinitions[GridIndex].Width;
			else
                _CatalogSize = this._ParentGrid.RowDefinitions[GridIndex].Height;

			this._btnToggle.IsChecked = false;
		}
	}
}
