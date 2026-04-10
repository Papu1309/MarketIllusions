using System;
using System.Windows;
using MarketIllusions.Connect;

namespace MarketIllusions.Windows
{
    public partial class IllusionCreateWindow : Window
    {
        private Products product;

        public IllusionCreateWindow(Products product)
        {
            InitializeComponent();
            this.product = product;
            ProductInfoTextBlock.Text = $"{product.Name} - {product.InvisibleProperty}";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    MessageBox.Show("Введите название иллюзии", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(PowerTextBox.Text, out int power))
                {
                    power = 75;
                }

                if (power < 0) power = 0;
                if (power > 100) power = 100;

                var illusion = new Illusions
                {
                    Name = NameTextBox.Text,
                    Description = $"{DescriptionTextBox.Text}\n\nТовар: {product.Name}\nСвойство: {product.InvisibleProperty}",
                    IllusionistId = App.CurrentUserId,
                    Power = power,
                    CreatedDate = DateTime.Now
                };

                Connection.entities.Illusions.Add(illusion);
                Connection.entities.SaveChanges();

                MessageBox.Show($"Иллюзия '{NameTextBox.Text}' создана!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании иллюзии: {ex.Message}",
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