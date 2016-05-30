/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class EnableDisableMapBehaviorCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ExtensionBehavior behavior = parameter as ExtensionBehavior;
            if (behavior == null)
                return;

            addRemoveMapBehavior(behavior);
        }

        private void addRemoveMapBehavior(ExtensionBehavior extensionBehavior)
        {
            if (MapApplication.Current == null && MapApplication.Current.Map != null)
                return;

            BehaviorCollection behaviors = Interaction.GetBehaviors(MapApplication.Current.Map);
            if (behaviors == null)
                return;

            Behavior<Map> mapBehavior = extensionBehavior.MapBehavior as Behavior<Map>;
            if (mapBehavior == null)
            {
                mapBehavior = BehaviorUtils.GetMapBehavior(extensionBehavior);
                extensionBehavior.MapBehavior = mapBehavior;
            }

            if (mapBehavior == null)
                return;

            if (extensionBehavior.IsEnabled)
            {
                if (!behaviors.Contains(mapBehavior))
                    behaviors.Add(mapBehavior);
            }
            else
                behaviors.Remove(mapBehavior);
        }
    }

    public class DeleteMapBehaviorCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter as ExtensionBehavior != null;
        }

        public override void Execute(object parameter)
        {
            ExtensionBehavior behavior = parameter as ExtensionBehavior;
            if (behavior == null)
                return;

            MessageBoxDialog.Show(LocalizableStrings.GetString("DeleteBehaviorCaption"), LocalizableStrings.GetString("DeleteBehaviorPrompt"), MessageBoxButton.OKCancel,
                            new MessageBoxClosedEventHandler(delegate(object obj, MessageBoxClosedArgs args1)
                            {
                                if (args1.Result == MessageBoxResult.OK)
                                {
                                    // remove from map
                                    removeMapBehavior(behavior);
                                }
                            }));
        }

        private void removeMapBehavior(ExtensionBehavior extensionBehavior)
        {
            if (MapApplication.Current == null && MapApplication.Current.Map != null)
                return;

            BehaviorCollection behaviors = Interaction.GetBehaviors(MapApplication.Current.Map);
            if (behaviors == null)
                return;

            Behavior<Map> mapBehavior = extensionBehavior.MapBehavior as Behavior<Map>;
            if (mapBehavior == null)
            {
                mapBehavior = BehaviorUtils.GetMapBehavior(extensionBehavior);
                extensionBehavior.MapBehavior = mapBehavior;
            }

            if (mapBehavior == null)
                return;

            behaviors.Remove(mapBehavior);

            if (View.Instance != null && View.Instance.ExtensionBehaviors != null && View.Instance.ExtensionBehaviors.Contains(extensionBehavior))
            {
                View.Instance.ExtensionBehaviors.Remove(extensionBehavior);
            }
        }
    }

    public class AddMapBehaviorCommand : CommandBase
    {
        public AddMapBehaviorCommand()
        {
            Instances = Instances ?? new List<AddMapBehaviorCommand>();
            ((List<AddMapBehaviorCommand>)Instances).Add(this);
        }

        public static IEnumerable<AddMapBehaviorCommand> Instances { get; set; }

        // This function is called when new extensions are available within Builder
        // Or existing extensions are deleted
        public void OnExtensionsCatalogChanged()
        {
            if (mapBehaviorConfigControl != null)
                mapBehaviorConfigControl.Refresh();
        }

        private MapBehaviorConfigControl mapBehaviorConfigControl;

        public override void Execute(object parameter)
        {
            addExtensionMapBehavior();
        }

        private void addExtensionMapBehavior()
        {
            if (mapBehaviorConfigControl == null)
            {
                mapBehaviorConfigControl = new MapBehaviorConfigControl();
                mapBehaviorConfigControl.OkClicked += (o, e) =>
                {
                    addRemoveMapBehavior(mapBehaviorConfigControl.ExtensionBehavior);

                    if (View.Instance != null && View.Instance.ExtensionBehaviors != null &&
                            !View.Instance.ExtensionBehaviors.Contains(mapBehaviorConfigControl.ExtensionBehavior))
                        View.Instance.ExtensionBehaviors.Add(mapBehaviorConfigControl.ExtensionBehavior);

                    MapApplication.Current.HideWindow(mapBehaviorConfigControl);
                };
                mapBehaviorConfigControl.CancelClicked += (o, e) =>
                {
                    MapApplication.Current.HideWindow(mapBehaviorConfigControl);
                };
            }
            mapBehaviorConfigControl.ExtensionBehavior = null;
            mapBehaviorConfigControl.TypeSelectionVisibility = Visibility.Visible;

            WindowType windowType = MapApplication.Current.IsEditMode ? WindowType.DesignTimeFloating : 
                WindowType.Floating;
			MapApplication.Current.ShowWindow(ESRI.ArcGIS.Mapping.Controls.Resources.Strings.AddBehavior, 
                mapBehaviorConfigControl, false, null, null, windowType);
        }

        private void addRemoveMapBehavior(ExtensionBehavior extensionBehavior)
        {
            if (MapApplication.Current == null && MapApplication.Current.Map != null)
                return;

            BehaviorCollection behaviors = Interaction.GetBehaviors(MapApplication.Current.Map);
            if (behaviors == null)
                return;

            Behavior<Map> mapBehavior = extensionBehavior.MapBehavior as Behavior<Map>;
            if (mapBehavior == null)
            {
                mapBehavior = BehaviorUtils.GetMapBehavior(extensionBehavior);
                extensionBehavior.MapBehavior = mapBehavior;
            }

            if (mapBehavior == null)
                return;

            if (extensionBehavior.IsEnabled)
            {
                if (!behaviors.Contains(mapBehavior))
                    behaviors.Add(mapBehavior);
            }
            else
                behaviors.Remove(mapBehavior);
        }
    }

    public class ConfigureMapBehaviorCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            ExtensionBehavior extensionBehavior = parameter as ExtensionBehavior;
            if (extensionBehavior == null)
                return false;

            ISupportsConfiguration supportConfiguration = extensionBehavior.MapBehavior as ISupportsConfiguration;
            if (supportConfiguration != null)
                return true;

            return false;
        }

        public override void Execute(object parameter)
        {
            ExtensionBehavior extensionBehavior = parameter as ExtensionBehavior;
            if (extensionBehavior == null)
                return;

            ISupportsConfiguration supportConfiguration = extensionBehavior.MapBehavior as ISupportsConfiguration;
            if (supportConfiguration != null)
            {
                try
                {
                    supportConfiguration.Configure();
                }
                catch (Exception ex)
                {
                    if(ViewerApplicationControl.Instance == null)
                        MessageBoxDialog.Show(LocalizableStrings.GetString("BehaviorConfigurationFailedDescription"), LocalizableStrings.GetString("BehaviorConfigurationFailed"), MessageBoxButton.OK);
                    else
                        NotificationPanel.Instance.AddNotification(LocalizableStrings.GetString("BehaviorConfigurationFailed"), LocalizableStrings.GetString("BehaviorConfigurationFailedDescription"), ex.ToString(), MessageType.Warning);
                }
            }
        }
    }

    public class MoveUpMapBehaviorCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            ExtensionBehavior mapBehavior = parameter as ExtensionBehavior;
            if (mapBehavior == null)
                return false;

            int pos = View.Instance.ExtensionBehaviors.IndexOf(mapBehavior);
            if (pos > 0)
                return true;

            return false;
        }

        public override void Execute(object parameter)
        {
            ExtensionBehavior mapBehavior = parameter as ExtensionBehavior;
            if (mapBehavior == null)
                return;

            if (View.Instance == null && View.Instance.ExtensionBehaviors == null)
                return;

            int pos = View.Instance.ExtensionBehaviors.IndexOf(mapBehavior);
            if (pos < 1)
                return;

            ExtensionBehavior selectedBehavior = View.Instance.ExtensionBehaviors[pos];
            if (selectedBehavior != null)
            {
                View.Instance.ExtensionBehaviors.Remove(selectedBehavior);
                View.Instance.ExtensionBehaviors.Insert(pos - 1, selectedBehavior);
            }
        }
    }

    public class MoveDownMapBehaviorCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            ExtensionBehavior mapBehavior = parameter as ExtensionBehavior;
            if (mapBehavior == null)
                return false;

            int pos = View.Instance.ExtensionBehaviors.IndexOf(mapBehavior);
            if (pos > -1 && pos < View.Instance.ExtensionBehaviors.Count - 1)
                return true;

            return false;
        }

        public override void Execute(object parameter)
        {
            ExtensionBehavior mapBehavior = parameter as ExtensionBehavior;
            if (mapBehavior == null)
                return;

            if (View.Instance == null && ViewerApplicationControl.Instance.BehaviorsConfiguration == null)
                return;

            int pos = View.Instance.ExtensionBehaviors.IndexOf(mapBehavior);
            if (pos < 0 || pos >= View.Instance.ExtensionBehaviors.Count - 1)
                return;

            ExtensionBehavior selectedBehavior = View.Instance.ExtensionBehaviors[pos];
            if (selectedBehavior != null)
            {
                View.Instance.ExtensionBehaviors.Remove(selectedBehavior);
                View.Instance.ExtensionBehaviors.Insert(pos + 1, selectedBehavior);
            }
        }
    }
}
