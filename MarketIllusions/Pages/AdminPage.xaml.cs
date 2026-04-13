using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketIllusions.Connect;
using MarketIllusions.Windows;

namespace MarketIllusions.Pages
{
    public partial class AdminPage : Page
    {
        private Users selectedUser;
        private Products selectedProduct;
        private Orders selectedOrder;

        public AdminPage()
        {
            InitializeComponent();
            this.Loaded += AdminPage_Loaded;
        }

        private void AdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            LoadProducts();
            LoadOrders();
            LoadHistory();
        }

        private void LoadUsers()
        {
            try
            {
                var users = Connection.entities.Users
                    .Include("Roles")
                    .ToList();
                UsersDataGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = Connection.entities.Products
                    .Include("Categories")
                    .ToList();
                ProductsDataGrid.ItemsSource = products;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                var orders = Connection.entities.Orders
                    .Include("Users")
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
                OrdersDataGrid.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHistory()
        {
            try
            {
                var history = Connection.entities.ProductHistory
                    .Include("Products")
                    .Include("Users")
                    .OrderByDescending(h => h.ChangeDate)
                    .ToList();

                HistoryDataGrid.ItemsSource = history;

                if (history == null || !history.Any())
                {
                    System.Windows.MessageBox.Show("История изменений пуста",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}\n\n{ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
            LoadUsers();
        }

        private void RefreshUsersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = UsersDataGrid.SelectedItem as Users;

            if (selectedUser != null)
            {
                EditUserButton.Visibility = Visibility.Visible;
                DeleteUserButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditUserButton.Visibility = Visibility.Collapsed;
                DeleteUserButton.Visibility = Visibility.Collapsed;
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                UserEditWindow editWindow = new UserEditWindow(selectedUser);
                editWindow.ShowDialog();
                LoadUsers();
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null) return;

            var result = System.Windows.MessageBox.Show($"Удалить пользователя '{selectedUser.FullName}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (selectedUser.Roles.Name == "Администратор")
                    {
                        var adminCount = Connection.entities.Users
                            .Count(u => u.RoleId == 1);

                        if (adminCount <= 1)
                        {
                            System.Windows.MessageBox.Show("Нельзя удалить последнего администратора!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    var orders = Connection.entities.Orders
                        .Where(o => o.UserId == selectedUser.Id)
                        .ToList();

                    foreach (var order in orders)
                    {
                        var orderDetails = Connection.entities.OrderDetails
                            .Where(od => od.OrderId == order.Id)
                            .ToList();

                        foreach (var detail in orderDetails)
                        {
                            Connection.entities.OrderDetails.Remove(detail);
                        }

                        Connection.entities.Orders.Remove(order);
                    }

                    var illusions = Connection.entities.Illusions
                        .Where(i => i.TargetUserId == selectedUser.Id)
                        .ToList();

                    foreach (var illusion in illusions)
                    {
                        Connection.entities.Illusions.Remove(illusion);
                    }

                    Connection.entities.Users.Remove(selectedUser);
                    Connection.entities.SaveChanges();

                    LoadUsers();
                    System.Windows.MessageBox.Show("Пользователь удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ProductEditWindow editWindow = new ProductEditWindow();
            editWindow.ShowDialog();
            LoadProducts();
        }

        private void RefreshProductsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedProduct = ProductsDataGrid.SelectedItem as Products;

            if (selectedProduct != null)
            {
                EditProductButton.Visibility = Visibility.Visible;
                DeleteProductButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditProductButton.Visibility = Visibility.Collapsed;
                DeleteProductButton.Visibility = Visibility.Collapsed;
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProduct != null)
            {
                ProductEditWindow editWindow = new ProductEditWindow(selectedProduct);
                editWindow.ShowDialog();
                LoadProducts();
            }
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProduct == null) return;

            var result = System.Windows.MessageBox.Show($"Удалить товар '{selectedProduct.Name}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var orderDetails = Connection.entities.OrderDetails
                        .Where(od => od.ProductId == selectedProduct.Id)
                        .ToList();

                    foreach (var detail in orderDetails)
                    {
                        Connection.entities.OrderDetails.Remove(detail);
                    }

                    var history = Connection.entities.ProductHistory
                        .Where(ph => ph.ProductId == selectedProduct.Id)
                        .ToList();

                    foreach (var record in history)
                    {
                        Connection.entities.ProductHistory.Remove(record);
                    }

                    Connection.entities.Products.Remove(selectedProduct);
                    Connection.entities.SaveChanges();

                    LoadProducts();
                    System.Windows.MessageBox.Show("Товар удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedOrder = OrdersDataGrid.SelectedItem as Orders;

            if (selectedOrder != null)
            {
                ChangeOrderStatusButton.Visibility = Visibility.Visible;
            }
            else
            {
                ChangeOrderStatusButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeOrderStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrder == null) return;

            OrderStatusWindow statusWindow = new OrderStatusWindow(selectedOrder);
            statusWindow.ShowDialog();
            LoadOrders();
        }

        private void RefreshHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }
    }
}