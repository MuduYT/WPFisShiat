using System.Windows;
using BibWpf.ViewModels.Dialogs;

namespace BibWpf.Dialogs;

public partial class ConfirmDeleteWindow : Window
{
    public ConfirmDeleteRequest Request { get; }
    public bool UserRequestedCascade { get; private set; }

    public ConfirmDeleteWindow(ConfirmDeleteRequest request)
    {
        Request = request;
        InitializeComponent();
        DataContext = this;

        bool hasAffected = request.AffectedEntries.Count > 0;
        
        if (hasAffected)
        {
            AffectedHeader.Visibility = Visibility.Visible;
            AffectedScroll.Visibility = Visibility.Visible;

            if (request.CanCascade)
            {
                DeleteButton.IsEnabled = false;
                CascadeCheckBox.Visibility = Visibility.Visible;
                RestrictWarning.Visibility = Visibility.Collapsed;
            }
            else
            {
                DeleteButton.IsEnabled = false;
                CascadeCheckBox.Visibility = Visibility.Collapsed;
                RestrictWarning.Visibility = Visibility.Visible;
            }
        }
        else
        {
            AffectedHeader.Visibility = Visibility.Collapsed;
            AffectedScroll.Visibility = Visibility.Collapsed;
            DeleteButton.IsEnabled = true;
            CascadeCheckBox.Visibility = Visibility.Collapsed;
            RestrictWarning.Visibility = Visibility.Collapsed;
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (Request.AffectedEntries.Count > 0 && Request.CanCascade)
        {
            UserRequestedCascade = true;
        }
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnCascadeCheckBoxChecked(object sender, RoutedEventArgs e)
    {
        DeleteButton.IsEnabled = CascadeCheckBox.IsChecked == true;
    }
}
