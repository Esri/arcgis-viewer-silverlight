/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Printing;
using System.Xml.Serialization;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Printing;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcGIS.Client.WebMap;

namespace PrintTool.AddIns
{
	/// <summary>
	/// Represents the ViewModel for interacting with PrintTool in a mapping application.
	/// </summary>
	[DataContract]
	public class PrintToolViewModel : INotifyPropertyChanged
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PrintToolViewModel"/> class.
		/// </summary>
		public PrintToolViewModel()
		{
			// Default values when ArcGIS Server printing is not used
			IsTitleVisible = IsDescriptionVisible = IsWidthVisible = IsHeightVisible = IsPrintLayoutVisible = true;
			IsAuthorVisible = IsCopyrightTextVisible = IsLayoutTemplatesVisible = IsFormatsVisible = IsUseScaleVisible = true;

			PrintWidthString = PrintHeightString = "650";
			Title = Resources.Strings.Untitled;
			CopyrightText = null;

			// NOTE: Replace with your own ArcGIS 10.1 Server Printing Tool Task URL
			PrintTaskURL = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/PrintingTools/GPServer/Export%20Web%20Map%20Task";			
						
			IsValid = false;

			LoadSilverlightPrintLayoutTemplates();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PrintToolViewModel"/> class.
		/// </summary>
		/// <param name="other">The other.</param>
		public PrintToolViewModel(PrintToolViewModel other)
		{
			CopyAllSettings(other);

			Error = null;
			Status = null;
		}

		#endregion
		
		#region Properties

		/// <summary>
		/// List of serializable layers supported in ArcGIS API for Silverlight v3.0.
		/// </summary>
		private static List<Type> SerializableLayers = new List<Type>(new Type[] {
			typeof(ArcGISTiledMapServiceLayer),	typeof(ArcGISDynamicMapServiceLayer), typeof(ArcGISImageServiceLayer), 
			typeof(FeatureLayer), typeof(GPResultImageLayer), typeof(GraphicsLayer), typeof(KmlLayer), 
			typeof(OpenStreetMapLayer), typeof(WmsLayer), typeof(WmtsLayer), typeof(TileLayer)});
				
		/// <summary>
		/// Value indicating whether this model is used for configuration.
		/// </summary>
		private bool isConfig = true;

		/// <summary>
		/// Value indicating whether the map scale input is valid.
		/// </summary>
		private bool isMapScaleInputValid = false;

		/// <summary>
		/// Value indicating whether visibility change was performed by this PrintViewModel.
		/// </summary>
		private bool ignoreVisibilityChange = false;

        /// <summary>
        /// Tracks whether service metadata for the current specified print service has been loaded
        /// </summary>
        private bool serviceLoaded = false;

		/// <summary>
		/// Print content to be submitted for printing.
		/// </summary>
		private UIElement printContent;

		private ContentControl _printPreview;
		/// <summary>
		/// Gets the print preview.
		/// </summary>
		private ContentControl printPreview
		{
			get
			{
				if (_printPreview == null)
				{
					_printPreview = new ContentControl() { DataContext = this };
				}
				return _printPreview;
			}
		}

        private List<Layer> _nonSerializableLayers;
        /// <summary>
        /// Gets the layers that are not serializable in ArcGIS API for Silverlight v3.0		
        /// </summary>
        private List<Layer> nonSerializableLayers
        {
            get
            {
                if (_nonSerializableLayers == null)
                {
                    _nonSerializableLayers = new List<Layer>();
                }
                return _nonSerializableLayers;
            }
        }

		private PrintTask _printTask;
		/// <summary>
		/// Gets the <see cref="ESRI.ArcGIS.Client.Printing.PrintTask"/> used by the ViewModel
		/// </summary>
		private PrintTask printTask
		{
			get
			{
				if (_printTask == null)
				{
					_printTask = new PrintTask();
				}
				if (UseProxy && MapApplication.Current != null && MapApplication.Current.Urls != null)
					_printTask.ProxyURL = MapApplication.Current.Urls.ProxyUrl;
				else
					_printTask.ProxyURL = null;
				return _printTask;
			}
		}

		private PrintDocument _printDocument;
		/// <summary>
		/// Gets the <see cref="System.Windows.Printing.PrintDocument"/> used by the ViewModel
		/// </summary>
		private PrintDocument printDocument
		{
			get
			{
				if (_printDocument == null)
				{
					_printDocument = new PrintDocument();
				}
				return _printDocument;
			}
		}

		private Map map;
		/// <summary>
		/// Gets or sets the <see cref="ESRI.ArcGIS.Client.Map"/> used by the ViewModel.
		/// </summary>
		public Map Map
		{
			get { return map; }
			set
			{
				if (map != value)
				{
					UnhookMapEvents();
					map = value;
					OnPropertyChanged("Map");
					HookupMapEvents();
					PrintChanged();
					PrintCommandChanged();
					GenerateCopyright();
				}
			}
		}

		private string autoGeneratedCopyrightText;
		/// <summary>
		/// Gets or sets the auto-generated copyright text on the print layout.
		/// </summary>
		public string AutoGeneratedCopyrightText
		{
			get { return autoGeneratedCopyrightText; }
			set
			{
				if (autoGeneratedCopyrightText != value)
				{
					if (CopyrightText == null || CopyrightText.Equals(autoGeneratedCopyrightText))
						CopyrightText = value;
					autoGeneratedCopyrightText = value;
					OnPropertyChanged("AutoGeneratedCopyrightText");
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether print tool has at least one visible setting.
		/// </summary>
		public bool HasVisibleSetting
		{
			get
			{
				return IsTitleVisible ||
					(!PrintWithArcGISServer && (IsDescriptionVisible || IsWidthVisible || IsHeightVisible || IsPrintLayoutVisible)) ||
				(PrintWithArcGISServer && (IsAuthorVisible || IsCopyrightTextVisible || IsLayoutTemplatesVisible || IsFormatsVisible || IsUseScaleVisible));
			}
		}
		
		private bool isValid;
		/// <summary>
		/// Gets a value indicating whether this instance is valid.
		/// </summary>
		public bool IsValid
		{
			get { return isValid; }
			private set
			{
				if (isValid != value)
				{
					isValid = value;
					OnPropertyChanged("IsValid");
					PrintChanged();
				}
			}
		}

		private Exception error;
		/// <summary>
		/// Gets the current print error.
		/// </summary>
		public Exception Error
		{
			get { return error; }
			private set
			{
				if (error != value)
				{
					error = value;
					OnPropertyChanged("Error");
					if (error != null)
						Status = null;
				}
			}
		}

		private string status;
		/// <summary>
		/// Gets the current print status.
		/// </summary>
		public string Status
		{
			get { return status; }
			private set
			{
				if (status != value)
				{
					status = value;
					OnPropertyChanged("Status");
					if (!string.IsNullOrEmpty(status))
						Error = null;
				}
			}
		}

		private bool isBusy;
		/// <summary>
		/// Gets a value indicating whether an operation is processing.
		/// </summary>
		public bool IsBusy
		{
			get { return isBusy; }
			private set
			{
				if (isBusy != value)
				{
					isBusy = value;
					OnPropertyChanged("IsBusy");
				}
			}
		}

		private bool isPrinting;
		/// <summary>
		/// Gets a value indicating whether print tool is already printing.
		/// </summary>
		public bool IsPrinting
		{
			get { return isPrinting; }
			private set
			{
				if (isPrinting != value)
				{
					isPrinting = value;
					OnPropertyChanged("IsPrinting");
				}
			}
		}

		private bool isMapReady;
		/// <summary>
		/// Gets a value indicating whether map is ready for printing.
		/// </summary>
		public bool IsMapReady
		{
			get { return isMapReady; }
			private set
			{
				if (isMapReady != value)
				{
					isMapReady = value;
					OnPropertyChanged("IsMapReady");
				}
			}
		}

		/// <summary>
		/// Gets the print image that will be sent for printing.
		/// </summary>
		public ImageSource PrintImage
		{
			get { return new WriteableBitmap(Map, null); }
		}

		#region DataContractSerializable Properties

		private bool isServiceAsynchronous;
		/// <summary>
		/// Gets or sets a value indicating whether the print service is asynchronous.
		/// </summary>
		[DataMember]
		public bool IsServiceAsynchronous
		{
			get { return isServiceAsynchronous; }
			set
			{
				if (isServiceAsynchronous != value)
				{
					isServiceAsynchronous = value;
					OnPropertyChanged("IsServiceAsynchronous");
				}
			}
		}
		
		private bool printWithArcGISServer;
		/// <summary>
		/// Gets or sets a value indicating whether the printing with ArcGIS Server is enabled.
		/// </summary>
		[DataMember]
		public bool PrintWithArcGISServer
		{
			get { return printWithArcGISServer; }
			set
			{
                UnhookPrintEvents();
                UnhookMapEvents();
                if (printWithArcGISServer != value)
				{
					printWithArcGISServer = value;
					OnPropertyChanged("PrintWithArcGISServer");
				}
                HookupMapEvents();
                HookupPrintEvents();

                // Loads print layout based on PrintWithArcGISServer value
                if (printWithArcGISServer)
                {
                    IsValid = false;
                    if (isConfig) LoadPrintServiceInformation();
                    if (Map != null)
                    {
                        CheckLayerSupport();
                        MapScaleString = Convert.ToString(Math.Round(Map.Scale, 0), CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    LoadSilverlightPrintLayoutTemplates();
                    IsValid = PrintLayout != null && PrintHeight > 0 && PrintWidth > 0;
                }

                if (!m_deserializing)
                    IsDescriptionVisible = IsWidthVisible = IsHeightVisible = IsPrintLayoutVisible = !printWithArcGISServer;

                LoadChanged();
                PrintChanged();
			}
		}

		private bool useProxy;
		/// <summary>
		/// Gets or sets a value indicating whether the application proxy is used.
		/// </summary>
		[DataMember]
		public bool UseProxy
		{
			get { return useProxy; }
			set
			{
				if (useProxy != value)
				{
					useProxy = value;
					OnPropertyChanged("UseProxy");					
					if (PrintWithArcGISServer)
					{
						IsValid = false;
					}

                    // Reset flag indicating whether currently specified service has been loaded
                    serviceLoaded = false;

                    // Raise state change notification for load command
                    LoadChanged();	
				}
			}
		}

		private string printTaskURL;

		/// <summary>
		/// Gets or sets the print task URL.
		/// </summary>
		[DataMember]
		public string PrintTaskURL
		{
			get { return printTaskURL; }
			set
			{
				if (printTaskURL != value)
				{
					printTaskURL = value;
					OnPropertyChanged("PrintTaskURL");	
					printTask.Url = printTaskURL;
					if (PrintWithArcGISServer)
					{
						IsValid = false;
					}

                    // Reset flag indicating whether currently specified service has been loaded
                    serviceLoaded = false;

                    // Raise state change notification for load command
                    LoadChanged();				
				}
			}
		}

		private string title;
		/// <summary>
		/// Gets or sets the title text on the print layout.
		/// </summary>
		[DataMember]
		public string Title
		{
			get { return title; }
			set
			{
				if (title != value)
				{
					title = value;
					OnPropertyChanged("Title");
				}
			}
		}

		private bool isTitleVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the title field is visible on the print tool.
		/// </summary>
		[DataMember]
		public bool IsTitleVisible
		{
			get { return isTitleVisible; }
			set
			{
				if (isTitleVisible != value)
				{
					isTitleVisible = value;
					OnPropertyChanged("IsTitleVisible");
				}
			}
		}

		private string description;
		/// <summary>
		/// Gets or sets the description text on the print layout.
		/// </summary>
		[DataMember]
		public string Description
		{
			get { return description; }
			set
			{
				if (description != value)
				{
					description = value;
					OnPropertyChanged("Description");
				}
			}
		}

		private bool isDescriptionVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the description field is visible on the print tool.
		/// </summary>
		[DataMember]
		public bool IsDescriptionVisible
		{
			get { return isDescriptionVisible; }
			set
			{
				if (isDescriptionVisible != value)
				{
					isDescriptionVisible = value;
					OnPropertyChanged("IsDescriptionVisible");
				}
			}
		}

		private string printWidthString;
		/// <summary>
		/// Gets or sets the width of the print layout.
		/// </summary>
		[DataMember]
		public string PrintWidthString
		{
			get { return printWidthString; }
			set
			{
				if (printWidthString != value)
				{
					printWidthString = value;
					OnPropertyChanged("PrintWidthString");
					if (!PrintWithArcGISServer)
						IsValid = false;
					if (string.IsNullOrEmpty(value))
						throw new ArgumentException(Resources.Strings.RequiredFieldException, "PrintWidth");
					var doubleValue = PrintWidth;
					if (!double.TryParse(value, out doubleValue) || doubleValue < 0)
						throw new ArgumentException(Resources.Strings.SizeFieldException, "PrintWidth");
					PrintWidth = doubleValue;
				}
			}
		}

		private double printWidth;
		/// <summary>
		/// Gets or sets the width of the print layout.
		/// </summary>
		public double PrintWidth
		{
			get { return printWidth; }
			set
			{
				if (printWidth != value)
				{
					printWidth = value;
					OnPropertyChanged("PrintWidth");
					if (!PrintWithArcGISServer)
						IsValid = PrintLayout != null && PrintHeight > 0 && PrintWidth > 0;
					PrintChanged();
				}
			}
		}

		private bool isWidthVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the width field is visible on the print tool.
		/// </summary>
		[DataMember]
		public bool IsWidthVisible
		{
			get { return isWidthVisible; }
			set
			{
				if (isWidthVisible != value)
				{
					isWidthVisible = value;
					OnPropertyChanged("IsWidthVisible");
				}
			}
		}

		private string printHeightString;
		/// <summary>
		/// Gets or sets the height of the print layout.
		/// </summary>
		[DataMember]
		public string PrintHeightString
		{
			get { return printHeightString; }
			set
			{
				if (printHeightString != value)
				{
					printHeightString = value;
					OnPropertyChanged("PrintHeightString");
					if (!PrintWithArcGISServer)
						IsValid = false;
					if (string.IsNullOrEmpty(value))
						throw new ArgumentException(Resources.Strings.RequiredFieldException, "PrintHeight");
					var doubleValue = PrintHeight;
					if (!double.TryParse(value, out doubleValue) || doubleValue < 0)
						throw new ArgumentException(Resources.Strings.SizeFieldException, "PrintHeight");
					PrintHeight = doubleValue;
				}
			}
		}

		private double printHeight;
		/// <summary>
		/// Gets or sets the height of the print layout.
		/// </summary>
		public double PrintHeight
		{
			get { return printHeight; }
			set
			{
				if (printHeight != value)
				{
					printHeight = value;
					OnPropertyChanged("PrintHeight");
					if (!PrintWithArcGISServer)
						IsValid = PrintLayout != null && PrintHeight > 0 && PrintWidth > 0;
					PrintChanged();
				}
			}
		}

		private bool isHeightVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the height field is visible on the print tool.
		/// </summary>
		[DataMember]
		public bool IsHeightVisible
		{
			get { return isHeightVisible; }
			set
			{
				if (isHeightVisible != value)
				{
					isHeightVisible = value;
					OnPropertyChanged("IsHeightVisible");
				}
			}
		}
		
		private PrintLayout printLayout;
		/// <summary>
		/// Gets or sets the default print layout when ArcGIS Server printing is not used.
		/// </summary>
		[DataMember]
		public PrintLayout PrintLayout
		{
			get { return printLayout; }
			set
			{
				if (printLayout != value)
				{
					printLayout = value;
					OnPropertyChanged("PrintLayout");	
					if (!PrintWithArcGISServer)
						IsValid = PrintLayout != null && PrintHeight > 0 && PrintWidth > 0;
					PrintChanged();
				}
			}
		}

		private bool isPrintLayoutVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the print layout picker is visible in the print tool.
		/// </summary>
		[DataMember]
		public bool IsPrintLayoutVisible
		{
			get { return isPrintLayoutVisible; }
			set
			{
				if (isPrintLayoutVisible != value)
				{
					isPrintLayoutVisible = value;
					OnPropertyChanged("IsPrintLayoutVisible");
				}
			}
		}
		
		private string author;
		/// <summary>
		/// Gets or sets the author text on the print layout.
		/// </summary>
		[DataMember]
		public string Author
		{
			get { return author; }
			set
			{
				if (author != value)
				{
					author = value;
					OnPropertyChanged("Author");
				}
			}
		}

		private bool isAuthorVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the author field is visible on the print tool.
		/// </summary>
		/// <value>
		[DataMember]
		public bool IsAuthorVisible
		{
			get { return isAuthorVisible; }
			set
			{
				if (isAuthorVisible != value)
				{
					isAuthorVisible = value;
					OnPropertyChanged("IsAuthorVisible");
				}
			}
		}

		private string copyrightText;
		/// <summary>
		/// Gets or sets the copyright text on the print layout.
		/// </summary>
		[DataMember]
		public string CopyrightText
		{
			get { return copyrightText; }
			set
			{
				if (copyrightText != value)
				{
					copyrightText = value;
					OnPropertyChanged("CopyrightText");
				}
			}
		}

		private bool isCopyrightTextVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the copyright field is visible on the print layout.
		/// </summary>
		[DataMember]
		public bool IsCopyrightTextVisible
		{
			get { return isCopyrightTextVisible; }
			set
			{
				if (isCopyrightTextVisible != value)
				{
					isCopyrightTextVisible = value;
					OnPropertyChanged("IsCopyrightTextVisible");
				}
			}
		}

		private List<string> layoutTemplates;
		/// <summary>
		/// Gets or sets the choice list for LayoutTemplate.
		/// </summary>
		[DataMember]
		public List<string> LayoutTemplates
		{
			get { return layoutTemplates; }
			set
			{
				if (layoutTemplates != value)
				{
					layoutTemplates = value;
					OnPropertyChanged("LayoutTemplates");
				}
			}
		}

		private string layoutTemplate;
		/// <summary>
		/// Gets or sets the default or selected layout template.
		/// </summary>
		[DataMember]
		public string LayoutTemplate
		{
			get { return layoutTemplate; }
			set
			{
				if (layoutTemplate != value)
				{
					layoutTemplate = value;
					OnPropertyChanged("LayoutTemplate");
				}
			}
		}

		private bool isLayoutTemplatesVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the layout template selector is visible in the print tool.
		/// </summary>
		[DataMember]
		public bool IsLayoutTemplatesVisible
		{
			get { return isLayoutTemplatesVisible; }
			set
			{
				if (isLayoutTemplatesVisible != value)
				{
					isLayoutTemplatesVisible = value;
					OnPropertyChanged("IsLayoutTemplatesVisible");
				}
			}
		}

		private List<string> formats;
		/// <summary>
		/// Gets or sets the choice list for Format.
		/// </summary>
		[DataMember]
		public List<string> Formats
		{
			get { return formats; }
			set
			{
				if (formats != value)
				{
					formats = value;
					OnPropertyChanged("Formats");
				}
			}
		}

		private string format;
		/// <summary>
		/// Gets or sets the default or selected format.
		/// </summary>
		[DataMember]
		public string Format
		{
			get { return format; }
			set
			{
				if (format != value)
				{
					format = value;
					OnPropertyChanged("Format");
				}
			}
		}

		private bool isFormatsVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the format selector is visible in the print tool.
		/// </summary>
		[DataMember]
		public bool IsFormatsVisible
		{
			get { return isFormatsVisible; }
			set
			{
				if (isFormatsVisible != value)
				{
					isFormatsVisible = value;
					OnPropertyChanged("IsFormatsVisible");
				}
			}
		}
		
		private bool useScale;
		/// <summary>
		/// Gets or sets a value indicating whether the current map scale is used.
		/// </summary>
		[DataMember]
		public bool UseScale
		{
			get { return useScale; }
			set
			{
				if (useScale != value)
				{
					useScale = value;
					OnPropertyChanged("UseScale");
					if (PrintWithArcGISServer)
					{
						if (!useScale && Map != null)
							MapScaleString = Convert.ToString(Math.Round(Map.Scale, 0), CultureInfo.InvariantCulture);
						IsValid = !UseScale || MapScale > 0;
					}
					PrintChanged();
				}
			}
		}

		private bool isUseScaleVisible;
		/// <summary>
		/// Gets or sets a value indicating whether the use scale option is visible in the print tool.
		/// </summary>
		[DataMember]
		public bool IsUseScaleVisible
		{
			get { return isUseScaleVisible; }
			set
			{
				if (isUseScaleVisible != value)
				{
					isUseScaleVisible = value;
					OnPropertyChanged("IsUseScaleVisible");
				}
			}
		}

		private string mapScaleString;
		/// <summary>
		/// Gets or sets the default or current map scale.
		/// </summary>
		[DataMember]
		public string MapScaleString
		{
			get { return mapScaleString; }
			set
			{
				if (mapScaleString != value)
				{
					mapScaleString = value;
					OnPropertyChanged("MapScaleString");
					if (PrintWithArcGISServer)
						IsValid = false;
					isMapScaleInputValid = false;
					PrintChanged();
					if (UseScale && string.IsNullOrEmpty(value))
						throw new ArgumentException(Resources.Strings.RequiredFieldException, "MapScale");
					var doubleValue = MapScale;
					if (!double.TryParse(value, out doubleValue) || doubleValue < 0)
						throw new ArgumentException(Resources.Strings.SizeFieldException, "MapScale");
					isMapScaleInputValid = true;
					PrintChanged();
					MapScale = doubleValue;
				}
			}
		}

		private double mapScale;
		/// <summary>
		/// Gets or sets the default or current map scale.
		/// </summary>
		public double MapScale
		{
			get { return mapScale; }
			set
			{
				if (mapScale != value)
				{
					mapScale = value;
					OnPropertyChanged("MapScale");
					if (PrintWithArcGISServer)
						IsValid = !UseScale || MapScale > 0;
					PrintChanged();
				}
			}
		}

        private bool isDpiVisible;
        /// <summary>
        /// Gets or sets a value indicating whether the DPI setting is visible in the print tool.
        /// </summary>
        [DataMember]
        public bool IsDpiVisible
        {
            get { return isDpiVisible; }
            set
            {
                if (isDpiVisible != value)
                {
                    isDpiVisible = value;
                    OnPropertyChanged("IsDpiVisible");
                }
            }
        }

        private string dpiString;
        /// <summary>
        /// Gets or sets the default or current DPI
        /// </summary>
        [DataMember]
        public string DpiString
        {
            get { return dpiString; }
            set
            {
                if (dpiString != value)
                {
                    dpiString = value;
                    OnPropertyChanged("DpiString");
                    if (PrintWithArcGISServer)
                        IsValid = false;
                    PrintChanged();
                    var doubleValue = Dpi;
                    if (string.IsNullOrEmpty(value))
                        doubleValue = 96; // 96 is default
                    else if (!double.TryParse(value, out doubleValue) || doubleValue < 0)
                        throw new ArgumentException(Resources.Strings.SizeFieldException, "Dpi");
                    PrintChanged();
                    Dpi = doubleValue;
                }
            }
        }

        private double dpi = 96;
        /// <summary>
        /// Gets or sets the default or current DPI
        /// </summary>
        public double Dpi
        {
            get { return dpi; }
            set
            {
                if (dpi != value)
                {
                    dpi = value;
                    OnPropertyChanged("Dpi");
                    PrintChanged();
                }

                if (PrintWithArcGISServer)
                    IsValid = Dpi > 0;
            }
        }

		private PrintLayouts printLayouts;
		public PrintLayouts PrintLayouts
		{
			get { return printLayouts; }
			set
			{
				if (printLayouts != value)
				{
					printLayouts = value;
					OnPropertyChanged("PrintLayouts");
				}
			}
		}

		#endregion

		#region Commands

		private DelegateCommand loadCommand;
		/// <summary>
		/// Command used to load print service information (i.e. LayoutTemplates, Formats).
		/// </summary>
		public ICommand Load
		{
			get
			{
				if (loadCommand == null)
				{
					loadCommand = new DelegateCommand(OnLoad, CanLoad);
				}
				return loadCommand;
			}
		}

		private void LoadChanged()
		{
			if (loadCommand != null)
			{
				loadCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanLoad(object commandParameter)
		{
			return !PrintWithArcGISServer ||
				(PrintWithArcGISServer && !string.IsNullOrEmpty(PrintTaskURL) && !IsBusy && !serviceLoaded);
		}

		/// <summary>
		/// Gets the print service information.
		/// </summary>
		private void OnLoad(object commandParameter)
		{
			if (!CanLoad(commandParameter)) return;
			IsBusy = true;
			Status = Resources.Strings.GetLayoutTemplatesStarted;
			if (PrintWithArcGISServer)
			{
                // Raise execution state change for load command
                LoadChanged();
				LoadPrintServiceInformation(true);
			}
			else
			{
				LoadSilverlightPrintLayoutTemplates(true);		
			}
		}
		
		private DelegateCommand print;
		/// <summary>
		/// Command used to execute or submit print job to ArcGIS Server.
		/// </summary>
		public ICommand Print
		{
			get
			{
				if (print == null)
				{
					print = new DelegateCommand(OnPrint, CanPrint);
				}
				return print;
			}
		}

		private void PrintChanged()
		{
			if (print != null)
			{
				print.RaiseCanExecuteChanged();
			}
		}

		private bool CanPrint(object commandParameter)
		{
			return Map != null && ((!PrintWithArcGISServer && PrintLayout != null && PrintHeight > 0 && PrintWidth > 0)
				|| (PrintWithArcGISServer && !printTask.IsBusy && (!UseScale ||  (isMapScaleInputValid && MapScale > 0))));
		}

		/// <summary>
		/// Executes or submit print job to ArcGIS server or displays print preview when ArcGIS server printing is not used.
		/// </summary>
		private void OnPrint(object commandParameter)
		{
			if (!CanPrint(commandParameter)) return;
			CheckLayerSupport();
			IsBusy = true;
			Status = Resources.Strings.PrintStarted;
			if (PrintWithArcGISServer)
			{
				IsPrinting = true;
                var printParameters = new PrintParameters(map)
                {
                    Format = Format,
                    LayoutTemplate = LayoutTemplate,

                    LayoutOptions = new LayoutOptions()
                    {
                        Title = Title,
                        AuthorText = Author,
                        Copyright = CopyrightText
                    },
                    MapOptions = new MapOptions(map)
                    {
                        Scale = UseScale ? MapScale : double.NaN
                    },
                    ExportOptions = new ExportOptions()
                    {
                        Dpi = Dpi
                    }
                };
                if (LayoutTemplate == "MAP_ONLY")
                    printParameters.ExportOptions.OutputSize = new Size(map.ActualWidth, map.ActualHeight);

				try
				{
					// Turns off layer's visibility for layers that are not supported 
					// to avoid exception thrown during map serialization.
					UpdateLayersNotSupported(false);

                    // Hide basemap layers from legend so they are excluded from the print legend
                    ToggleBasemapLegend(false);

                    // Initialize layers with default legend labels
                    InitializeLegendLabels(); 
					if (IsServiceAsynchronous)
						printTask.SubmitJobAsync(printParameters);
					else
						printTask.ExecuteAsync(printParameters);

                    // Toggle visibility for layers that are not supported in printing back on
					UpdateLayersNotSupported(true);

                    // Turn legend for basemap layers back on
                    ToggleBasemapLegend(true);
                }
				catch (Exception ex)
				{
					Error = ex;
					IsBusy = false;
					// Turns on layer's visibility for layers that are not supported.
					UpdateLayersNotSupported(true);
				}
			}
			else
			{
				Status = Resources.Strings.GetPrintLayoutXamlStarted;
				var layoutXamlWebClient = new WebClient();
				layoutXamlWebClient.DownloadStringCompleted += LayoutXamlWebClient_DownloadStringCompleted;
				layoutXamlWebClient.DownloadStringAsync(new Uri(new Uri(MapApplication.Current.Urls.BaseUrl), PrintLayout.XamlFilePath));
			}
		}

		private DelegateCommand printCommand;
		/// <summary>
		/// Command used from the print preview dialog.
		/// </summary>
		public ICommand PrintCommand
		{
			get
			{
				if (printCommand == null)
				{
					printCommand = new DelegateCommand(OnPrintCommand, CanPrintCommand);
				}
				return printCommand;
			}
		}

		private void PrintCommandChanged()
		{
			if (printCommand != null)
			{
				printCommand.RaiseCanExecuteChanged();
			}
		}

		private bool CanPrintCommand(object commandParameter)
		{
			return Map != null && Map.Layers !=null && commandParameter is UIElement;
		}
		
		/// <summary>
		/// Sets print content and invokes print from PrintDocument.
		/// </summary>
		private void OnPrintCommand(object commandParameter)
		{
			if (!CanPrintCommand(commandParameter)) return;
			IsBusy = true;
			IsPrinting = false;
			Status = Resources.Strings.PrintStarted;
			printContent = commandParameter as UIElement;
			printDocument.Print(string.IsNullOrEmpty(Title) ? Resources.Strings.Untitled : Title);
		}

		#endregion

		#endregion

		#region Public Methods

		/// <summary>
		/// Applies the changes using source model.
		/// </summary>
		/// <param name="sourceModel">The source model.</param>
		public void ApplyChanges(PrintToolViewModel sourceModel)
		{
			CopyAllSettings(sourceModel);
			Status = null;
			Error = null;
		}
		
		/// <summary>
		/// Activates the PrintTool by setting properties to initial values and subscribing to PrintTask events.
		/// </summary>
		public void Activate()
		{
			isConfig = false;
			Status = null;
			Error = null;
			IsBusy = false;
			IsMapReady = true;
			PrintChanged();
            CheckLayerSupport();
		}
		
		#endregion

		#region Helper Methods and Event Handlers

		/// <summary>
		/// Copies all settings.
		/// </summary>
		/// <param name="source">The source.</param>
		private void CopyAllSettings(PrintToolViewModel source)
		{
			if (source != null)
			{
				this.IsServiceAsynchronous = source.IsServiceAsynchronous;
				this.PrintWithArcGISServer = source.PrintWithArcGISServer;
				this.UseProxy = source.UseProxy;
				this.PrintTaskURL = source.PrintTaskURL;
				this.Title = source.Title;
				this.IsTitleVisible = source.IsTitleVisible;
				this.Description = source.Description;
				this.IsDescriptionVisible = source.IsDescriptionVisible;				
				this.PrintWidthString = Convert.ToString(source.PrintWidth, CultureInfo.InvariantCulture);
				this.IsWidthVisible = source.IsWidthVisible;
				this.PrintHeightString = Convert.ToString(source.PrintHeight, CultureInfo.InvariantCulture);
				this.IsHeightVisible = source.IsHeightVisible;
				this.PrintLayout = source.PrintLayout;
				this.IsPrintLayoutVisible = source.IsPrintLayoutVisible;
				this.CopyrightText = source.CopyrightText;
				this.IsCopyrightTextVisible = source.IsCopyrightTextVisible;
				this.Author = source.Author;
				this.IsAuthorVisible = source.IsAuthorVisible;
				this.LayoutTemplates = source.LayoutTemplates;
				this.LayoutTemplate = source.LayoutTemplate;
				this.IsLayoutTemplatesVisible = source.IsLayoutTemplatesVisible;
				this.Formats = source.Formats;
				this.Format = source.Format;
				this.IsFormatsVisible = source.IsFormatsVisible;
				this.UseScale = source.UseScale;
				this.MapScaleString = Convert.ToString(Math.Round(source.MapScale, 0), CultureInfo.InvariantCulture);
                this.IsDpiVisible = source.IsDpiVisible;
                this.DpiString = source.DpiString;
                this.IsUseScaleVisible = source.isUseScaleVisible;
			}
		}

		private void UnhookMapEvents()
		{
			if (Map != null)
			{
				if (PrintWithArcGISServer)
				{
					Map.PropertyChanged -= Map_PropertyChanged;
				}
				else
				{
					Map.Progress -= Map_Progress;
				}
				if (Map.Layers != null)
				{
					Map.Layers.CollectionChanged -= Layers_CollectionChanged;
					foreach (var l in Map.Layers)
					{
						l.Initialized -= Layer_Initialized;
						l.PropertyChanged -= Layer_PropertyChanged;
					}
				}
			}
		}

		private void UnhookPrintEvents()
		{
			if (PrintWithArcGISServer)
			{
				if (IsServiceAsynchronous)
				{
					printTask.StatusUpdated -= PrintTask_StatusUpdated;
					printTask.JobCompleted -= PrintTask_JobCompleted;
				}
				else
					printTask.ExecuteCompleted -= PrintTask_ExecuteCompleted;
			}
			else
			{
				printDocument.BeginPrint -= PrintDocument_BeginPrint;
				printDocument.EndPrint -= PrintDocument_EndPrint;
				printDocument.PrintPage -= PrintDocument_PrintPage;
			}
		}

		private void HookupMapEvents()
		{
			if (Map != null)
			{
				if (PrintWithArcGISServer)
				{
					Map.PropertyChanged += Map_PropertyChanged;
				}
				else
				{
					Map.Progress += Map_Progress;
				}
				if (Map.Layers != null)
				{
					Map.Layers.CollectionChanged += Layers_CollectionChanged;
					foreach (var l in Map.Layers)
					{
						l.Initialized += Layer_Initialized;
						l.PropertyChanged += Layer_PropertyChanged;
					}
				}
			}
		}

		private void HookupPrintEvents()
		{
			if (PrintWithArcGISServer)
			{
				if (IsServiceAsynchronous)
				{
					printTask.StatusUpdated += PrintTask_StatusUpdated;
					printTask.JobCompleted += PrintTask_JobCompleted;
				}
				else
					printTask.ExecuteCompleted += PrintTask_ExecuteCompleted;
			}
			{
				printDocument.BeginPrint += PrintDocument_BeginPrint;
				printDocument.EndPrint += PrintDocument_EndPrint;
				printDocument.PrintPage += PrintDocument_PrintPage;
			}
		}

		/// <summary>
		/// Loads print service information (choice list for layouts and formats) 
		/// where showStatus indicates if Status should be displayed.
		/// </summary>
		private void LoadPrintServiceInformation(bool showStatus = false)
		{
			try
			{
				printTask.GetServiceInfoCompleted += PrintTask_GetServiceInfoCompleted;
				printTask.GetServiceInfoAsync(showStatus);
			}
			catch (Exception ex)
			{
				Error = ex;
				IsBusy = false;
			}
		}
		
		/// <summary>
		/// Loads Silverlight print layout templates where showPreview indicates if PrintLayoutPicker should be displayed. 
		/// By default, first print layout is used.
		/// </summary>
		private void LoadSilverlightPrintLayoutTemplates(bool showPreview = false)
		{
			var printLayoutsWebClient = new WebClient();
			printLayoutsWebClient.DownloadStringCompleted += PrintLayoutsWebClient_DownloadStringCompleted;
			printLayoutsWebClient.DownloadStringAsync(new Uri(new Uri(MapApplication.Current.Urls.BaseUrl), "./Config/Print/PrintLayoutList.xml"), showPreview);
		}

		/// <summary>
		/// Generates the copyright text based on layer types.
		/// </summary>
		private void GenerateCopyright()
		{
			var hasEsriLayer = Map.Layers.Any(l => l is ArcGISDynamicMapServiceLayer || l is ArcGISImageServiceLayer || l is ArcGISTiledMapServiceLayer);
			var hasBingLayer = Map.Layers.Any(l => l is TileLayer);
			var hasOpenStreetMap = Map.Layers.Any(l => l is OpenStreetMapLayer);
			var sb = new StringBuilder();
			if (hasEsriLayer)
				sb.Append("Sources: USGS, FAO, NPS, EPA, ESRI, DeLorme, TANA, and other suppliers");
			if (hasBingLayer)
				sb.AppendFormat("{0}© 2010 Microsoft Corporation and its data suppliers", hasEsriLayer ? "; " : string.Empty);
			if (hasOpenStreetMap)
				sb.AppendFormat("{0}© OpenStreetMap (and) contributors, CC-BY-SA", hasEsriLayer || hasBingLayer ? "; " : string.Empty);
			// Set to null if empty to distinguish between CopyrightText that was configured to be empty 
			// and CopyrightText that was cleared due to layer content.
			var copyright = sb.ToString();
			AutoGeneratedCopyrightText = string.IsNullOrEmpty(copyright) ? null : copyright;
		}

		private bool IsSecuredLayer(Layer layer)
		{
			bool hasProxy = false;
			if (layer is ArcGISTiledMapServiceLayer)
			{
				hasProxy = !string.IsNullOrEmpty((layer as ArcGISTiledMapServiceLayer).ProxyURL);
			}
			else if (layer is ArcGISImageServiceLayer)
			{
				hasProxy = !string.IsNullOrEmpty((layer as ArcGISImageServiceLayer).ProxyURL);
			}
			else if (layer is ArcGISDynamicMapServiceLayer)
			{
				hasProxy = !string.IsNullOrEmpty((layer as ArcGISDynamicMapServiceLayer).ProxyURL);
			}
			else if (layer is FeatureLayer)
			{
				hasProxy = !string.IsNullOrEmpty((layer as FeatureLayer).ProxyUrl);
			}
			return hasProxy;
		}

		/// <summary>
		/// Populates the layersNotSupported and warning messages.
		/// </summary>
		private void CheckLayerSupport()
		{
			if (Map != null && Map.Layers != null)
			{
				nonSerializableLayers.Clear();
				var sb = new StringBuilder();
				foreach (var l in Map.Layers)
				{
					// Layers that are not visible are not serialized and therefore ignored in the check.
					if(!l.Visible) continue;

					// Used for displaying warning messages.
					var title = MapApplication.GetLayerName(l);	
					
					var layerType = l.GetType().Name;

					// Used for legend in the print layout.
					l.DisplayName = title;

					// Validates by known list of supported types.
					if (!SerializableLayers.Contains(l.GetType()))
					{
						if (nonSerializableLayers.Count > 0)
                            sb.Append("\n\n");

						sb.AppendFormat(Resources.Strings.NotSupportedLayerType, title, layerType);
						nonSerializableLayers.Add(l);
					}
                    else if (IsSecuredLayer(l))
                    {
                        if (nonSerializableLayers.Count > 0)
                            sb.Append("\n\n");

                        sb.AppendFormat(Resources.Strings.NotSupportedLayerWithProxy, title);
                        nonSerializableLayers.Add(l);
                    }

					if (l is GraphicsLayer)
					{
						// Client-side clustering is not supported by print service.
						if ((l as GraphicsLayer).Clusterer != null)
						{
                            if (nonSerializableLayers.Count > 0)
                                sb.Append("\n\n");

                            sb.AppendFormat(Resources.Strings.NotSupportedClusterer, title, layerType);
						}

						// Validates renderer support by checking if we are able to serialize it.
						var renderer = (l as GraphicsLayer).Renderer;
						bool downgradeRenderer = (l as GraphicsLayer).RendererTakesPrecedence && !(renderer is IJsonSerializable);
						if (downgradeRenderer && (l as GraphicsLayer).Renderer != null)
						{
                            if (nonSerializableLayers.Count > 0)
                                sb.Append("\n\n");

                                sb.AppendFormat(Resources.Strings.NotSupportedLayerRenderer,
									(l as GraphicsLayer).Renderer.GetType().Name, title, layerType);
						}
						else
						{
							// Validates symbol support by checking if we are able to serialize it.
							bool downgradeSymbols = !(l as GraphicsLayer).RendererTakesPrecedence && (l as GraphicsLayer).Graphics != null &&
								(l as GraphicsLayer).Graphics.Any(g => g.Symbol == null || !(g.Symbol is IJsonSerializable));
							if (downgradeSymbols) // For symbol override
							{
                                if (nonSerializableLayers.Count > 0)
                                    sb.Append("\n\n");

                                sb.AppendFormat(Resources.Strings.NotSupportedGraphicSymbol,
									((l as GraphicsLayer).Graphics.FirstOrDefault(g => g.Symbol == null || !(g.Symbol is IJsonSerializable))).GetType().Name, title, layerType);
							}
							else if((l as GraphicsLayer).Renderer != null)// For symbols in the renderer
							{
								if (renderer is IJsonSerializable)
								{
									if (renderer is SimpleRenderer)
									{
										var symbol = renderer.GetSymbol(null);
										if (symbol != null && !(symbol is IJsonSerializable))
										{
                                            if (nonSerializableLayers.Count > 0)
                                                sb.Append("\n\n");

                                            sb.AppendFormat(Resources.Strings.NotSupportedGraphicSymbol,
												symbol.GetType().Name, title, layerType);
										}
									}
									else if (renderer is UniqueValueRenderer)
									{
										var uvr = renderer as UniqueValueRenderer;
										foreach (var info in uvr.Infos)
										{
											if (info.Symbol != null && !(info.Symbol is IJsonSerializable))
											{
                                                if (nonSerializableLayers.Count > 0)
                                                    sb.Append("\n\n");

                                                sb.AppendFormat(Resources.Strings.NotSupportedGraphicSymbol,
													info.Symbol.GetType().Name, title, layerType);
											}
										}
									}
									else if (renderer is ClassBreaksRenderer)
									{
										var cbr = renderer as ClassBreaksRenderer;
										foreach (var info in cbr.Classes)
										{
											if (info.Symbol != null && !(info.Symbol is IJsonSerializable))
											{
                                                if (nonSerializableLayers.Count > 0)
                                                    sb.Append("\n\n");

                                                sb.AppendFormat(Resources.Strings.NotSupportedGraphicSymbol,
													info.Symbol.GetType().Name, title, layerType);
											}
										}
									}
									else if (renderer is TemporalRenderer)
									{
										var tr = renderer as TemporalRenderer;
										if (!(tr.ObservationRenderer is IJsonSerializable))
										{
                                            if (nonSerializableLayers.Count > 0)
                                                sb.Append("\n\n");

                                            sb.AppendFormat(Resources.Strings.NotSupportedLayerRenderer,
												tr.ObservationRenderer.GetType().Name, title, layerType);
										}
										if (!(tr.LatestObservationRenderer is IJsonSerializable))
										{
                                            if (nonSerializableLayers.Count > 0)
                                                sb.Append("\n\n");

                                            sb.AppendFormat(Resources.Strings.NotSupportedLayerRenderer,
												tr.LatestObservationRenderer.GetType().Name, title, layerType);
										}
										if (!(tr.TrackRenderer is IJsonSerializable))
										{
                                            if (nonSerializableLayers.Count > 0)
                                                sb.Append("\n\n");

                                            sb.AppendFormat(Resources.Strings.NotSupportedLayerRenderer,
												tr.TrackRenderer.GetType().Name, title, layerType);
										}
										if (!(tr.SymbolAger is IJsonSerializable))
										{
                                            if (nonSerializableLayers.Count > 0)
                                                sb.Append("\n\n");

                                            sb.AppendFormat(Resources.Strings.NotSupportedLayerRenderer,
												tr.SymbolAger.GetType().Name, title, layerType);
										}
									}
								}
							}
						}
					}
				}
				Status = sb.ToString();
			}
		}

		/// <summary>
		/// Updates the layer's visibility for layers that are not supported.
		/// </summary>
		private void UpdateLayersNotSupported(bool isVisible)
		{
			if (nonSerializableLayers != null && nonSerializableLayers.Count > 0)
			{
				foreach (var l in nonSerializableLayers)
				{
					ignoreVisibilityChange = true;
					l.Visible = isVisible;
					ignoreVisibilityChange = false;
					if (isVisible)
					{
						if (l is DynamicLayer) //i.e. HeatMapLayer
						{
							(l as DynamicLayer).Refresh();
						}
						else if (l is GraphicsLayer) //i.e. GeoRssLayer
							(l as GraphicsLayer).Refresh();
					}
				}
				if (isVisible) nonSerializableLayers.Clear();
			}
		}

        /// <summary>
        /// Toggles properties on basemap layers to include them in or exclude them from the map legend
        /// </summary>
        /// <param name="include">Whether to include the basemap layers in the legend or not</param>
        private void ToggleBasemapLegend(bool include)
        {
            var basemapLayers = Map.Layers.Where(l => Document.GetIsBaseMap(l) && l.ShowLegend);
            foreach (var layer in basemapLayers)
            {
                if (!include)
                    layer.ShowLegend = false;
                else 
                    layer.ShowLegend = true;
            }
        }

        /// <summary>
        /// Populates graphics layers that don't have legend labels with default values
        /// </summary>
        private void InitializeLegendLabels()
        {
            var graphicsLayers = Map.Layers.Where(l => l is GraphicsLayer);

            foreach (GraphicsLayer gLayer in graphicsLayers)
            {
                if (gLayer.Renderer is UniqueValueRenderer)
                {
                    var uvRenderer = (UniqueValueRenderer)gLayer.Renderer;
                    var infos = uvRenderer.Infos.Where(i => string.IsNullOrEmpty(i.Label));
                    foreach (var info in infos)
                        info.Label = info.Value.ToString();
                }
                else if (gLayer.Renderer is ClassBreaksRenderer)
                {
                    var cbRenderer = (ClassBreaksRenderer)gLayer.Renderer;
                    var classes = cbRenderer.Classes.Where(cb => string.IsNullOrEmpty(cb.Label));
                    foreach (var classBreak in classes)
                        classBreak.Label = string.Format("{0} - {1}", classBreak.MinimumValue, classBreak.MaximumValue);
                }
            }
        }

		/// <summary>
		/// This event is used to build warning messages non-serializable layers when printing with ArcGIS server
		/// and for generating copyright text.
		/// </summary>
		private void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			GenerateCopyright();
			if (PrintWithArcGISServer)
			{
				if (e.NewItems != null)
				{
					foreach (var item in e.NewItems)
					{
						var l = item as Layer;
						if (!l.IsInitialized) { l.Initialized += Layer_Initialized; }
						else { CheckLayerSupport(); }
					}
				}
				else
				{
					CheckLayerSupport();
				}
			}
		}
		
		/// <summary>		
		/// This event is used to build warning messages for non-serialiable layers when printing with ArcGIS server.
		/// </summary>
		private void Layer_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PrintWithArcGISServer && !ignoreVisibilityChange && e.PropertyName == "Visible")
				CheckLayerSupport();
		}

		/// <summary>		
		/// This event is used to build warning messages for non-serialiable layers when printing with ArcGIS server.
		/// </summary>
		private void Layer_Initialized(object sender, EventArgs e)
		{
			(sender as Layer).Initialized -= Layer_Initialized;
			CheckLayerSupport();		
		}

		/// <summary>		
		/// This event is used to update MapScale if visible and no default scale has been set when printing with ArcGIS server.
		/// </summary>	
		private void Map_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (UseScale && !IsUseScaleVisible) return;
			if (e.PropertyName == "Scale")
				MapScaleString = Convert.ToString(Math.Round((sender as Map).Scale, 0), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// This event is used to detect when map is ready for printing.
		/// </summary>
		private void Map_Progress(object sender, ProgressEventArgs e)
		{
			IsMapReady = e.Progress == 100;
		}

		/// <summary>
		/// This event is used to get print service information to populate layout and format choice lists.
		/// </summary>
		private void PrintTask_GetServiceInfoCompleted(object sender, ServiceInfoEventArgs e)
		{
			(sender as PrintTask).GetServiceInfoCompleted -= PrintTask_GetServiceInfoCompleted;
			IsBusy = printTask.IsBusy;
            
            // Set flag indicating that currently specified service has been retrieved
            serviceLoaded = true;

            // Raise execution state change for load command
            LoadChanged();

            var showStatus = (bool) e.UserState;
			if (e.Error != null || e.ServiceInfo == null)
			{
				Error = e.Error;
				return;
			}
            // Use service-defined choice lists.
            if (e.ServiceInfo.LayoutTemplates == null || e.ServiceInfo.LayoutTemplates.Count() == 0)
            {
                Error = new Exception("No layouts found for the specified URL");
                return;
            }
            else
            {
                LayoutTemplates = new List<string>(e.ServiceInfo.LayoutTemplates);
            }
            if (showStatus) Status = Resources.Strings.GetLayoutTemplatesCompleted;
			IsServiceAsynchronous = e.ServiceInfo.IsServiceAsynchronous;

			if (e.ServiceInfo.Formats != null)
			{
				Formats = new List<string>(e.ServiceInfo.Formats);
			}

			// Use service-defined default values.
			if (e.ServiceInfo.Parameters != null)
			{
				var layoutTemplateParameter = e.ServiceInfo.Parameters.FirstOrDefault(p => p.Name == "Layout_Template");
				LayoutTemplate = (layoutTemplateParameter != null) ? layoutTemplateParameter.DefaultValue as string : LayoutTemplates.FirstOrDefault();
				var formatParameter = e.ServiceInfo.Parameters.FirstOrDefault(p => p.Name == "Format");
				Format = (formatParameter != null) ? formatParameter.DefaultValue as string : Formats.FirstOrDefault();
			}
			IsValid = e.ServiceInfo != null;
		}

		/// <summary>
		/// This event is used to open exported printable output file from a synchronous-server when printing with ArcGIS server.
		/// </summary>
		private void PrintTask_ExecuteCompleted(object sender, PrintEventArgs e)
		{
			IsBusy = IsPrinting = printTask.IsBusy;
			if (e.Error != null || e.PrintResult == null)
			{
				Error = e.Error;
				return;
			}
			Status = Resources.Strings.PrintCompleted;
			var uri = e.PrintResult.Url;
			if (UseProxy && !string.IsNullOrEmpty(printTask.ProxyURL))
				uri = new Uri(string.Concat(printTask.ProxyURL, "?", e.PrintResult.Url.OriginalString));
			System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank");
		}

		/// <summary>		
		/// This event is used to display status messages from an asynchronous-server when printing with ArcGIS server.
		/// </summary>
		private void PrintTask_StatusUpdated(object sender, JobInfoEventArgs e)
		{
			IsBusy = printTask.IsBusy;
			if (e.JobInfo != null)
			{
				var sb = new StringBuilder();
				sb.AppendFormat("{0}", e.JobInfo.JobStatus);
				if (e.JobInfo.Messages != null)
				{
                    foreach (var m in e.JobInfo.Messages)
						sb.AppendFormat("\n\n{0}: {1}", m.MessageType, m.Description);
				}
				Status = sb.ToString();
			}
		}

		/// <summary>		
		/// This event is used to open exported printable output file from an asynchronous-server when printing with ArcGIS server.
		/// </summary>		
		private void PrintTask_JobCompleted(object sender, PrintJobEventArgs e)
		{
			IsBusy = IsPrinting = printTask.IsBusy;
			if (e.Error != null || e.PrintResult == null)
			{
				Error = e.Error;
				return;
			}
			Status = Resources.Strings.PrintCompleted;
			var uri = e.PrintResult.Url;
			if (UseProxy && !string.IsNullOrEmpty(printTask.ProxyURL))
				uri = new Uri(string.Concat(printTask.ProxyURL, "?", e.PrintResult.Url.OriginalString));
			System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank");
		}

		/// <summary>
		/// This event is used to display print preview using selected print layout.
		/// </summary>
		private void LayoutXamlWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			(sender as WebClient).DownloadStringCompleted -= LayoutXamlWebClient_DownloadStringCompleted;
			IsBusy = (sender as WebClient).IsBusy;
			if (e.Error != null || string.IsNullOrEmpty(e.Result))
			{
				Error = e.Error;
				return;
			}			
			try
			{
				printPreview.Content = XamlReader.Load(e.Result);
			}
			catch(Exception ex)
			{	
				Error = ex;
				return;
			}			
			Status = Resources.Strings.GetPrintLayoutXamlCompleted;
			MapApplication.Current.ShowWindow(Resources.Strings.PrintLabel, printPreview, false,
				// when canceled reset properties.
				(a, b) =>
				{
					IsBusy = IsPrinting = false;
					Status = Resources.Strings.PrintCanceled;
				}, null, WindowType.Floating);
		}

		/// <summary>
		/// This event is used to display print layout choices as defined in the print configuration file.
		/// </summary>
		private void PrintLayoutsWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			(sender as WebClient).DownloadStringCompleted -= PrintLayoutsWebClient_DownloadStringCompleted;
			IsBusy = (sender as WebClient).IsBusy;
			var showPreview = (bool)e.UserState; 
			if (e.Error != null || string.IsNullOrEmpty(e.Result))
			{
				Error = e.Error;
				return;
			}
			if (showPreview) Status = Resources.Strings.GetLayoutTemplatesCompleted;
			using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(e.Result)))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrintLayouts));
				memoryStream.Position = 0;
				PrintLayouts = (PrintLayouts)xmlSerializer.Deserialize(memoryStream);
				memoryStream.Close();
			}
			if (PrintLayouts != null)
			{
				if (showPreview)
				{
					PrintLayouts.IsConfig = isConfig;
					var printLayoutPicker = new View.PrintLayoutPicker() { DataContext = PrintLayouts };
					PrintLayouts.PropertyChanged += (a, b) =>
						{
							if (b.PropertyName == "IsApplied")
							{
								PrintLayout = new PrintLayout(PrintLayouts.SelectedLayout);
								MapApplication.Current.HideWindow(printLayoutPicker);
							}
						};
					MapApplication.Current.ShowWindow(Resources.Strings.PrintLayoutPickerTitle, printLayoutPicker, true, null, null,
						isConfig ? WindowType.DesignTimeFloating : WindowType.Floating);
				}
				else if (PrintLayout == null)
					PrintLayout = PrintLayouts.FirstOrDefault();
			}
			IsValid = PrintLayout != null && PrintHeight > 0 && PrintWidth > 0;
		}

		/// <summary>
		/// This event is used to set the print content.
		/// </summary>
		private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
		{
			if (printContent != null)
				e.PageVisual = printContent;
		}

		/// <summary>
		/// This event is used to reset busy and printing status and also display message of completion.
		/// </summary>
		private void PrintDocument_EndPrint(object sender, EndPrintEventArgs e)
		{
			IsBusy = IsPrinting = false;			
			Status = Resources.Strings.PrintCompleted;
		}

		/// <summary>
		/// This event is used to set busy and printing status and also display message that print has started.
		/// </summary>
		private void PrintDocument_BeginPrint(object sender, BeginPrintEventArgs e)
		{
			IsBusy = IsPrinting = true;
			Status = Resources.Strings.PrintStarted;						
		}

        // Flag indicating whether deserialization is in progress
        private bool m_deserializing = false;

        /// <summary>
        /// Sets a flag during deserialization to allow internal logic to detect that deserialization is in progress
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OnDeserializing]
        public void onDeserializing(StreamingContext context)
        {
            m_deserializing = true;
        }

        /// <summary>
        /// Resets flag that indicates when deserialization is in progress
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OnDeserialized]
        public void onDeserialized(StreamingContext context)
        {
            m_deserializing = false;
        }

		#endregion

		#region INotifyPropertyChanged

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		#endregion
	}
}
