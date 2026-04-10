using System.Windows;
using System.Windows.Controls;
using MarketIllusions.Pages;

namespace MarketIllusions.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UserNameTextBlock.Text = App.CurrentUserName;
            UserRoleTextBlock.Text = App.CurrentUserRole;

            // Показываем кнопки в зависимости от роли
            if (App.CurrentUserRole == "Администратор")
            {
                AdminButton.Visibility = Visibility.Visible;
                CartButton.Visibility = Visibility.Collapsed;
            }
            else if (App.CurrentUserRole == "Мастер иллюзий")
            {
                IllusionistButton.Visibility = Visibility.Visible;
                CartButton.Visibility = Visibility.Collapsed;
            }

            // Загружаем каталог по умолчанию
            MainFrame.Navigate(new CatalogPage());
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage());
        }

        private void IllusionistButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IllusionistPage());
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CartPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}