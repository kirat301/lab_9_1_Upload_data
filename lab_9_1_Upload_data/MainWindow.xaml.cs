using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Data.SqlClient;

namespace lab_9_1_Upload_data
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Product> _products;

        public MainWindow()
        {
            InitializeComponent();
            _products = new ObservableCollection<Product>();
            productDG.ItemsSource = _products;
            LoadProducts();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

        }
        public void LoadProducts()
        {
            try
            {
                string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=ShopDB;Integrated Security=true;Encrypt=True;TrustServerCertificate=True";
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
                txtStatus.Text = $"Загруженно записей: {_products.Count}";
            }
            productDG.ItemsSource = _products;
        }
    }
}
