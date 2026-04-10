using System.Windows;

namespace MarketIllusions
{
    public partial class App : Application
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUserRole { get; set; }
        public static string CurrentUserName { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Устанавливаем культуру для корректного отображения валюты
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("ru-RU");
        }
    }
}