using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows;

namespace BookReader.Common
{
	internal class ThemeHelper
	{
		/// <summary>
		/// Load a given theme resources based on the resource name discovered by the <see cref="DiscoverThemes"/> method />
		/// </summary>
		/// <param name="themeColor"></param>
		static public void LoadTheme(string themeColor)
		{
			Application.Current.Resources = (ResourceDictionary)Application.LoadComponent(
				new Uri(string.Format("Resources\\Themes\\{0}.xaml", themeColor), UriKind.Relative));
		}

		/// <summary>
		/// Loop in the resources to discover all theme names
		/// </summary>
		static public List<String> DiscoverThemes()
		{
			List<String> result = new List<string>();

			foreach (String resourceName in Application.ResourceAssembly.GetManifestResourceNames())
			{
				if (resourceName.ToLower().EndsWith(".g.resources"))
				{
					using (Stream stream = Application.ResourceAssembly.GetManifestResourceStream(resourceName))
					{
						using (ResourceReader reader = new ResourceReader(stream))
						{
							foreach (DictionaryEntry entry in reader)
							{
								Regex regex = new Regex(@"resources/themes/(\w+).baml");
								Match m = regex.Match(entry.Key.ToString());
								if (m.Success)
								{
									//add the theme to the list
									result.Add( m.Groups[1].Value.ToString() );
								}
							}
						}
					}
				}
			}
			
			return result;
		}
	}
}
