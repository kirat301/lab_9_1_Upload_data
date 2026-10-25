CREATE DATABASE ShopDB;
USE ShopDB;
CREATE TABLE Categories (
Id INT PRIMARY KEY IDENTITY(1,1),
Name NVARCHAR(100) NOT NULL UNIQUE
);
CREATE TABLE Products (
Id INT PRIMARY KEY IDENTITY(1,1),
Name NVARCHAR(200) NOT NULL,
Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
Quantity INT NOT NULL DEFAULT 0 CHECK (Quantity >= 0),
CategoryId INT FOREIGN KEY REFERENCES Categories(Id),
Description NVARCHAR(500) NULL
);
INSERT INTO Categories (Name) VALUES
('Электроника'), ('Бытовая техника'), ('Книги'), ('Одежда');
INSERT INTO Products (Name, Price, Quantity, CategoryId) VALUES
('Ноутбук Lenovo', 49999.00, 10, 1),
('Смартфон Samsung', 34990.00, 5, 1),
('Наушники Sony', 5499.00, 20, 1),
('Холодильник Bosch', 54900.00, 3, 2),
('Стиральная машина LG', 42000.00, 4, 2),
('Микроволновая печь', 8990.00, 15, 2),
('Чистый код (книга)', 1200.00, 50, 3),
('Паттерны проектирования', 890.00, 30, 3),
('Куртка зимняя', 7500.00, 25, 4),
('Джинсы классические', 3200.00, 40, 4);