/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Application.Controls;
using ESRI.ArcGIS.Client.Extensibility;
using System.ComponentModel;
using ESRI.ArcGIS.Mapping.GP.Resources;

namespace ESRI.ArcGIS.Mapping.GP
{
    [TemplatePart(Name = PART_ParamContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ParamsScroller, Type = typeof(ScrollViewer))]
    public class GPWidget : Control
    {
        private const string PART_ParamContainer = "ParamContainer";
        private const string PART_ParamsScroller = "ParamsScroller";
        ScrollViewer paramsScroller;

        ESRI.ArcGIS.Client.Tasks.Geoprocessor gp;
        List<ParameterSupport.ParameterBase> InputParameters = new List<ParameterSupport.ParameterBase>();
        Grid ParamContainer;
        bool isLoading = false;

        public GPWidget()
        {
            this.DefaultStyleKey = typeof(GPWidget);
            Execute = new DelegateCommand(execute, canExecute);
            Cancel = new DelegateCommand(cancel);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ParamContainer = GetTemplateChild(PART_ParamContainer) as Grid;
            paramsScroller = GetTemplateChild(PART_ParamsScroller) as ScrollViewer;
            BuildUI();
        }

        private void BuildUI()
        {
            if (ParamContainer == null) return;
            if (ServiceInfo != null && Configuration == null)
            {
                Configuration = new GPConfiguration();
                Configuration.LoadConfiguration(ServiceInfo, ServiceEndpoint);
                return;
            }
            else if (Configuration != null && ServiceEndpoint == null)
            {
                ServiceEndpoint = Configuration.TaskEndPoint;
            }
            paramsScroller.IsEnabled = true;
            if (isLoading)
            {
                ParamContainer.Children.Clear();
                ParamContainer.Children.Add(new TextBlock() { Text = ESRI.ArcGIS.Mapping.GP.Resources.Strings.Loading, FontWeight = System.Windows.FontWeights.Bold });
            }
            else if (Configuration != null)
            {
                double size = ViewUtility.GetViewHeight() - 325;
                paramsScroller.MaxHeight = size < 100 ? 100 : size;
                size = ViewUtility.GetViewWidth() - 200;
                paramsScroller.MaxWidth = size < 100 ? 100 : size;
                if (paramsScroller.MaxWidth > 500)
                    paramsScroller.MaxWidth = 500;
                ParamContainer.Children.Clear();
                ParamContainer.ColumnDefinitions.Clear();
                ParamContainer.RowDefinitions.Clear();
                ParamContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                ParamContainer.ColumnDefinitions.Add(new ColumnDefinition());// { Width = new GridLength(0, GridUnitType.Star) });
                InputParameters.Clear();

                foreach (ParameterSupport.ParameterConfig config in Configuration.InputParameters)
                {
                    ParameterSupport.ParameterBase parameter = ParameterSupport.ParameterFactory.Create(config, Map);
                    if (parameter != null)
                    {
                        parameter.CanExecuteChanged += parameter_CanExecuteChanged;
                        InputParameters.Add(parameter);
                        if (config.ShownAtRunTime)
                        {
                            ParamContainer.RowDefinitions.Add(new RowDefinition());
                            TextBlock label = new TextBlock()
                                                  {
                                                      Text = config.Label,
                                                      VerticalAlignment = System.Windows.VerticalAlignment.Center,
                                                      Padding = new Thickness(2, 0, 2, 0)
                                                  };
                            label.SetValue(Grid.RowProperty, ParamContainer.RowDefinitions.Count - 1);
                            //if (config.Required)
                            //    label.Text += "*";
                            ParamContainer.Children.Add(label);
                            parameter.AddUI(ParamContainer);
                        }
                    }
                }
            }    
        }

