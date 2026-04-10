using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MarketIllusions.Connect;
using MarketIllusions.Windows;
using System.Data.Entity;

namespace MarketIllusions.Pages
{
    public partial class CatalogPage : Page
    {
        private List<Products> allProducts;
        private List<Categories> categories;

        public CatalogPage()
        {
            InitializeComponent();
            LoadCategories();

            this.Loaded += CatalogPage_Loaded;
        }

        private void CatalogPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void LoadCategories()
        {
            categories = Connection.entities.Categories.ToList();

            if (CategoryFilterComboBox != null)
            {
                foreach (var category in categories)
                {
                    CategoryFilterComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = category.Name,
                        Tag = category.Id
                    });
                }
            }
        }

        private void LoadProducts(string searchText = "", int? categoryId = null)
        {
            try
            {
                var query = Connection.entities.Products.AsQueryable();

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(p => p.Name.Contains(searchText) ||
                                            p.Description.Contains(searchText));
                }

                allProducts = query.ToList();

                if (ProductsWrapPanel != null)
                {
                    DisplayProducts(allProducts);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayProducts(List<Products> products)
        {
            if (ProductsWrapPanel == null)
                return;

            ProductsWrapPanel.Children.Clear();

            if (products == null || products.Count == 0)
            {
                TextBlock noProductsText = new TextBlock();
                noProductsText.Text = "Товары не найдены";
                noProductsText.FontSize = 14;
                noProductsText.Foreground = new SolidColorBrush(Colors.Gray);
                noProductsText.HorizontalAlignment = HorizontalAlignment.Center;
                noProductsText.Margin = new Thickness(0, 50, 0, 0);
                ProductsWrapPanel.Children.Add(noProductsText);
                return;
            }

            foreach (var product in products)
            {
                var card = CreateProductCard(product);
                if (card != null)
                {
                    ProductsWrapPanel.Children.Add(card);
                }
            }
        }

        private Border CreateProductCard(Products product)
        {
            try
            {
                var border = new Border();
                border.Style = (Style)FindResource("CardStyle");
                border.Width = 250;
                border.Height = 380;

                var stackPanel = new StackPanel();

                // Изображение
                var image = new Image();
                image.Width = 200;
                image.Height = 150;
                image.Stretch = Stretch.Uniform;
                image.Margin = new Thickness(0, 0, 0, 10);

                try
                {
                    if (!string.IsNullOrEmpty(product.ImagePath) &&
                        System.IO.File.Exists(product.ImagePath))
                    {
                        image.Source = new BitmapImage(new Uri(product.ImagePath));
                    }
                    else
                    {
                        image.Source = CreatePlaceholderImage(product.CategoryId);
                    }
                }
                catch
                {
                    image.Source = CreatePlaceholderImage(product.CategoryId);
                }

                stackPanel.Children.Add(image);

                // Название
                var nameText = new TextBlock();
                nameText.Text = product.Name;
                nameText.FontWeight = FontWeights.Bold;
                nameText.FontSize = 14;
                nameText.Margin = new Thickness(0, 0, 0, 5);
                nameText.TextWrapping = TextWrapping.Wrap;
                stackPanel.Children.Add(nameText);

                // Описание
                var descText = new TextBlock();
                descText.Text = product.Description;
                descText.FontSize = 11;
                descText.Foreground = new SolidColorBrush(Colors.Gray);
                descText.Margin = new Thickness(0, 0, 0, 5);
                descText.TextWrapping = TextWrapping.Wrap;
                descText.MaxHeight = 40;
                stackPanel.Children.Add(descText);

                // Цена
                var priceText = new TextBlock();
                priceText.Text = $"Цена: {product.Price:C}";
                priceText.FontSize = 12;
                priceText.FontWeight = FontWeights.Bold;
                priceText.Margin = new Thickness(0, 0, 0, 5);
                stackPanel.Children.Add(priceText);

                // Невидимое свойство
                var invisibleText = new TextBlock();
                invisibleText.Text = $"Свойство: {product.InvisibleProperty}";
                invisibleText.FontSize = 10;
                invisibleText.Foreground = new SolidColorBrush(Colors.LightGray);
                invisibleText.Margin = new Thickness(0, 0, 0, 5);
                stackPanel.Children.Add(invisibleText);

                // Кнопки действий
                var buttonPanel = new StackPanel();
                buttonPanel.Orientation = Orientation.Horizontal;
                buttonPanel.HorizontalAlignment = HorizontalAlignment.Center;
                buttonPanel.Margin = new Thickness(0, 10, 0, 0);

                if (App.CurrentUserRole == "Администратор")
                {
                    var editButton = new Button();
                    editButton.Content = "Редактировать";
                    editButton.Width = 100;
                    editButton.Click += (s, ev) => EditProduct(product);
                    buttonPanel.Children.Add(editButton);

                    var deleteButton = new Button();
                    deleteButton.Content = "Удалить";
                    deleteButton.Width = 100;
                    deleteButton.Click += (s, ev) => DeleteProduct(product);
                    buttonPanel.Children.Add(deleteButton);
                }
                else if (App.CurrentUserRole == "Пользователь")
                {
                    var detailsButton = new Button();
                    detailsButton.Content = "Подробнее";
                    detailsButton.Width = 100;
                    detailsButton.Click += (s, ev) => ViewDetails(product);
                    buttonPanel.Children.Add(detailsButton);

                    var cartButton = new Button();
                    cartButton.Content = "В корзину";
                    cartButton.Width = 100;
                    cartButton.Click += (s, ev) => AddToCart(product);
                    buttonPanel.Children.Add(cartButton);
                }
                else if (App.CurrentUserRole == "Мастер иллюзий")
                {
                    var illusionButton = new Button();
                    illusionButton.Content = "Создать иллюзию";
                    illusionButton.Width = 200;
                    illusionButton.Click += (s, ev) => CreateIllusion(product);
                    buttonPanel.Children.Add(illusionButton);
                }

                stackPanel.Children.Add(buttonPanel);
                border.Child = stackPanel;

                return border;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при создании карточки товара: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private ImageSource CreatePlaceholderImage(int categoryId)
        {
            try
            {
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    context.DrawRectangle(new SolidColorBrush(Colors.LightGray), null,
                        new Rect(0, 0, 200, 150));

                    string text = "Товар";
                    if (categoryId == 1) text = "ВОЗДУХ";
                    else if (categoryId == 2) text = "ТИШИНА";
                    else if (categoryId == 3) text = "ВРЕМЯ";

                    var formattedText = new FormattedText(text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        16, new SolidColorBrush(Colors.White), 1);

                    context.DrawText(formattedText, new System.Windows.Point(100 - formattedText.Width / 2,
                        75 - formattedText.Height / 2));
                }

                var renderTarget = new RenderTargetBitmap(200, 150, 96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(drawingVisual);

                return renderTarget;
            }
            catch
            {
                return null;
            }
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilterComboBox == null || CategoryFilterComboBox.SelectedItem == null)
                return;

            var selectedItem = CategoryFilterComboBox.SelectedItem as ComboBoxItem;
            int? categoryId = null;

            if (selectedItem != null && selectedItem.Tag != null)
            {
                categoryId = (int)selectedItem.Tag;
            }

            string searchText = "";
            if (SearchTextBox != null)
            {
                searchText = SearchTextBox.Text ?? "";
            }

            LoadProducts(searchText, categoryId);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CategoryFilterComboBox == null || CategoryFilterComboBox.SelectedItem == null)
                return;

            var selectedItem = CategoryFilterComboBox.SelectedItem as ComboBoxItem;
            int? categoryId = null;

            if (selectedItem != null && selectedItem.Tag != null)
            {
                categoryId = (int)selectedItem.Tag;
            }

            string searchText = "";
            if (SearchTextBox != null)
            {
                searchText = SearchTextBox.Text ?? "";
            }

            LoadProducts(searchText, categoryId);
        }

        private void EditProduct(Products product)
        {
            ProductEditWindow editWindow = new ProductEditWindow(product);
            editWindow.ShowDialog();

            string searchText = SearchTextBox?.Text ?? "";
            LoadProducts(searchText, GetSelectedCategoryId());
        }

        private void DeleteProduct(Products product)
        {
            var result = System.Windows.MessageBox.Show($"Удалить товар '{product.Name}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var orderDetails = Connection.entities.OrderDetails
                        .Where(od => od.ProductId == product.Id).ToList();

                    foreach (var detail in orderDetails)
                    {
                        Connection.entities.OrderDetails.Remove(detail);
                    }

                    var history = Connection.entities.ProductHistory
                        .Where(ph => ph.ProductId == product.Id).ToList();

                    foreach (var record in history)
                    {
                        Connection.entities.ProductHistory.Remove(record);
                    }

                    var illusions = Connection.entities.Illusions
                        .Where(i => i.Description.Contains(product.Name)).ToList();

                    foreach (var illusion in illusions)
                    {
                        Connection.entities.Illusions.Remove(illusion);
                    }

                    Connection.entities.Products.Remove(product);
                    Connection.entities.SaveChanges();

                    string searchText = SearchTextBox?.Text ?? "";
                    LoadProducts(searchText, GetSelectedCategoryId());

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

        private void ViewDetails(Products product)
        {
            ProductDetailsWindow detailsWindow = new ProductDetailsWindow(product);
            detailsWindow.ShowDialog();
        }

        private void AddToCart(Products product)
        {
            CartHelper.AddToCart(product);
            System.Windows.MessageBox.Show($"Товар '{product.Name}' добавлен в корзину",
                "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreateIllusion(Products product)
        {
            IllusionCreateWindow illusionWindow = new IllusionCreateWindow(product);
            illusionWindow.ShowDialog();
        }

        private int? GetSelectedCategoryId()
        {
            if (CategoryFilterComboBox == null || CategoryFilterComboBox.SelectedItem == null)
                return null;

            var selectedItem = CategoryFilterComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Tag != null)
            {
                return (int)selectedItem.Tag;
            }
            return null;
        }
    }

    public static class CartHelper
    {
        public static List<CartItem> CartItems = new List<CartItem>();

        public static void AddToCart(Products product, int quantity = 1)
        {
            var existingItem = CartItems.FirstOrDefault(ci => ci.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                CartItems.Add(new CartItem { Product = product, Quantity = quantity });
            }
        }

        public static void ClearCart()
        {
            CartItems.Clear();
        }
    }

    public class CartItem
    {
        public Products Product { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Product.Price * Quantity;
    }
}