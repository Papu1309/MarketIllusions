using System;
using System.Linq;
using System.Windows;
using MarketIllusions.Connect;

namespace MarketIllusions.Windows
{
    public partial class ProductEditWindow : Window
    {
        private Products product;
        private bool isNew;

        public ProductEditWindow(Products product = null)
        {
            InitializeComponent();

            LoadCategories();

            if (product == null)
            {
                this.product = new Products();
                isNew = true;
                this.Title = "Добавление товара";
            }
            else
            {
                this.product = product;
                isNew = false;
                LoadProductData();
            }
        }

        private void LoadCategories()
        {
            var categories = Connection.entities.Categories.ToList();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "Name";
            CategoryComboBox.SelectedValuePath = "Id";
        }

        private void LoadProductData()
        {
            NameTextBox.Text = product.Name;
            DescriptionTextBox.Text = product.Description;
            PriceTextBox.Text = product.Price.ToString();
            CategoryComboBox.SelectedValue = product.CategoryId;
            InvisiblePropertyTextBox.Text = product.InvisibleProperty;
            PurityTextBox.Text = product.PurityPercent.ToString();
            DurationTextBox.Text = product.DurationMinutes.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    MessageBox.Show("Введите название товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                product.Name = NameTextBox.Text;
                product.Description = DescriptionTextBox.Text;

                if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
                {
                    MessageBox.Show("Некорректная цена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                product.Price = price;

                product.CategoryId = (int)CategoryComboBox.SelectedValue;
                product.InvisibleProperty = InvisiblePropertyTextBox.Text;

                if (int.TryParse(PurityTextBox.Text, out int purity))
                    product.PurityPercent = purity;

                if (int.TryParse(DurationTextBox.Text, out int duration))
                    product.DurationMinutes = duration;

                product.Quantity = 0; // Склад всегда пуст

                if (isNew)
                {
                    product.CreatedDate = DateTime.Now;
                    Connection.entities.Products.Add(product);
                }
                else
                {
                    // Записываем в историю изменение цены
                    var oldProduct = Connection.entities.Products
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Id == product.Id);

                    if (oldProduct != null && oldProduct.Price != product.Price)
                    {
                        var history = new ProductHistory
                        {
                            ProductId = product.Id,
                            UserId = App.CurrentUserId,
                            OldPrice = oldProduct.Price,
                            NewPrice = product.Price,
                            Action = "Изменение цены",
                            ChangeDate = DateTime.Now
                        };
                        Connection.entities.ProductHistory.Add(history);
                    }
                }

                Connection.entities.SaveChanges();

                MessageBox.Show("Товар сохранен", "Успех",
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