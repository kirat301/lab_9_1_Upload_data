using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab_9_1_Upload_data
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Product> _products;
        private ProductRepository repository;
        private const string connectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=ShopDB;Integrated Security=true;Encrypt=True;TrustServerCertificate=True";


        public MainWindow()
        {
            InitializeComponent();
            _products = new ObservableCollection<Product>();
            productDG.ItemsSource = _products;
            repository = new ProductRepository(connectionString);
            LoadProducts();
            LoadCategories();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
        public void LoadProducts()
        {
            try
            {
                
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // открыть соединение
                    string sql = @"SELECT p.Id, p.Name, p.Price, p.Quantity, c.Name AS Category
                                   FROM Products p
                                    LEFT JOIN Categories c ON p.CategoryId = c.Id
                                    ORDER BY p.Name;";
                    _products.Clear();

                    using (var command = new SqlCommand(sql, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // читаем строки по одной
                        {
                            _products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                Quantity = reader.GetInt32(3),
                                Category = reader.IsDBNull(4) ? "Без категории" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось подключиться к серверу БД.\n" +
                    "Проверьте, что SQL Server запущен.",
                    "Ошибка подключения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateStatusBar();
            }
            productDG.ItemsSource = _products;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new ProductEditWindow { Owner = this };

                if (dlg.ShowDialog() == true)
                {
                    repository.Add(dlg.Result);
                    LoadProducts();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Ошибка добавления товара:\n" + ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            var select = productDG.SelectedItem as Product;

            if (select == null)
            {
                MessageBox.Show(
                    "Выберите товар для редактирования.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dlg = new ProductEditWindow(select) { Owner = this };

                if (dlg.ShowDialog() == true)
                {
                    repository.Update(dlg.Result);
                    LoadProducts();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Ошибка изменения товара:\n" + ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var select = productDG.SelectedItem as Product;

            if (select == null)
            {
                MessageBox.Show(
                    "Выберите товар для удаления.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show(
                "Удалить товар " + select.Name + "?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                try
                {
                    repository.Delete(select.Id);
                    LoadProducts();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(
                        "Ошибка удаления товара:\n" + ex.Message,
                        "Ошибка базы данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.Trim();

                if (searchText == "")
                {
                    LoadProducts();
                    return;
                }

                List<Product> results = repository.Search(searchText);

                _products.Clear();

                foreach (Product product in results)
                {
                    _products.Add(product);
                }

                UpdateStatusBar();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Ошибка поиска товаров:\n" + ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadProducts();
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();

            cmbCategory.Items.Add("Все категории");

            List<string> categories = repository.GetCategories();

            foreach (string category in categories)
            {
                cmbCategory.Items.Add(category);
            }

            cmbCategory.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            decimal minPrice = 0;
            decimal maxPrice = 0;

            if (chkPrice.IsChecked == true)
            {
                if (!decimal.TryParse(txtPriceFrom.Text, out minPrice))
                {
                    MessageBox.Show("Введите корректную минимальную цену.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPriceFrom.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPriceTo.Text, out maxPrice))
                {
                    MessageBox.Show("Введите корректную максимальную цену.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPriceTo.Focus();
                    return;
                }

                if (minPrice > maxPrice)
                {
                    MessageBox.Show(
                        "Минимальная цена не может быть больше максимальной.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    txtPriceFrom.Focus();
                    return;
                }
            }

            string search = txtSearch.Text.Trim();
            string category = cmbCategory.SelectedItem?.ToString();

            var view = CollectionViewSource.GetDefaultView(_products);

            view.Filter = item =>
            {
                Product product = item as Product;

                bool nameMatch = product.Name.IndexOf(
                    search, StringComparison.OrdinalIgnoreCase) >= 0;

                bool priceMatch = chkPrice.IsChecked != true ||
                    (product.Price >= minPrice && product.Price <= maxPrice);

                bool categoryMatch = category == "Все категории" ||
                    string.IsNullOrEmpty(category) ||
                    product.Category == category;

                return nameMatch && priceMatch && categoryMatch;
            };

            UpdateStatusBar();
        }

        private void btnApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            chkPrice.IsChecked = false;
            txtPriceFrom.Clear();
            txtPriceTo.Clear();
            txtSearch.Clear();
            cmbCategory.SelectedIndex = 0;

            var view = CollectionViewSource.GetDefaultView(_products);
            view.Filter = null;
            view.Refresh();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_products == null)
                return;
            var view = CollectionViewSource.GetDefaultView(_products);

            view.SortDescriptions.Clear();

            switch (cmbSort.SelectedIndex)
            {
                case 0:
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    break;

                case 1:
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
                    break;

                case 2:
                    view.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                    break;

                case 3:
                    view.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                    break;

                case 4:
                    view.SortDescriptions.Add(new SortDescription("Quantity", ListSortDirection.Ascending));
                    break;

                case 5:
                    view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
                    break;
            }
        }

        private void UpdateStatusBar()
        {
            var view = CollectionViewSource.GetDefaultView(_products);

            int visible = view.Cast<Product>().Count();
            int total = _products.Count;

            decimal sum = view.Cast<Product>()
                .Sum(p => p.Price * p.Quantity);

            txtCount.Text = $"Товаров: {visible} из {total}";
            txtTotal.Text = $"Итоговая сумма: {sum:N2} руб.";
        }
    }
}
