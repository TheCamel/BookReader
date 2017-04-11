using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace BookReader
{
    internal class BookTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PublicTemplate { get; set; }
        public DataTemplate ProtectedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is IBook)
            {
                IBook taskitem = item as IBook;
                if (taskitem.IsSecured)
                    return ProtectedTemplate;
                else
                    return PublicTemplate;
            }
            return null;
        }

    }
}
