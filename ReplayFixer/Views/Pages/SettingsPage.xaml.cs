using Microsoft.Extensions.Options;
using ReplayFixer.Models.Helpers;
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
        public ObservableCollection<Environment.SpecialFolder> PreferedStartingPaths;

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

            PreferedStartingPaths = new ObservableCollection<Environment.SpecialFolder>();
            foreach (Environment.SpecialFolder specialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                PreferedStartingPaths.Add(specialFolder);
            }

            preferedStartingPathComboBox.ItemsSource = PreferedStartingPaths;
            preferedStartingPathComboBox.SelectedItem = options.Value.PreferedStartingPath;
            preferedStartingPathComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                Environment.SpecialFolder specialFolder = (Environment.SpecialFolder)(sender as ComboBox)?.SelectedItem;
                options.Value.PreferedStartingPath = specialFolder;
            };

            languageOptionsComboBox.ItemsSource = new ObservableCollection<string> { "English", "Español" } ;
            switch (options.Value.Language)
            {
                case "en-US":
                    languageOptionsComboBox.SelectedItem = "English";
                    break;
                case "es-DO":
                    languageOptionsComboBox.SelectedItem = "Español";
                    break;

            }
            languageOptionsComboBox.SelectedItem = options.Value.Language;

            languageOptionsComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                if (sender is ComboBox) {
                    if ((sender as ComboBox).SelectedItem == "English")
                        options.Value.Language = "en-US";
                    else if ((sender as ComboBox).SelectedItem == "Español")
                        options.Value.Language = "es-DO";
                }
                SettingsHelpers.AddOrUpdateAppSetting("AppConfig:Language", options.Value.Language);
            };

            delimiterTextBox.Text = Encoding.UTF8.GetString(new byte[] { options.Value.Delimiter });
            delimiterTextBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                if (sender is TextBox)
                {
                    var delimiter = (sender as TextBox).Text;
                    if (delimiter.Length > 0)
                    {
                        options.Value.Delimiter = Encoding.UTF8.GetBytes(delimiter)[0];
                        delimiterHexTextBox.Text = "0x" + options.Value.Delimiter.ToString("X2");
                        SettingsHelpers.AddOrUpdateAppSetting("AppConfig:Delimiter", options.Value.Delimiter.ToString());
                    }
                }
            };
            delimiterHexTextBox.Text = "0x"+options.Value.Delimiter.ToString("X2");
        }
    }
}
