using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BibWpf.Converters
{
    public static class DataGridIndexHelper
    {
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.RegisterAttached(
                "Index",
                typeof(int),
                typeof(DataGridIndexHelper),
                new PropertyMetadata(0));

        public static int GetIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(IndexProperty);
        }

        public static void SetIndex(DependencyObject obj, int value)
        {
            obj.SetValue(IndexProperty, value);
        }

        public static readonly DependencyProperty AutoIndexProperty =
            DependencyProperty.RegisterAttached(
                "AutoIndex",
                typeof(bool),
                typeof(DataGridIndexHelper),
                new PropertyMetadata(false, OnAutoIndexChanged));

        public static bool GetAutoIndex(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoIndexProperty);
        }

        public static void SetAutoIndex(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoIndexProperty, value);
        }

        private static void OnAutoIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.LoadingRow += OnLoadingRow;
                    dataGrid.UnloadingRow += OnUnloadingRow;
                    dataGrid.Sorting += OnSorting;
                }
                else
                {
                    dataGrid.LoadingRow -= OnLoadingRow;
                    dataGrid.UnloadingRow -= OnUnloadingRow;
                    dataGrid.Sorting -= OnSorting;
                }
            }
        }

        private static void OnLoadingRow(object? sender, DataGridRowEventArgs e)
        {
            e.Row.SetValue(IndexProperty, e.Row.GetIndex() + 1);
        }

        private static void OnUnloadingRow(object? sender, DataGridRowEventArgs e)
        {
            e.Row.ClearValue(IndexProperty);
        }

        private static void OnSorting(object? sender, DataGridSortingEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                // Alle Indizes neu berechnen nach der Sortierung
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RefreshAllIndexes(dataGrid);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private static void RefreshAllIndexes(DataGrid dataGrid)
        {
            foreach (var item in dataGrid.Items)
            {
                if (dataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                {
                    row.SetValue(IndexProperty, row.GetIndex() + 1);
                }
            }
        }
    }
}
