using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketIllusions.Connect;
using System.Data.Entity;

namespace MarketIllusions.Pages
{
    public partial class IllusionistPage : Page
    {
        public IllusionistPage()
        {
            InitializeComponent();
            this.Loaded += IllusionistPage_Loaded;
        }

        private void IllusionistPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загружаем пользователей
                var users = Connection.entities.Users
                    .Where(u => u.RoleId == 2)
                    .ToList();
                TargetUserComboBox.ItemsSource = users;
                TargetUserComboBox.DisplayMemberPath = "FullName";
                TargetUserComboBox.SelectedValuePath = "Id";

                // Загружаем заказы
                var orders = Connection.entities.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
                OrderComboBox.ItemsSource = orders;
                OrderComboBox.DisplayMemberPath = "Id";
                OrderComboBox.SelectedValuePath = "Id";

                LoadIllusions();
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadIllusions()
        {
            try
            {
                var illusions = Connection.entities.Illusions
                    .Include("Users")
                    .Where(i => i.IllusionistId == App.CurrentUserId)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToList();
                IllusionsDataGrid.ItemsSource = illusions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке иллюзий: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                var illusions = Connection.entities.Illusions
                    .Where(i => i.IllusionistId == App.CurrentUserId)
                    .ToList();

                int totalIllusions = illusions.Count;
                double avgPower = illusions.Any() ? (double)illusions.Average(i => i.Power) : 0;
                int maxPower = illusions.Any() ? (int)illusions.Max(i => i.Power) : 0;
                int minPower = illusions.Any() ? (int)illusions.Min(i => i.Power) : 0;

                var today = DateTime.Today;
                int todayIllusions = illusions.Count(i => i.CreatedDate >= today);

                StatsTextBlock.Text = $"Всего иллюзий: {totalIllusions}\n" +
                                     $"Создано сегодня: {todayIllusions}\n" +
                                     $"Средняя сила: {avgPower:F1}\n" +
                                     $"Максимальная сила: {maxPower}\n" +
                                     $"Минимальная сила: {minPower}";
            }
            catch (Exception ex)
            {
                StatsTextBlock.Text = $"Ошибка при загрузке статистики: {ex.Message}";
            }
        }

        private void CreateIllusionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(IllusionNameTextBox.Text))
                {
                    MessageBox.Show("Введите название иллюзии", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(IllusionPowerTextBox.Text, out int power))
                {
                    power = 50;
                }

                if (power < 0) power = 0;
                if (power > 100) power = 100;

                var illusion = new Illusions
                {
                    Name = IllusionNameTextBox.Text,
                    Description = IllusionDescriptionTextBox.Text,
                    IllusionistId = App.CurrentUserId,
                    Power = power,
                    CreatedDate = DateTime.Now
                };

                if (TargetUserComboBox.SelectedValue != null)
                {
                    illusion.TargetUserId = (int)TargetUserComboBox.SelectedValue;
                }

                if (OrderComboBox.SelectedValue != null)
                {
                    illusion.OrderId = (int)OrderComboBox.SelectedValue;
                }

                Connection.entities.Illusions.Add(illusion);
                Connection.entities.SaveChanges();

                MessageBox.Show("Иллюзия создана!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем поля
                IllusionNameTextBox.Text = "";
                IllusionDescriptionTextBox.Text = "";
                IllusionPowerTextBox.Text = "50";
                TargetUserComboBox.SelectedIndex = -1;
                OrderComboBox.SelectedIndex = -1;

                LoadIllusions();
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании иллюзии: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshIllusionsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadIllusions();
        }

        private void RefreshStatsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }
    }
}