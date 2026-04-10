using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MarketIllusions.Connect;
using Microsoft.Win32;

namespace MarketIllusions.Pages
{
    public partial class ProfilePage : Page
    {
        private Users currentUser;
        private string selectedAvatarPath;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            currentUser = Connection.entities.Users
                .FirstOrDefault(u => u.Id == App.CurrentUserId);

            if (currentUser != null)
            {
                LoginTextBox.Text = currentUser.Login;
                FullNameTextBox.Text = currentUser.FullName;
                EmailTextBox.Text = currentUser.Email;
                PhoneTextBox.Text = currentUser.Phone;
                SuggestibilityTextBox.Text = currentUser.SuggestibilityLevel.ToString();

                if (!string.IsNullOrEmpty(currentUser.AvatarPath) &&
                    File.Exists(currentUser.AvatarPath))
                {
                    try
                    {
                        AvatarImage.Source = new BitmapImage(new Uri(currentUser.AvatarPath));
                    }
                    catch { }
                }
            }
        }

        private void ChooseAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedAvatarPath = openFileDialog.FileName;
                try
                {
                    AvatarImage.Source = new BitmapImage(new Uri(selectedAvatarPath));
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

                currentUser.FullName = FullNameTextBox.Text;
                currentUser.Email = EmailTextBox.Text;
                currentUser.Phone = PhoneTextBox.Text;

                if (int.TryParse(SuggestibilityTextBox.Text, out int suggestibility))
                {
                    if (suggestibility < 0) suggestibility = 0;
                    if (suggestibility > 100) suggestibility = 100;
                    currentUser.SuggestibilityLevel = suggestibility;
                }

                if (!string.IsNullOrEmpty(selectedAvatarPath))
                {
                    string avatarFolder = "Avatars";
                    if (!Directory.Exists(avatarFolder))
                    {
                        Directory.CreateDirectory(avatarFolder);
                    }

                    string newAvatarPath = Path.Combine(avatarFolder,
                        $"{currentUser.Id}_{Path.GetFileName(selectedAvatarPath)}");

                    File.Copy(selectedAvatarPath, newAvatarPath, true);
                    currentUser.AvatarPath = newAvatarPath;
                }

                if (!string.IsNullOrEmpty(NewPasswordBox.Password))
                {
                    if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        MessageBox.Show("Пароли не совпадают", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    currentUser.Password = NewPasswordBox.Password;
                }

                Connection.entities.SaveChanges();

                App.CurrentUserName = currentUser.FullName;

                MessageBox.Show("Профиль обновлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserData();
            NewPasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";
        }
    }
}