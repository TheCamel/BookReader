using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BookReader.Controls
{
	public class DropDownButton : ToggleButton
	{
		public static readonly DependencyProperty DropDownProperty =
			DependencyProperty.Register("DropDown", typeof(ContextMenu), typeof(DropDownButton), 
			new FrameworkPropertyMetadata(DropDownPropertyChangedCallback));

		public ContextMenu DropDown
		{
			get
			{
				return (ContextMenu)GetValue(DropDownProperty);
			}
			set
			{
				SetValue(DropDownProperty, value);
			}
		} 

		public DropDownButton()
		{
			// Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 
			Binding binding = new Binding("DropDown.IsOpen");
			binding.Source = this;
			this.SetBinding(IsCheckedProperty, binding);
		}

		static void DropDownPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DropDownButton)d).DropDownPropertyChangedCallback(e);
		}

		void DropDownPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
		{
			if (DropDown != null)
			{
				DropDown.PlacementTarget = this;
				DropDown.Placement = PlacementMode.Bottom;
			}
		}
	}
}
