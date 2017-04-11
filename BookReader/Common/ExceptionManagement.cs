using System;
using System.IO;

namespace BookReader.Common
{
	internal class ExceptionManagement
	{
		static internal DebugWindow _dlg;

		static public void Manage( string from, Exception error )
		{
			string errMessage = string.Format("{0}==> {1}; {2}", from, DateTime.Now, error.Message);

			if( Properties.Settings.Default.UseDebug )
			{
				if( _dlg == null )
					_dlg = new DebugWindow();

				_dlg.ExceptionContent = errMessage;
				_dlg.Show();
			}
			
            //allways log exceptions
			string file = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".exe", ".log");
			FileStream fs = File.Open( file , FileMode.Append, FileAccess.Write );

			using (StreamWriter sw = new StreamWriter(fs)) 
			{
				sw.WriteLine(errMessage);
			}

			fs.Close();
		}
	}
}
