/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Windows.Resources;
using System.Reflection;
using System.Xml.Linq;
using System.Net;
using System.Globalization;

namespace ESRI.ArcGIS.Mapping.Core.SatelliteResources
{
	/// <summary>
	/// LocalizedResources class.
	/// </summary>
	public class Xap
	{
		/// <summary>Resource manifest filename.</summary>
		private const string appResourceManifestFilename = "AppResourcesManifest.xaml";

		/// <summary>
		/// Check if culture is a supported language.
		/// </summary>
		/// <param name="culture">Culture string</param>
		/// <returns>true if supported, false otherwise.</returns>
		public bool IsSupportedLanguage(string culture)
		{
			foreach (string language in ESRI.ArcGIS.Mapping.Core.Constants.SupportedLanguages)
			{
				if (language.Equals(culture)) return true;
			}
			return false;
		}

		/// <summary>
		/// Load the resources files from the uri into process.
		/// </summary>
		/// <param name="UriResources">Uri to resource xap file.</param>
		/// <param name="eventHandler">Event handler to call after resources are loaded.</param>
		public void Load(string UriResources, EventHandler eventHandler)
		{
			if (string.IsNullOrEmpty(UriResources)) return;
			try
			{
				Uri addressUri = new Uri(UriResources, UriKind.RelativeOrAbsolute);
				WebClient webClient = new WebClient();
				webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(OpenReadResourceXapCompleted);
				webClient.OpenReadAsync(addressUri, eventHandler);
			}
			catch { };
		}

		/// <summary>
		/// Open Resource Xap file Complete hander.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event Args</param>
		private void OpenReadResourceXapCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			try
			{
				if ((e.Error == null) && (!e.Cancelled))
				{	// Load the Localized resources.
					StreamResourceInfo streamInfo = new StreamResourceInfo(e.Result, null);
					if (streamInfo != null)
					{	// Get resource Manifest file.
						Uri manifiestURl = new Uri(appResourceManifestFilename, UriKind.RelativeOrAbsolute);
						if (manifiestURl != null)
						{
							StreamResourceInfo resourceInfo = Application.GetResourceStream(streamInfo, manifiestURl);
							if (resourceInfo != null)
							{
								StreamReader resourceReader = new StreamReader(resourceInfo.Stream);
								if (resourceReader != null)
								{
									string appManifest = resourceReader.ReadToEnd();
									if (!String.IsNullOrEmpty(appManifest))
									{
										XElement resourcesElements = XDocument.Parse(appManifest).Root;
										if (resourcesElements != null)
										{	// Get names of all resources in manifest file.
											List<XElement> ResourceParts = new List<XElement>();
											foreach (XElement assemblyElement in resourcesElements.Elements().Elements())
												ResourceParts.Add(assemblyElement);

											// Load the resources.
											Assembly resourceAssembly = null;
											foreach (XElement resourceElement in ResourceParts)
											{
												string assemblyName = resourceElement.Attribute("Source").Value;
                                                if ((!string.IsNullOrEmpty(assemblyName)) && (assemblyName.ToLower().EndsWith("resources.dll", StringComparison.OrdinalIgnoreCase)))
												{	// only load resources assemblies
													AssemblyPart resourcePart = new AssemblyPart();
													StreamResourceInfo resourceStreamInfo = Application.GetResourceStream(new StreamResourceInfo(e.Result, "application/binary"), new Uri(assemblyName, UriKind.RelativeOrAbsolute));
													if (resourceStreamInfo != null)// Load resource assembles
														resourceAssembly = resourcePart.Load(resourceStreamInfo.Stream);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch { }
			finally
			{
				if (e.UserState != null)
				{
					if (e.UserState is EventHandler)
					{
						EventHandler callbackEvent = e.UserState as EventHandler;
						if (callbackEvent != null)
							callbackEvent(sender, new EventArgs());
					}
				}
			}
		}

	}
}
