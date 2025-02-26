using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataGridWithNewRow;
public partial class MainWindow : Window
{
    new MainWindowViewModel DataContext => (MainWindowViewModel)base.DataContext;
    public MainWindow()
    {
        InitializeComponent();
        DataGridIP.BeginningEdit += (sender, e) =>
        {
            Title = "Main Window - EDITING";
        };
        DataGridIP.CellEditEnding += (sender, e) =>
        {
            Title = "Main Window";
            switch (e.EditAction)
            {
                case DataGridEditAction.Cancel:
                    if (e.Row.IsNewItem)
                    {
                        DataContext.IpData.Remove((Record)e.Row.Item);
                        // Refresh the ephemeral row
                        DataGridIP.CanUserAddRows = false;
                        DataGridIP.CanUserAddRows = true;
                    }
                    break;
            };
        };
        DataGridIP.PreparingCellForEdit += (sender, e) =>
        {
            if (e.EditingElement is ContentPresenter cp)
            {
                cp.ApplyTemplate(); // Make sure the template elements exist.
                if(FindVisualChild<TextBox>(cp) is { } textBox)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        textBox.SelectAll();
                        textBox.Focus();
                    });
                }
            }
        };
        // Add test data items
        for (int i = 0; i < 3; i++)  DataContext.IpData.Add(new());
    }
    private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            if (child is T foundChild)
            {
                return foundChild;
            }
            T? childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }
        return default;
    }
    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        if(sender is Button button)
        {
            if(button.CommandParameter is Record record)
            {
                MessageBox.Show($"{record.Descrizione} Pulsante.");
            }
        }
    }
}

class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<Record> IpData { get; } = new();
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public event PropertyChangedEventHandler? PropertyChanged;
}

[DebuggerDisplay("{Descrizione}")]
class Record
{
    static int _autoIncrement = 1;
    public string Descrizione { get; set; } = $"Record {_autoIncrement++}";
    public string? IP { get; set; } = "0.0.0.0";
}
