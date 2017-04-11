using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BookReader.Controls
{
    /// <summary>
    /// Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {
        public static readonly DependencyProperty RatingValueProperty =
            DependencyProperty.Register("RatingValue", typeof (int), typeof (RatingControl),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                                                   RatingValueChanged));

        private int _maxValue = 5;

        public RatingControl()
        {
            InitializeComponent();
        }

        public int RatingValue
        {
            get { return (int) GetValue(RatingValueProperty); }
            set
            {
                if (value < 0)
                {
                    SetValue(RatingValueProperty, 0);
                }
                else if (value > _maxValue)
                {
                    SetValue(RatingValueProperty, _maxValue);
                }
                else
                {
                    SetValue(RatingValueProperty, value);
                }
            }
        }

        private static void RatingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingControl parent = sender as RatingControl;
            int ratingValue = (int)e.NewValue;
            UIElementCollection children = ((StackPanel)(parent.Content)).Children;
            ToggleButton button = null;

            for (int i = 0; i < ratingValue; i++)
            {
                button = children[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = true;
            }

            for (int i = ratingValue; i < children.Count; i++)
            {
                button = children[i] as ToggleButton;
                if (button != null)
                    button.IsChecked = false;
            }
        }

        private void RatingButtonClickEventHandler(Object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;

            int newRating = int.Parse((String)button.Tag);

            if ((bool)button.IsChecked || newRating < RatingValue)
            {
                RatingValue = newRating;
            }
            else
            {
                RatingValue = newRating - 1;
            }

            e.Handled = true;
        }

        //private void RatingButtonMouseEnterEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    ToggleButton button = sender as ToggleButton;
        //    double hoverRating = double.Parse((String)button.Tag);
        //    int numberOfButtonsToHighlight = (int)(2 * hoverRating);

        //    UIElementCollection children = RatingContentPanel.Children;

        //    ToggleButton hlbutton = null;

        //    for (int i = 0; i < numberOfButtonsToHighlight; i++)
        //    {
        //        hlbutton = children[i] as ToggleButton;
        //        if (hlbutton != null)
        //            hlbutton.IsChecked = true;
        //    }

        //    for (int i = numberOfButtonsToHighlight; i < children.Count; i++)
        //    {
        //        hlbutton = children[i] as ToggleButton;
        //        if (hlbutton != null)
        //            hlbutton.IsChecked = false;
        //    }
        //}

        //private void RatingButtonMouseLeaveEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    double ratingValue = RatingValue;
        //    int numberOfButtonsToHighlight = (int)(2 * ratingValue);

        //    UIElementCollection children = RatingContentPanel.Children;
        //    ToggleButton button = null;

        //    for (int i = 0; i < numberOfButtonsToHighlight; i++)
        //    {
        //        button = children[i] as ToggleButton;
        //        if (button != null)
        //            button.IsChecked = true;
        //    }

        //    for (int i = numberOfButtonsToHighlight; i < children.Count; i++)
        //    {
        //        button = children[i] as ToggleButton;
        //        if (button != null)
        //            button.IsChecked = false;
        //    }
        //}
    }
}