using System;
using System.Windows;
using MarketIllusions.Connect;

namespace MarketIllusions.Windows
{
    public partial class OrderStatusWindow : Window
    {
        private Orders order;

        public OrderStatusWindow(Orders order)
        {
            InitializeComponent();
            this.order = order;
            LoadStatuses();
            LoadOrderData();
        }

        private void LoadStatuses()
        {
            string[] statuses = { "Создан", "В обработке", "Выполнен", "Отменен" };
            StatusComboBox.ItemsSource = statuses;
        }

        private void LoadOrderData()
        {
            OrderInfoTextBlock.Text = $"Заказ №{order.Id} от {order.OrderDate:dd.MM.yyyy}\nСумма: {order.TotalAmount:C}";
            StatusComboBox.SelectedItem = order.Status;
            IsPaidCheckBox.IsChecked = order.IsPaid ?? false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                order.Status = StatusComboBox.SelectedItem.ToString();
                order.IsPaid = IsPaidCheckBox.IsChecked ?? false;

                Connection.entities.SaveChanges();

                MessageBox.Show("Статус заказа обновлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}",
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