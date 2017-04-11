using System;
using System.Windows.Media.Imaging;

namespace BookReader
{
	internal interface IBookItem
	{
        IBook Parent                { get; set; }
        string FilePath             { get; set; }
        string FileName             { get; set; }
        int Index                   { get; set; }
        BitmapImage Image           { get; set; }
        bool ImageExist             { get; }
        DateTime ImageLastAcces     { get; set; }
	}
}
