using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace BookReader.Reader.Common
{
    public class SearchFilter
    {
        public SearchFilter( ICollectionView filteredView, TextBox textBox )
		{
			string filterText = "";

			filteredView.Filter = delegate( object obj )				
			{
				if( String.IsNullOrEmpty( filterText ) )
					return true;

                IBook bk = obj as IBook;
				if( bk == null )
					return false;

				int index = bk.FileName.IndexOf(
					filterText,
					0,
					StringComparison.InvariantCultureIgnoreCase );

				return index > -1;
			};			

			textBox.TextChanged += delegate
			{
				filterText = textBox.Text;
				filteredView.Refresh();
			};
		}
    }
}
