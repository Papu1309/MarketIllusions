using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MarketIllusions.Connect;
using MarketIllusions.Pages;
using System.Data.Entity;

namespace MarketIllusions.Windows
{
    public partial class ProductDetailsWindow : Window
    {
        private Products product;

        public ProductDetailsWindow(Products product)
        {
            InitializeComponent();
            this.product = product;

            this.Loaded += ProductDetailsWindow_Loaded;
        }

        private void ProductDetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProductDetails();
            LoadHistory();
            LoadIllusions();
        }

        private void LoadProductDetails()
        {
            try
            {
                NameTextBlock.Text = product.Name;
                DescriptionTextBlock.Text = product.Description;
                PriceTextBlock.Text = $"{product.Price:C}";
                InvisiblePropertyTextBlock.Text = $"Невидимое свойство: {product.InvisibleProperty}";

                var category = Connection.entities.Categories
                    .FirstOrDefault(c => c.Id == product.CategoryId);
                CategoryTextBlock.Text = category?.Name ?? "Без категории";

                DetailsTextBlock.Text = $"Чистота: {product.PurityPercent}%\n" +
                                       $"Длительность: {product.DurationMinutes} мин\n" +
                                       $"Дата добавления: {product.CreatedDate:dd.MM.yyyy}";

                CreatePlaceholderImage();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке данных товара: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreatePlaceholderImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(product.ImagePath) &&
                    System.IO.File.Exists(product.ImagePath))
                {
                    ProductImage.Source = new BitmapImage(new Uri(product.ImagePath));
                }
                else
                {
                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        context.DrawRectangle(
                            new SolidColorBrush(Colors.LightGray),
                            null,
                            new Rect(0, 0, 200, 150));

                        string text = "ТОВАР";
                        if (product.CategoryId == 1) text = "ВОЗДУХ";
                        else if (product.CategoryId == 2) text = "ТИШИНА";
                        else if (product.CategoryId == 3) text = "ВРЕМЯ";

                        var formattedText = new FormattedText(
                            text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"),
                            16,
                            new SolidColorBrush(Colors.White),
                            1);

                        context.DrawText(formattedText, new System.Windows.Point(100 - formattedText.Width / 2, 75 - formattedText.Height / 2));
                    }

                    var renderTarget = new RenderTargetBitmap(200, 150, 96, 96, PixelFormats.Pbgra32);
                    renderTarget.Render(drawingVisual);
                    ProductImage.Source = renderTarget;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при создании изображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHistory()
        {
            try
            {
                var history = Connection.entities.ProductHistory
                    .Include("Users")
                    .Where(h => h.ProductId == product.Id)
                    .OrderByDescending(h => h.ChangeDate)
                    .ToList();

                if (history != null && history.Any())
                {
                    HistoryDataGrid.ItemsSource = history;
                }
                else
                {
                    HistoryDataGrid.ItemsSource = null;
                    System.Windows.MessageBox.Show("История изменений отсутствует",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadIllusions()
        {
            try
            {
                var illusions = Connection.entities.Illusions
                    .Where(i => i.Description.Contains(product.Name) ||
                               i.Description.Contains(product.InvisibleProperty))
                    .OrderByDescending(i => i.CreatedDate)
                    .ToList();

                if (illusions != null && illusions.Any())
                {
                    IllusionsDataGrid.ItemsSource = illusions;
                }
                else
                {
                    IllusionsDataGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке иллюзий: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CartHelper.AddToCart(product);
                System.Windows.MessageBox.Show($"Товар '{product.Name}' добавлен в корзину",
                    "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}