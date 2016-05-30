/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(ICommand))]
	//[DisplayName("RestoreDefaultThemeDisplayName")]
	//[Category("CategoryTheme")]
	//[Description("RestoreDefaultThemeDescription")]
    public class RestoreDefaultThemeCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            if (View.Instance == null)
                return;

            if (View.Instance.ApplicationColorSet == null)
                View.Instance.ApplicationColorSet = new Core.ApplicationColorSet();
            else
            {
                Core.ApplicationColorSet set = new Core.ApplicationColorSet();
                View.Instance.ApplicationColorSet.AccentColor = set.AccentColor;
                View.Instance.ApplicationColorSet.AccentTextColor = set.AccentTextColor;
                View.Instance.ApplicationColorSet.BackgroundEndGradientColor = set.BackgroundEndGradientColor;
                View.Instance.ApplicationColorSet.BackgroundStartGradientColor = set.BackgroundStartGradientColor;
                View.Instance.ApplicationColorSet.BackgroundTextColor = set.BackgroundTextColor;
                View.Instance.ApplicationColorSet.SelectionColor = set.SelectionColor;
                View.Instance.ApplicationColorSet.SelectionOutlineColor = set.SelectionOutlineColor;
            }
        }
    }
}
