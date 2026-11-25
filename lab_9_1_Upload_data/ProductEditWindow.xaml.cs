using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace lab_9_1_Upload_data
{
    public partial class ProductEditWindow : Window
    {
        private ProductRepository repository;
        private int productId;

        public Product Result { get; set; }

        // Конструктор для добавления товара
        public ProductEditWindow()
        {
            InitializeComponent();

            repository = new ProductRepository(
                @"Server=(localdb)\MSSQLLocalDB;Database=ShopDB;Integrated Security=true;Encrypt=True;TrustServerCertificate=True");

            LoadCategories();
        }

        // Конструктор для редактирования товара
        public ProductEditWindow(Product product)
        {
            InitializeComponent();

            repository = new ProductRepository(
                @"Server=(localdb)\MSSQLLocalDB;Database=ShopDB;Integrated Security=true;Encrypt=True;TrustServerCertificate=True");

            LoadCategories();

            productId = product.Id;
            txtName.Text = product.Name;
            txtPrice.Text = product.Price.ToString();
            txtQuantity.Text = product.Quantity.ToString();
            cmbCategory.SelectedItem = product.Category;
        }

        // Загрузка категорий
        private void LoadCategories()
        {
            try
            {
                cmbCategory.ItemsSource = repository.GetCategories();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки категорий:\n" + ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Кнопка «Сохранить»
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            decimal price;
            int quantity;

            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show("Введите название товара.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество.");
                return;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию.");
                return;
            }

            Result = new Product();

            Result.Id = productId;
            Result.Name = txtName.Text.Trim();
            Result.Price = price;
            Result.Quantity = quantity;
            Result.Category = cmbCategory.SelectedItem.ToString();

            DialogResult = true;
        }
    }
}
