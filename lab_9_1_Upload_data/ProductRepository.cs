
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace lab_9_1_Upload_data
{
    public class ProductRepository
    {
        private readonly string connectionString;

        public ProductRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Получение всех товаров
        public List<Product> GetAll()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT p.Id, p.Name, p.Price, p.Quantity, c.Name AS Category
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryId = c.Id";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Product product = new Product();

                    product.Id = Convert.ToInt32(reader["Id"]);
                    product.Name = reader["Name"].ToString();
                    product.Price = Convert.ToDecimal(reader["Price"]);
                    product.Quantity = Convert.ToInt32(reader["Quantity"]);
                    if (reader["Category"] == DBNull.Value) { product.Category = "";}
                    else { product.Category = reader["Category"].ToString(); }

                    products.Add(product);
                }
            }

            return products;
        }

        // Добавление товара
        public void Add(Product p)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO Products (Name, Price, Quantity, CategoryId)
                    VALUES (@Name, @Price, @Quantity,
                        (SELECT Id FROM Categories WHERE Name = @Category))";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Name", p.Name);
                command.Parameters.AddWithValue("@Price", p.Price);
                command.Parameters.AddWithValue("@Quantity", p.Quantity);
                command.Parameters.AddWithValue("@Category", p.Category);

                command.ExecuteNonQuery();
            }
        }

        // Изменение товара
        public void Update(Product p)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    UPDATE Products
                    SET Name = @Name,
                        Price = @Price,
                        Quantity = @Quantity,
                        CategoryId = (
                            SELECT Id FROM Categories WHERE Name = @Category
                        )
                    WHERE Id = @Id";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", p.Id);
                command.Parameters.AddWithValue("@Name", p.Name);
                command.Parameters.AddWithValue("@Price", p.Price);
                command.Parameters.AddWithValue("@Quantity", p.Quantity);
                command.Parameters.AddWithValue("@Category", p.Category);

                command.ExecuteNonQuery();
            }
        }

        // Удаление товара
        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Products WHERE Id = @Id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                command.ExecuteNonQuery();
            }
        }

        // Получение списка категорий
        public List<string> GetCategories()
        {
            List<string> categories = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Name FROM Categories ORDER BY Name";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(reader["Name"].ToString());
                }
            }

            return categories;
        }
    }
}
