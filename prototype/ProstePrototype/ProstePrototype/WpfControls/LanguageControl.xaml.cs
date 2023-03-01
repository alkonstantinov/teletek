using CefSharp;
using CefSharp.Wpf;
using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProstePrototype.WpfControls
{
    /// <summary>
    /// Interaction logic for LanguageControl.xaml
    /// </summary>
    public partial class LanguageControl : UserControl
    {
        public static readonly DependencyProperty DefaultLanguageProperty =
             DependencyProperty.Register(nameof(DefaultLanguage), typeof(string), typeof(LanguageControl),
                 new PropertyMetadata(null));
        public string DefaultLanguage
        {
            get { return (string)GetValue(DefaultLanguageProperty); }
            set { SetValue(DefaultLanguageProperty, value); }
        }

        public static readonly DependencyProperty CurrentLanguageProperty =
            DependencyProperty.Register(nameof(CurrentLanguage), typeof(string), typeof(LanguageControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentLanguageChanged));
        public string CurrentLanguage
        {
            get { return (string)GetValue(CurrentLanguageProperty); }
            set { SetValue(CurrentLanguageProperty, value); }
        }

        //public static readonly DependencyProperty LanguagesProperty =
        //    DependencyProperty.Register(nameof(Languages), typeof(Dictionary<string,Language>), typeof(LanguageControl), new PropertyMetadata(null));

        private Dictionary<string, Language> Languages = new Dictionary<string, Language>();
        //{
        //    get { return (Dictionary<string, Language>)GetValue(LanguagesProperty); }
        //    set { SetValue(LanguagesProperty, value); }
        //}

        private readonly string applicationDirectory;
        private string language;
        private JObject translationJson;
        //private string imageSource;

        public LanguageControl()
        {
            applicationDirectory = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

            InitializeComponent();

            this.DataContext = this;
            // Select the default language
            HandleRessources(null);
        }

        private void HandleRessources(string langKey)
        {
            string translationLocation = Path.Combine(applicationDirectory, "html/imports/translations.js").Replace(@"\", "/");
            string translations = Regex.Replace(File.ReadAllText(translationLocation), @"^const Translations = ", "", RegexOptions.IgnoreCase);
            JObject TransJson = JObject.Parse(translations.ToString());
            translationJson = (JObject)TransJson["translations"];
            JArray allLanguages = (JArray)TransJson["languages"];

            foreach (JObject el in allLanguages)
            {
                Language elLang = new Language
                {
                    Code = el["id"].Value<string>(),
                    IconPath = Path.Combine(applicationDirectory, el["icon"].Value<string>()),
                    Name = el["title"].Value<string>()
                };
                if (Languages == null || !Languages.ContainsKey(elLang.Code))
                    Languages.Add(elLang.Code, elLang);
            };
            var init = TransJson["initial"].Value<string>();
            var findInitId = allLanguages.First(e => (string)e["key"] == init);
            language = (langKey == null) ? findInitId["id"].Value<string>() : language = langKey;

            CurrentLanguage = language;
        }

        private void SetRessourceDictionary()
        {
            // set all the needed data in the DefaultDictionary
            ResourceDictionary dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("DefaultDictionary.xaml", UriKind.Relative);

            foreach (var dictKey in dictionary.Keys)
            {
                dictionary[dictKey] = (string)translationJson[dictKey][language];
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }

        private static void OnCurrentLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LanguageControl)d;
            var languageCode = (string)e.NewValue;
            if (control.Languages != null && control.Languages.ContainsKey(languageCode))
            {
                var languageClass = control.Languages[languageCode];
                control.iconImage.ImageSource = new BitmapImage(new Uri(languageClass.IconPath, UriKind.RelativeOrAbsolute));
                control.language= languageCode;
                control.SetRessourceDictionary();
                control.SetWebBrowsersLang();
            }
        }

        private void SetWebBrowsersLang()
        {
            // Get a reference to the MainWindow instance
            MainWindow win = (MainWindow)Window.GetWindow(this);
            
            if (language != null)
            {
                var script = $"toggleLang('{language}');";
                if (win != null) {
                    win.wb1.ExecuteScriptAsyncWhenPageLoaded(script);
                    win.wb2.ExecuteScriptAsyncWhenPageLoaded(script);
                }
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();
            BuildMenu(menu);
            menu.PlacementTarget = this;
            menu.IsOpen = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var languageCode = (string)menuItem.Tag;
            CurrentLanguage = languageCode;
        }

        private void BuildMenu(ContextMenu menuLanguages)
        {
            foreach (var language in Languages.Values)
            {
                var menuItem = new MenuItem();
                menuItem.Header = language.Name;
                menuItem.FontFamily = Application.Current.Resources["poppins_regular"] as FontFamily;
                menuItem.Tag = language.Code;
                var icon = new Image();
                icon.Source = new BitmapImage(new Uri(language.IconPath, UriKind.RelativeOrAbsolute));
                icon.Width = 16;
                icon.Height = 16;
                icon.Stretch = Stretch.Uniform;
                menuItem.Icon = icon;
                menuItem.Click += MenuItem_Click;
                menuLanguages.Items.Add(menuItem);
            }
        }
    }

    public class HalfValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = System.Convert.ToDouble(value);
            return doubleValue / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
