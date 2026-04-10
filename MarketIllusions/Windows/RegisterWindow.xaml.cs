using System;
using System.Linq;
using System.Windows;
using MarketIllusions.Connect;

namespace MarketIllusions.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(fullName))
            {
                ErrorTextBlock.Text = "Заполните обязательные поля (*)";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorTextBlock.Text = "Пароли не совпадают";
                return;
            }

            var existingUser = Connection.entities.Users.FirstOrDefault(u => u.Login == login);
            if (existingUser != null)
            {
                ErrorTextBlock.Text = "Пользователь с таким логином уже существует";
                return;
            }

            int suggestibility = 50;
            if (!string.IsNullOrEmpty(SuggestibilityTextBox.Text))
            {
                int.TryParse(SuggestibilityTextBox.Text, out suggestibility);
                if (suggestibility < 0) suggestibility = 0;
                if (suggestibility > 100) suggestibility = 100;
            }

            var userRole = Connection.entities.Roles.FirstOrDefault(r => r.Name == "Пользователь");

            var newUser = new Users
            {
                Login = login,
                Password = password,
                FullName = fullName,
                Email = email,
                Phone = phone,
                SuggestibilityLevel = suggestibility,
                RoleId = userRole.Id,
                RegistrationDate = DateTime.Now,
                AvatarPath = ""
            };

            Connection.entities.Users.Add(newUser);
            Connection.entities.SaveChanges();

            MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}