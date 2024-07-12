namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerNamespaceInAssembly
{
    using System.Data;
    using Dapper;
    using FluentDocker.SqlServer.Example.Models;

    /// <summary>
    /// <a href="https://www.learndapper.com/">Dapper</a>
    /// </summary>
    [TestFixture]
    internal class DapperTests : BaseTest
    {
        [Test, Order(1)]
        public void WhenICreateDb_ThenDbIsCreated()
        {
            var rowsAffected = this.SqlConnection.DbConnection.Execute("CREATE DATABASE myDapperDb;", CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(-1));
        }

        [Test, Order(2)]
        public void WhenISelectDb_ThenDbIsSelected()
        {
            var rowsAffected = this.SqlConnection.DbConnection.Execute("USE myDapperDb;", CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(-1));
        }

        [Test, Order(3)]
        public void WhenICreateTable_ThenTableIsCreated()
        {
            var command = @"CREATE TABLE Categories(
                CategoryID int IDENTITY(1,1) PRIMARY KEY,
                CategoryName varchar(255),
                Description varchar(255));";

            var rowsAffected = this.SqlConnection.DbConnection.Execute(command, CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(-1));
        }

        [Test, Order(4)]
        public void WhenIInsertData_ThenDataIsInserted()
        {
            var command = @"INSERT INTO Categories
                VALUES
                ('Beverages', 'Soft drinks, coffees, teas, beers, and ales'),
                ('Condiments', 'Sweet and savory sauces, relishes, spreads, and seasonings'),
                ('Confections', 'Desserts, candies, and sweet breads'),
                ('Dairy Products', 'Cheeses'),
                ('Grains/Cereals', 'Breads, crackers, pasta, and cereal'),
                ('Meat/Poultry', 'Prepared meats'),
                ('Produce', 'Dried fruit and bean curd'),
                ('Seafood', 'Seaweed and fish');";

            var rowsAffected = this.SqlConnection.DbConnection.Execute(command, CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(8));
        }

        [Test, Order(5)]
        public void WhenISelectUsingExecuteScalar_ThenTheFirstColumnOfTheFirstRowInResultSetIsReturned()
        {
            var command = "SELECT * FROM Categories WHERE CategoryID=6 OR CategoryID=7;";
            var oValue = this.SqlConnection.DbConnection.ExecuteScalar(command, CommandType.Text);
            Assert.That(oValue, Is.Not.Null);

            int resultId;
            if (int.TryParse(oValue.ToString(), out resultId))
            {
                Assert.That(resultId, Is.EqualTo(6));
            }
            else
            {
                Assert.Fail($"Did not return an int.\nMethod: (extension) IDbConnection.ExecuteScalar({command}, {CommandType.Text})");
            }
        }

        [Test, Order(6)]
        [TestCase(4, "Dairy Products", "Cheeses")]
        [TestCase(6, "Meat/Poultry", "Prepared meats")]
        public void WhenISelectUsingQuerySingleOrDefault_ThenObjectsIsReturned(int categoryId, string categoryName, string description)
        {
            var command = "SELECT TOP 1 * FROM Categories WHERE CategoryID = @CategoryID;";
            var category = this.SqlConnection.DbConnection.QuerySingleOrDefault<Category>(command, new { CategoryID = categoryId });

            Assert.That(category, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(category.CategoryID, Is.EqualTo(categoryId));
                Assert.That(category.CategoryName, Is.EqualTo(categoryName));
                Assert.That(category.Description, Is.EqualTo(description));
            });
        }

        [Test, Order(7)]
        public void WhenISelectTopThreeUsingQuery_ThenObjectsAreReturned()
        {
            var command = "SELECT TOP 3 * FROM Categories;";
            var categories = this.SqlConnection.DbConnection.Query<Category>(command);

            Assert.That(categories, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(categories.Count, Is.EqualTo(3));
                Assert.That(categories.ElementAt(0).CategoryID, Is.EqualTo(1));
                Assert.That(categories.ElementAt(0).CategoryName, Is.EqualTo("Beverages"));
                Assert.That(categories.ElementAt(0).Description, Is.EqualTo("Soft drinks, coffees, teas, beers, and ales"));
                Assert.That(categories.ElementAt(1).CategoryID, Is.EqualTo(2));
                Assert.That(categories.ElementAt(1).CategoryName, Is.EqualTo("Condiments"));
                Assert.That(categories.ElementAt(1).Description, Is.EqualTo("Sweet and savory sauces, relishes, spreads, and seasonings"));
                Assert.That(categories.ElementAt(2).CategoryID, Is.EqualTo(3));
                Assert.That(categories.ElementAt(2).CategoryName, Is.EqualTo("Confections"));
                Assert.That(categories.ElementAt(2).Description, Is.EqualTo("Desserts, candies, and sweet breads"));
            });
        }
    }
}
