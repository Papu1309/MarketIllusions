using System;
using System.Linq;
using System.Windows;
using MarketIllusions.Connect;

namespace MarketIllusions.Windows
{
    public partial class UserEditWindow : Window
    {
        private Users user;

        public UserEditWindow(Users user)
        {
            InitializeComponent();
            this.user = user;
            LoadRoles();
            LoadUserData();
        }

        private void LoadRoles()
        {
            var roles = Connection.entities.Roles.ToList();
            RoleComboBox.ItemsSource = roles;
            RoleComboBox.DisplayMemberPath = "Name";
            RoleComboBox.SelectedValuePath = "Id";
        }

        private void LoadUserData()
        {
            LoginTextBox.Text = user.Login;
            FullNameTextBox.Text = user.FullName;
            EmailTextBox.Text = user.Email;
            PhoneTextBox.Text = user.Phone;
            SuggestibilityTextBox.Text = user.SuggestibilityLevel.ToString();
            RoleComboBox.SelectedValue = user.RoleId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(FullNameTextBox.Text))
                {
                    MessageBox.Show("Введите полное имя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                user.FullName = FullNameTextBox.Text;
                user.Email = EmailTextBox.Text;
                user.Phone = PhoneTextBox.Text;

                if (int.TryParse(SuggestibilityTextBox.Text, out int suggestibility))
                {
                    if (suggestibility < 0) suggestibility = 0;
                    if (suggestibility > 100) suggestibility = 100;
                    user.SuggestibilityLevel = suggestibility;
                }

                user.RoleId = (int)RoleComboBox.SelectedValue;

                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    user.Password = PasswordBox.Password;
                }

                Connection.entities.SaveChanges();

                MessageBox.Show("Данные пользователя обновлены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}