Try reconfiguring your columns so that they have both a `CellTemplate` and a `CellEditingTemplate` as shown below. For debugging, I also find it helpful to monitor the editing state in the title bar of the main window. In my code example, as a matter of personal style, a textbox that receives focus will `SelectAll()` and also the `Escape` key action is made more immediate when editing a new record.

___

So, trying to stay as close as I can to the XAML you provided for the `DataGrid`:

```xaml
<DataGrid 
    Name="DataGridIP" 
    CanUserAddRows="True"
    AutoGenerateColumns="False"
    Margin="10,0,10,10" 
    BorderBrush="Transparent"
    RowHeight="40" 
    HorizontalAlignment="Stretch"
    CanUserResizeRows="False"
    CanUserResizeColumns="False"
    ItemsSource="{Binding IpData}">
    <DataGrid.Columns>
        <DataGridTemplateColumn Header="Action" Width="80">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <Button
                        Width="30" 
                        Height="30"
                        BorderBrush="Transparent"
                        FocusVisualStyle="{x:Null}"
                        Cursor="Hand"
                        Click="OnActionButtonClick"
                        CommandParameter="{Binding .}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
        <DataGridTemplateColumn Header="Description" Width="*">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <TextBlock 
                        Text="{Binding Descrizione}" 
                        FontFamily="{DynamicResource Abel}" 
                        FontSize="18"
                        VerticalAlignment="Center"
                        TextWrapping="Wrap"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
            <DataGridTemplateColumn.CellEditingTemplate>
                <DataTemplate>
                    <TextBox 
                        Text="{Binding Descrizione, UpdateSourceTrigger=PropertyChanged}"
                        FontFamily="{DynamicResource Abel}" 
                        FontSize="18"
                        BorderBrush="Transparent"
                        BorderThickness="0"
                        VerticalContentAlignment="Center"
                        HorizontalScrollBarVisibility="Auto"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellEditingTemplate>
        </DataGridTemplateColumn>
        <DataGridTemplateColumn Header="IP Address" Width="*">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <TextBlock 
                        Text="{Binding IP}" 
                        FontFamily="{DynamicResource Oswald}" 
                        FontSize="18"
                        VerticalAlignment="Center"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
            <DataGridTemplateColumn.CellEditingTemplate>
                <DataTemplate>
                    <TextBox
                        Text="{Binding IP, UpdateSourceTrigger=PropertyChanged}"
                        FontFamily="{DynamicResource Oswald}"
                        FontSize="18" 
                        BorderBrush="Transparent"
                        BorderThickness="0" 
                        VerticalContentAlignment="Center"
                        HorizontalScrollBarVisibility="Auto"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellEditingTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```
___

**Minimal New Item Flow Example**

Using lambdas to subscribe to the `DataGridIP` events in the CTOR, one working implmentation is shown below.

- Mouse double-click to edit.
- In response, turn off `IsDataGridReadOnly` and update the title bar.
- Make it read-only again when a row edit is committed or cancelled.

Also shown is code that applies a `SelectAll` to the textbox when editing begins, and a block to customize column generation.

```csharp
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

private void AnyTextBoxGotFocus(object? sender, RoutedEventArgs? e)
{
    if(sender is TextBox textBox)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (!DataContext.IsDataGridReadOnly)
            {
                textBox.SelectAll();
            }
            textBox.Focus();
        });
    }
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
```
___

**Models**

```
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
```