        void parameter_CanExecuteChanged(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        #region ExecuteCompleted event
        public sealed class ExecuteCompleteEventArgs : EventArgs
        {
            public List<GPParameter> Results { get; internal set; }
        }
        public sealed class ExecuteFailedEventArgs : EventArgs
        {
            public List<Exception> Errors { get; internal set; }
        }
        public event EventHandler<ExecuteCompleteEventArgs> ExecuteCompleted;
        public event EventHandler<ExecuteFailedEventArgs> ExecuteFailed;

       
        void fireExecuteCompleted(ExecuteCompleteEventArgs args)
        {
            if (!IsExecuting)
                return;
            IsExecuting = false;
            RaiseCanExecuteChanged();
            HasResults = true;
            setResults(args.Results);
            if (ExecuteCompleted != null)
                ExecuteCompleted(this, args);
        }
        #endregion

        #region GP task event handlers
        void gp_Failed(object sender, Client.Tasks.TaskFailedEventArgs e)
        {
             IsExecuting = false;
            RaiseCanExecuteChanged();
            HasResults = true;
            setResults(null);
            LatestErrors = new List<Exception> { e.Error };
            if (ExecuteFailed != null)
                ExecuteFailed(this, new ExecuteFailedEventArgs(){ Errors = LatestErrors});
        }
        List<string> resultRequestedParams;
        void gp_JobCompleted(object sender, Client.Tasks.JobInfoEventArgs e)
        {
            if (e.JobInfo.JobStatus == esriJobStatus.esriJobFailed)
            {
                IsExecuting = false;
                ExecutingText = ESRI.ArcGIS.Mapping.GP.Resources.Strings.JobFailed;
                asyncErrors.AddRange(e.JobInfo.Messages.Where(p => p.MessageType == GPMessageType.Error).Select(p => new Exception(p.Description)));
                fireAsyncCompleteEvent();
            }
            else
            {
                ExecutingText = ESRI.ArcGIS.Mapping.GP.Resources.Strings.GettingResults;
                asyncResultsExpected = Configuration.OutputParameters.Count;
                JobID = e.JobInfo.JobId;
                foreach (ParameterSupport.ParameterConfig param in Configuration.OutputParameters)
                {
                    if (!resultRequestedParams.Contains(param.Name))
                    {
                        if (param.Type == GPParameterType.MapServiceLayer)
                        {
                            asyncResultsExpected--;
                            asyncResults.Add(null);
                            if (asyncResultsExpected == 0)
                                fireAsyncCompleteEvent();
                        }
                        else
                        {
                            Geoprocessor gp = new Geoprocessor(ServiceEndpoint.AbsoluteUri);
                            gp.Failed += gp_FailedAsync;
                            gp.GetResultDataCompleted += gp2_GetResultDataCompleted;
                            gp.GetResultDataAsync(e.JobInfo.JobId, param.Name);
                        }
                    }
                }
            }
        }

        void gp2_GetResultDataCompleted(object sender, Client.Tasks.GPParameterEventArgs e)
        {
            asyncResultsExpected--;
            asyncResults.Add(e.Parameter);
            if (asyncResultsExpected == 0)
                fireAsyncCompleteEvent();
        }
        
        List<Exception> asyncErrors;
        List<GPParameter> asyncResults;
        int asyncResultsExpected;

        void gp_FailedAsync(object sender, Client.Tasks.TaskFailedEventArgs e)
        {
            asyncErrors.Add( e.Error );
            asyncResultsExpected--;
            if (asyncResultsExpected == 0)
                fireAsyncCompleteEvent();
        }

        void fireAsyncCompleteEvent()
        {
            HasResults = true;
            setResults(asyncResults);
            LatestErrors = asyncErrors;
           if (asyncResults.Count < 1 && asyncErrors.Count > 0)
            {
                if (ExecuteFailed != null)
                    ExecuteFailed(this, new ExecuteFailedEventArgs() { Errors = asyncErrors });
            }
            else
                fireExecuteCompleted(new ExecuteCompleteEventArgs() { Results = asyncResults });
            RaiseCanExecuteChanged();
        }

        void gp_StatusUpdated(object sender, Client.Tasks.JobInfoEventArgs e)
        {
            var statusKey = e.JobInfo.JobStatus.ToString().Replace("esriJob", "");
            ExecutingText = StringResourcesManager.Instance.Get(statusKey);
        }

        void gp_ExecuteCompleted(object sender, Client.Tasks.GPExecuteCompleteEventArgs e)
        {
            ExecuteCompleteEventArgs args = new ExecuteCompleteEventArgs() { Results = e.Results.OutParameters };
            fireExecuteCompleted(args);
        }
        #endregion

        #region GP Configuration
        public GPConfiguration Configuration
        {
            get { return (GPConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Configuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(GPConfiguration), typeof(GPWidget), new PropertyMetadata(OnConfigurationPropertyChanged));

        private static void OnConfigurationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GPWidget obj = (GPWidget)d;
            obj.BuildUI();
        }
        #endregion

        #region MetadataLoader
        MetaData.MetaDataLoader metaDataLoader;
        MetaData.MetaDataLoader MetaDataLoader
        {
            get
            {
                if (metaDataLoader == null)
                {
                    metaDataLoader = new MetaData.MetaDataLoader();
                    metaDataLoader.LoadFailed += new EventHandler(metaDataLoader_LoadFailed);
                    metaDataLoader.LoadSucceeded += new EventHandler(metaDataLoader_LoadSucceeded);
                }
                return metaDataLoader;
            }
        }

        void metaDataLoader_LoadSucceeded(object sender, EventArgs e)
        {
            isLoading = false;
            ServiceInfo = MetaDataLoader.ServiceInfo;
            BuildUI();
        }

        void metaDataLoader_LoadFailed(object sender, EventArgs e)
        {
            isLoading = false;
            ServiceInfo = null;

            // Create error text
            TextBlock tb = new TextBlock() 
            { 
                Text = ESRI.ArcGIS.Mapping.GP.Resources.Strings.FailedToLoad, 
                FontWeight = System.Windows.FontWeights.Bold 
            };
            tb.Foreground = new SolidColorBrush(Colors.Red);
            ToolTipService.SetToolTip(tb, new ToolTip() { Content = MetaDataLoader.Error.Message });

            // Replace UI with error text
            ParamContainer.Children.Clear();
            ParamContainer.Children.Add(tb);

            // Update execution state - execution should not be possible when service metadata retrieval fails
            RaiseCanExecuteChanged();

            return;
        }
        #endregion

        #region ServiceEndPoint
        public Uri ServiceEndpoint
        {
            get { return (Uri)GetValue(ServiceEndpointProperty); }
            set { SetValue(ServiceEndpointProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ServiceEndpoint"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ServiceEndpointProperty =
            DependencyProperty.Register("ServiceEndpoint", typeof(Uri), typeof(GPWidget), new PropertyMetadata(OnServiceEndpointPropertyChanged));

        private static void OnServiceEndpointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GPWidget obj = (GPWidget)d;
            Uri newValue = (Uri)e.NewValue;
            Uri oldValue = (Uri)e.OldValue;

            if (newValue == null)
                obj.ServiceInfo = null;
            else if (newValue != null && oldValue != newValue)
            {
                obj.MetaDataLoader.ServiceEndpoint = newValue;
                obj.isLoading = true;

                // Get proxy URL if proxy is being used
                string proxyUrl = null;
                if (obj.Configuration != null && obj.Configuration.UseProxy
                && MapApplication.Current != null && MapApplication.Current.Urls != null)
                    proxyUrl = MapApplication.Current.Urls.ProxyUrl;

                obj.MetaDataLoader.LoadMetadata(DesignerProperties.GetIsInDesignMode(obj), proxyUrl);
            }
            (obj.Execute as DelegateCommand).RaiseCanExecuteChanged();
        }
        #endregion

        #region ServiceInfo
        public MetaData.GPMetaData ServiceInfo
        {
            get { return (MetaData.GPMetaData)GetValue(ServiceInfoProperty); }
            internal set { SetValue(ServiceInfoProperty, value); }
        }

        //internal because the setter is internal
        /// <summary>
        /// Identifies the <see cref="ServiceInfo"/> dependency property.
        /// </summary>
        internal static readonly DependencyProperty ServiceInfoProperty =
            DependencyProperty.Register("ServiceInfo", typeof(MetaData.GPMetaData), typeof(GPWidget), new PropertyMetadata(OnServiceInfoPropertyChanged));

        private static void OnServiceInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GPWidget obj = (GPWidget)d;
            (obj.Execute as DelegateCommand).RaiseCanExecuteChanged();
        }
        #endregion

        #region Execute Command
        private void execute(object commandParameter)
        {
            paramsScroller.IsEnabled = false;
            IsExecuting = true;
            HasResults = false;
            setResults(null);
            RaiseCanExecuteChanged();
            gp = new Client.Tasks.Geoprocessor(ServiceEndpoint.AbsoluteUri);
            gp.Failed += gp_Failed;
            List<ESRI.ArcGIS.Client.Tasks.GPParameter> inputValues = new List<ESRI.ArcGIS.Client.Tasks.GPParameter>();
            InputLayers = new Dictionary<string,string>();
            foreach (ParameterSupport.ParameterBase param in InputParameters)
            {
                if (param.Value != null)
                    inputValues.Add(param.Value);
                if (param is ParameterSupport.FeatureLayerParameterBase)
                {
                    ParameterSupport.FeatureLayerParameterBase flParam = param as ParameterSupport.FeatureLayerParameterBase;
                    if (!string.IsNullOrEmpty(flParam.InputLayerID))
                        InputLayers.Add(flParam.Config.Name, flParam.InputLayerID);
                }
            }
            gp.OutputSpatialReference = Map.SpatialReference;

            // Initialize proxy
            gp.ProxyURL = Configuration.UseProxy ? Configuration.ProxyUrl : null;

            if (ServiceInfo.ExecutionType == "esriExecutionTypeAsynchronous")
            {
                gp.StatusUpdated += gp_StatusUpdated;
                gp.JobCompleted += gp_JobCompleted;
                asyncResults = new List<GPParameter>();
                resultRequestedParams = new List<string>();
                asyncErrors = new List<Exception>();
                gp.SubmitJobAsync(inputValues);
            }
            else
            {
                gp.ExecuteCompleted += gp_ExecuteCompleted;
                gp.ExecuteAsync(inputValues);
            }
        }
        
        private void RaiseCanExecuteChanged()
        {
            (Execute as DelegateCommand).RaiseCanExecuteChanged();
        }

        private bool canExecute(object commandParameter)
        {
            if (Configuration == null
            || IsExecuting
            || MetaDataLoader.Error != null)
                return false;

            if (Configuration.InputParameters != null && Configuration.InputParameters.Count == 0)
                return true;
            foreach (ParameterSupport.ParameterBase param in InputParameters)
            {
                if (!param.CanExecute)
                    return false;
            }
            return true;
        }

        public ICommand Execute
        {
            get { return (ICommand)GetValue(ExecuteProperty); }
            private set { SetValue(ExecuteProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Execute"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExecuteProperty =
            DependencyProperty.Register("Execute", typeof(ICommand), typeof(GPWidget), null);
        #endregion

        #region Cancel Command
        private void cancel(object commandParameter)
        {
            IsExecuting = false;
            if (gp != null)
            {
                if (ServiceInfo.ExecutionType == "esriExecutionTypeAsynchronous")
                {
                    gp.StatusUpdated -= gp_StatusUpdated;
                    gp.JobCompleted -= gp_JobCompleted;
                    gp.Failed -= gp_Failed;
                }
                else
                {
                    gp.ExecuteCompleted -= gp_ExecuteCompleted;
                }
            }
            RaiseCanExecuteChanged();
        }
        public ICommand Cancel
        {
            get { return (ICommand)GetValue(CancelProperty); }
            private set { SetValue(CancelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Cancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelProperty =
            DependencyProperty.Register("Cancel", typeof(ICommand), typeof(GPWidget), null);
        #endregion

        #region Map
        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Map"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(Map), typeof(GPWidget), new PropertyMetadata(OnMapPropertyChanged));

        private static void OnMapPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GPWidget obj = (GPWidget)d;
            obj.RaiseCanExecuteChanged();
        }
        #endregion

        #region IsExecuting
        public bool IsExecuting
        {
            get { return (bool)GetValue(IsExecutingProperty); }
            set { SetValue(IsExecutingProperty, value); }
        }

        public static readonly DependencyProperty IsExecutingProperty =
            DependencyProperty.Register("IsExecuting", typeof(bool), typeof(GPWidget), new PropertyMetadata(OnIsProcessingPropertyChanged));

        private static void OnIsProcessingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GPWidget obj = (GPWidget)d;
        }
        #endregion

        #region ExecuteText
        public string ExecuteText
        {
            get { return (string)GetValue(ExecuteTextProperty); }
            set { SetValue(ExecuteTextProperty, value); }
        }

        public static readonly DependencyProperty ExecuteTextProperty =
            DependencyProperty.Register("ExecuteText", typeof(string), typeof(GPWidget), new PropertyMetadata(Strings.Execute));
        #endregion

        #region ExecutingText
        public string ExecutingText
        {
            get { return (string)GetValue(ExecutingTextProperty); }
            set { SetValue(ExecutingTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExecutingText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExecutingTextProperty =
            DependencyProperty.Register("ExecutingText", typeof(string), typeof(GPWidget), 
            new PropertyMetadata(Strings.Executing));
        #endregion

        #region HasResults
        public bool HasResults
        {
            get { return (bool)GetValue(HasResultsProperty); }
            set { SetValue(HasResultsProperty, value); }
        }

        public static readonly DependencyProperty HasResultsProperty =
            DependencyProperty.Register("HasResults", typeof(bool), typeof(GPWidget), null);
        #endregion

        #region LatestResults
        void setResults(List<GPParameter> results )
        {
            //if (results != null && InputLayers != null && InputLayers.Count > 1)
            //    LayerOrderer.OrderLayers(Map, Configuration.LayerOrder, InputLayers);
            LatestResults = results;
        }

        public List<GPParameter> LatestResults
        {
            get { return (List<GPParameter>)GetValue(LatestResultsProperty); }
            set { SetValue(LatestResultsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatestResults.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatestResultsProperty =
            DependencyProperty.Register("LatestResults", typeof(List<GPParameter>), typeof(GPWidget), null);
        #endregion

        #region LatestErrors
        public List<Exception> LatestErrors
        {
            get { return (List<Exception>)GetValue(LatestErrorsProperty); }
            set { SetValue(LatestErrorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LatestErrors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatestErrorsProperty =
            DependencyProperty.Register("LatestErrors", typeof(List<Exception>), typeof(GPWidget), null);
        #endregion



        public string JobID
        {
            get { return (string)GetValue(JobIDProperty); }
            set { SetValue(JobIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for JobID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JobIDProperty =
            DependencyProperty.Register("JobID", typeof(string), typeof(GPWidget), null);

        

        #region Add layers according to order
        public Dictionary<string, string> InputLayers
        {
            get { return (Dictionary<string, string>)GetValue(InputLayersProperty); }
            set { SetValue(InputLayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputLayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputLayersProperty =
            DependencyProperty.Register("InputLayers", typeof(Dictionary<string, string>), typeof(GPWidget), null);

        #endregion
    }
}
