using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;

namespace ReplayFixer.Views.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsPage: Page
    {

        public ObservableCollection<string> ThemeOptions;
        public SettingsPage(IThemeService themeService, IOptions<AppConfig> options)
        {
            InitializeComponent();
            ThemeOptions = new ObservableCollection<string>() { "Light", "Dark" };
            themeOptionsComboBox.ItemsSource = ThemeOptions;

            themeOptionsComboBox.SelectedItem = options.Value.Theme;
            themeOptionsComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) => {
                var themeString = (sender as ComboBox)?.SelectedItem.ToString();
                options.Value.Theme = themeString ?? "Light";
                themeService.SetTheme((ThemeType)Enum.Parse(typeof(ThemeType), themeString ?? "Light"));
            };

        }
    }
}
