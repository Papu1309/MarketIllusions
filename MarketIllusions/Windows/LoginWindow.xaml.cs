using System.Windows;
using MarketIllusions.Connect;

namespace MarketIllusions.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Заполните все поля";
                return;
            }

            var user = Connection.GetUser(login, password);

            if (user != null)
            {
                App.CurrentUserId = user.Id;
                App.CurrentUserRole = user.Roles.Name;
                App.CurrentUserName = user.FullName;

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Неверный логин или пароль";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}