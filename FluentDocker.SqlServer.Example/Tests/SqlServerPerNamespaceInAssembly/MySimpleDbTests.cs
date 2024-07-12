namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerNamespaceInAssembly
{
    using System.Data;

    [TestFixture]
    internal class MySimpleDbTests : BaseTest
    {
        [Test, Order(1)]
        public void WhenICreateDb_ThenDbIsCreated()
        {
            var rowsAffected = this.SqlConnection.ExecuteNonQuery("CREATE DATABASE mySimpleDb;", CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(-1));
        }

        [Test, Order(2)]
        public void WhenISelectDb_ThenDbIsSelected()
        {
            var rowsAffected = this.SqlConnection.ExecuteNonQuery("USE mySimpleDb;", CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(-1));
        }

        [Test, Order(3)]
        public void WhenICreateTable_ThenTableIsCreated()
        {
            var command = @"CREATE TABLE Customers(
                CustomerID int IDENTITY(1,1) PRIMARY KEY,
                CustomerName varchar(255) NOT NULL,
                ContactName varchar(255) NOT NULL,
                Address varchar(255) NOT NULL,
                City varchar(255) NOT NULL,
                PostalCode varchar(255) NOT NULL,
                Country varchar(255) NOT NULL);";

            var rowsAffected = this.SqlConnection.ExecuteNonQuery(command, CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(-1));
        }

        [Test, Order(4)]
        public void WhenIInsertData_ThenDataIsInserted()
        {
            var command = @"INSERT INTO Customers
                VALUES
                ('Alfreds Futterkiste', 'Maria Anders', 'Obere Str. 57', 'Berlin', '12209', 'Germany'),
                ('Ana Trujillo Emparedados y helados', 'Ana Trujillo', 'Avda. de la Constitución 2222', 'México D.F.', '05021', 'Mexico'),
                ('Antonio Moreno Taquería', 'Antonio Moreno', 'Mataderos 2312', 'México D.F.', '05023', 'Mexico'),
                ('Around the Horn', 'Thomas Hardy', '120 Hanover Sq.', 'London', 'WA1 1DP', 'UK'),
                ('Berglunds snabbköp', 'Christina Berglund', 'Berguvsvägen 8', 'Luleå', 'S-958 22', 'Sweden'),
                ('Blauer See Delikatessen', 'Hanna Moos', 'Forsterstr. 57', 'Mannheim', '68306', 'Germany'),
                ('Blondel père et fils', 'Frédérique Citeaux', '24, place Kléber', 'Strasbourg', '67000', 'France'),
                ('Bólido Comidas preparadas', 'Martín Sommer',  'C/ Araquil, 67', 'Madrid', '28023', 'Spain'),
                ('Cardinal', 'Tom B. Erichsen', 'Skagen 21', 'Stavanger', '4006', 'Norway');";

            var rowsAffected = this.SqlConnection.ExecuteNonQuery(command, CommandType.Text);
            Assert.That(rowsAffected, Is.EqualTo(9));
        }

        [Test, Order(5)]
        public void WhenISelectUsingExecuteScalar_ThenTheFirstColumnOfTheFirstRowInResultSetIsReturned()
        {
            var command = "SELECT * FROM Customers WHERE CustomerID=4 OR CustomerID=5;";
            var oValue = this.SqlConnection.ExecuteScalar(command, CommandType.Text);
            int resultId;
            if (int.TryParse(oValue.ToString(), out resultId))
            {
                Assert.That(resultId, Is.EqualTo(4));
            }
        }

        [Test, Order(6)]
        public void WhenISelectUsingExecuteReader_ThenTheReaderContainingTheResultSetIsReturned()
        {
            var command = "SELECT * FROM Customers WHERE CustomerID=4 OR CustomerID=5;";
            List<Tuple<int, string, string, string, string, string, string>> result = new List<Tuple<int, string, string, string, string, string, string>>();
            using (var reader = this.SqlConnection.ExecuteReader(command, CommandType.Text))
            {
                while (reader.Read())
                {
                    var id = (int)reader["CustomerID"];
                    var customerName = (string)reader["CustomerName"];
                    var contactName = (string)reader["ContactName"];
                    var address = (string)reader["Address"];
                    var city = (string)reader["City"];
                    var postalCode = (string)reader["PostalCode"];
                    var country = (string)reader["Country"];
                    result.Add(Tuple.Create(id, customerName, contactName, address, city, postalCode, country));
                }
            }

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Item1, Is.EqualTo(4));
            Assert.That(result[0].Item2, Is.EqualTo("Around the Horn"));
            Assert.That(result[0].Item3, Is.EqualTo("Thomas Hardy"));
            Assert.That(result[0].Item4, Is.EqualTo("120 Hanover Sq."));
            Assert.That(result[0].Item5, Is.EqualTo("London"));
            Assert.That(result[0].Item6, Is.EqualTo("WA1 1DP"));
            Assert.That(result[0].Item7, Is.EqualTo("UK"));
            Assert.That(result[1].Item1, Is.EqualTo(5));
            Assert.That(result[1].Item2, Is.EqualTo("Berglunds snabbköp"));
            Assert.That(result[1].Item3, Is.EqualTo("Christina Berglund"));
            Assert.That(result[1].Item4, Is.EqualTo("Berguvsvägen 8"));
            Assert.That(result[1].Item5, Is.EqualTo("Luleå"));
            Assert.That(result[1].Item6, Is.EqualTo("S-958 22"));
            Assert.That(result[1].Item7, Is.EqualTo("Sweden"));
        }
    }
}
