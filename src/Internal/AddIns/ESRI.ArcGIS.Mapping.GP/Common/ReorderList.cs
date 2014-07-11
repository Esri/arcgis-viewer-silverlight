/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Controls;
using System.Collections;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.GP
{
    [TemplatePart(Name = PART_ListBox, Type = typeof(ListBox))]
    [TemplatePart(Name = PART_MoveUpButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_MoveDownButton, Type = typeof(Button))]
    public class ReorderList : Control
    {
        private const string PART_ListBox = "ListBox";
        private const string PART_MoveUpButton = "MoveUpButton";
        private const string PART_MoveDownButton = "MoveDownButton";
        ListBox ListBox { get; set; }
        Button MoveUpButton { get; set; }
        Button MoveDownButton { get; set; }
        
        public ReorderList()
        {
            DefaultStyleKey = typeof(ReorderList);
            MoveDown = new DelegateCommand(moveDown, canMoveDown);
            MoveUp = new DelegateCommand(moveUp, canMoveUp);
        }

        public override void OnApplyTemplate()
        {
            if (ListBox != null)
                ListBox.SelectionChanged -= ListBox_SelectionChanged;
            base.OnApplyTemplate();
            ListBox = GetTemplateChild(PART_ListBox) as ListBox;
            if (ListBox != null)
                ListBox.SelectionChanged += ListBox_SelectionChanged;

            updateItemsSource();
        }

        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            canMoveDownChanged();
            canMoveUpChanged();
        }

        public static DependencyProperty ItemsProperty = DependencyProperty.Register(
            "ItemsProperty", typeof(ObservableCollection<string>), typeof(ReorderList), new PropertyMetadata(OnItemsPropertyChanged));

        public ObservableCollection<string> Items
        {
            get { return GetValue(ItemsProperty) as ObservableCollection<string>; }
            set { SetValue(ItemsProperty, value);  }
        }

        private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ReorderList r = d as ReorderList;
            if (e.NewValue != null && r.ListBox != null)
                r.updateItemsSource();
        }

        private void updateItemsSource()
        {
            if (ListBox != null && Items != null)
            {
                ListBox.ItemsSource = Items;
                if (ListBox.Items.Count > 0)
                    ListBox.SelectedIndex = 0;
            }

            canMoveDownChanged();
            canMoveUpChanged();
        }

        #region MoveUp Command
        private void moveUp(object commandParameter)
        {
            int selIndex = ListBox.SelectedIndex;
            if (selIndex < 1)
                return;
            int newIndex = selIndex - 1;
            string selItem = ListBox.SelectedItem as string;
            Items.RemoveAt(selIndex);
            Items.Insert(newIndex, selItem);
            ListBox.SelectedIndex = newIndex;
        }

        private void canMoveUpChanged()
        {
            (MoveUp as DelegateCommand).RaiseCanExecuteChanged();
        }

        private bool canMoveUp(object commandParameter)
        {
            if (ListBox == null)
                return false;
            if (ListBox.Items.Count > 1 && ListBox.SelectedIndex > 0)
                return true;
            return false;
        }

        public ICommand MoveUp
        {
            get { return (ICommand)GetValue(MoveUpProperty); }
            private set { SetValue(MoveUpProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Execute"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MoveUpProperty =
            DependencyProperty.Register("MoveUp", typeof(ICommand), typeof(ReorderList), null);
        #endregion

        #region MoveDown Command
        private void moveDown(object commandParameter)
        {
            int selIndex = ListBox.SelectedIndex;
            if (selIndex >= (ListBox.Items.Count - 1))
                return;
            int newIndex = selIndex + 1;
            string selItem = ListBox.SelectedItem as string;
            Items.RemoveAt(selIndex);
            Items.Insert(newIndex, selItem);
            ListBox.SelectedIndex = newIndex;
        }

        private void canMoveDownChanged()
        {
            (MoveDown as DelegateCommand).RaiseCanExecuteChanged();
        }

        private bool canMoveDown(object commandParameter)
        {
            if (ListBox == null)
                return false;
            if (ListBox.Items.Count > 1 && ListBox.SelectedIndex < (ListBox.Items.Count - 1))
                return true;
            return false;
        }

        public ICommand MoveDown
        {
            get { return (ICommand)GetValue(MoveDownProperty); }
            private set { SetValue(MoveDownProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Execute"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MoveDownProperty =
            DependencyProperty.Register("MoveDown", typeof(ICommand), typeof(ReorderList), null);
        #endregion
    }
}
