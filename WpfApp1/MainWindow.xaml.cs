using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Безопасно выбрать значения по умолчанию, если не выбрано
            EnsureSelected(edinizm, 0);   // °C
            EnsureSelected(edinizm_v, 1); // °F

            // Первый пересчёт — когда всё уже загружено
            this.Loaded += (_, __) => Alllogic();
        }

        private void knopka_Click(object sender, RoutedEventArgs e) => Alllogic();
        private void tempret_izm(object sender, RoutedEventArgs e) => Alllogic();
        private void edinizm_SelectionChanged(object sender, SelectionChangedEventArgs e) => Alllogic();
        private void tempvibr_TextChanged(object sender, TextChangedEventArgs e) => Alllogic();

        // ---------- Основная логика ----------
        private void Alllogic()
        {
            if (tempvibr == null || edinizm == null || edinizm_v == null || txtResult == null)
                return;

            // гарантируем выбранные элементы (и убираем NRE)
            EnsureSelected(edinizm, 0);
            EnsureSelected(edinizm_v, 1);

            var styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var culture = CultureInfo.CurrentCulture;

            // читаем число из TextBox
            if (!double.TryParse(tempvibr.Text, styles, culture, out double input))
            {
                txtResult.Text = "Введите корректное число.";
                return;
            }

            string fromUnit = GetUnit(edinizm);
            string toUnit = GetUnit(edinizm_v);

            try
            {
                double res = ConvertTemperature(input, fromUnit, toUnit);
                txtResult.Text = $"{input} {fromUnit} = {res:F2} {toUnit}";
            }
            catch (Exception ex)
            {
                txtResult.Text = ex.Message;
            }
        }

        // Нормализуем единицы измерения и безопасно читаем ComboBox
        private static string GetUnit(ComboBox cb)
        {
            if (cb == null) return "°C";

            if (cb.SelectedItem is ComboBoxItem item && item.Content is string s1)
                return NormalizeUnit(s1);

            // если SelectedItem ещё не выставлен — пробуем первый пункт
            if (cb.Items.Count > 0 && cb.Items[0] is ComboBoxItem first && first.Content is string s2)
                return NormalizeUnit(s2);

            return "°C";
        }

        private static string NormalizeUnit(string txt)
        {
            if (string.Equals(txt, "K", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(txt, "°K", StringComparison.OrdinalIgnoreCase))
                return "K";

            if (string.Equals(txt, "°C", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(txt, "C", StringComparison.OrdinalIgnoreCase))
                return "°C";

            if (string.Equals(txt, "°F", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(txt, "F", StringComparison.OrdinalIgnoreCase))
                return "°F";

            return txt;
        }

        private static void EnsureSelected(ComboBox cb, int indexIfNone)
        {
            if (cb == null) return;
            if (cb.SelectedIndex < 0 && cb.Items.Count > indexIfNone)
                cb.SelectedIndex = indexIfNone;
        }

        private static double ConvertTemperature(double value, string from, string to)
        {
            // в Цельсии
            double celsius = from switch
            {
                "°C" => value,
                "°F" => (value - 32.0) * 5.0 / 9.0,
                "K" => value - 273.15,
                _ => throw new InvalidOperationException("Неизвестная единица измерения.")
            };

            // из Цельсия в целевую
            return to switch
            {
                "°C" => celsius,
                "°F" => celsius * 9.0 / 5.0 + 32.0,
                "K" => celsius + 273.15,
                _ => throw new InvalidOperationException("Неизвестная единица измерения.")
            };
        }
    }
}
