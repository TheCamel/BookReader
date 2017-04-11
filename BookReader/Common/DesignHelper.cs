using System.Diagnostics;

namespace BookReader.Common
{
	internal class DesignHelper
	{
		internal static bool IsInDesignMode()
		{
			bool returnFlag = false;
#if DEBUG
			if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
			{
				returnFlag = true;
			}
			else if (Process.GetCurrentProcess().ProcessName.ToUpper().Equals("DEVENV"))
			{
				returnFlag = true;
			}
#endif
			return returnFlag;
		}
	}
}
