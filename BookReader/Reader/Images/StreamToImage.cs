using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace BookReader
{
	/// <summary>
	/// Class to manage image to stream conversion and revert
	/// </summary>
	internal class StreamToImage
	{
		/// <summary>
		/// Return a memory stream from a BitmapImage
		/// </summary>
		/// <param name="imageSource"></param>
		/// <returns></returns>
		static public MemoryStream GetStreamFromImage(BitmapImage imageSource)
		{
			MemoryStream memStream = new MemoryStream();
			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(imageSource));
			encoder.Save(memStream);
			return memStream;
		}

		/// <summary>
		/// Create a BitmapImage from a memory stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="?"></param>
		/// <returns></returns>
		static public BitmapImage GetImageFromStreamBug( MemoryStream stream )
		{
			return GetImageFromStreamBug( stream, 0 );
		}

		/// <summary>
		/// Create a BitmapImage from a stream with the specified width (height ratio kept) if not zero
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		static public BitmapImage GetImageFromStreamBug(MemoryStream stream, int width )
		{
			MemoryStream stream2 = new MemoryStream();
			stream.WriteTo(stream2);
			stream.Flush();
			stream.Close();
			stream2.Position = 0;

			BitmapImage myImage = new BitmapImage();
			myImage.BeginInit();
			myImage.StreamSource = stream2;
			if (width != 0)
				myImage.DecodePixelWidth = width;
			myImage.EndInit();

			//stream.Close();
			stream = null;
			stream2 = null;

			return myImage;
		}

		/// <summary>
		/// Create a BitmapImage from a stream with the specified width (height ratio kept) if not zero
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="resize"></param>
		/// <returns></returns>
		static public BitmapImage GetImageFromStream( Stream stream, int width )
		{
			BitmapImage myImage = new BitmapImage();
			myImage.BeginInit();
			myImage.StreamSource = stream;
			if (width != 0)
				myImage.DecodePixelWidth = width;
			myImage.EndInit();

			return myImage;
		}

	}
}
