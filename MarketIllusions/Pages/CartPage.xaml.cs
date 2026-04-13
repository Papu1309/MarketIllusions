using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketIllusions.Connect;

namespace MarketIllusions.Pages
{
    public partial class CartPage : Page
    {
        public ObservableCollection<CartItem> CartItems { get; set; }

        public CartPage()
        {
            InitializeComponent();
            CartItems = new ObservableCollection<CartItem>(CartHelper.CartItems);
            CartItemsControl.ItemsSource = CartItems;
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = CartItems.Sum(item => item.Total);
            TotalTextBlock.Text = $"Итого: {total:C}";
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.Tag as CartItem;

            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                UpdateTotal();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.Tag as CartItem;

            if (item != null)
            {
                item.Quantity++;
                UpdateTotal();
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.Tag as CartItem;

            if (item != null)
            {
                CartItems.Remove(item);
                CartHelper.CartItems.Remove(item);
                UpdateTotal();
            }
        }

        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count > 0)
            {
                var result = MessageBox.Show("Очистить корзину?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CartItems.Clear();
                    CartHelper.ClearCart();
                    UpdateTotal();
                }
            }
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var order = new Orders
                {
                    UserId = App.CurrentUserId,
                    OrderDate = DateTime.Now,
                    TotalAmount = CartItems.Sum(item => item.Total),
                    Status = "Создан",
                    IsPaid = false,
                    IllusionLevel = 100
                };

                Connection.entities.Orders.Add(order);
                Connection.entities.SaveChanges();

                foreach (var item in CartItems)
                {
                    var orderDetail = new OrderDetails
                    {
                        OrderId = order.Id,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        IllusionNote = $"Заказ на {item.Product.Name}"
                    };

                    Connection.entities.OrderDetails.Add(orderDetail);
                }

                Connection.entities.SaveChanges();

                MessageBox.Show($"Заказ №{order.Id} оформлен!\nСумма: {order.TotalAmount:C}\nЧек будет пустым, как и полагается.",
                    "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                CartItems.Clear();
                CartHelper.ClearCart();
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}