namespace FluentDocker.SqlServer.Example.Tests.SqlServerPerNamespaceInAssembly
{
    using System.Data;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// <a href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqlcommand?view=sqlclient-dotnet-standard-5.2">SqlCommand Class</a>
    /// </summary>
    [TestFixture]
    internal class MySchoolDbTests : BaseTest
    {
        private const string ScriptRelativePath = @"Resources/MySchool.sql";

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            var scriptPath = GetScriptPath();
            var scriptSuccess = this.SqlConnection.ExecuteScript(scriptPath);
            if (!scriptSuccess)
            {
                Assert.Fail($"SQL file error.\nFile: {scriptPath}");
            }
        }

        [Test]
        [TestCase(2012, "There are 4 courses in 2012.")]
        public void WhenIGetCoursesOfSpecifiedYear_ThenTheCountIsReturned(int year, string expectedResult)
        {
            string command = "Select Count([CourseID]) FROM [MySchool].[dbo].[Course] Where Year=@Year";
            SqlParameter parameterYear = new SqlParameter("@Year", SqlDbType.Int);
            parameterYear.Value = year;
            var oValue = this.SqlConnection.ExecuteScalar(command, CommandType.Text, parameterYear);

            int count;
            string? result = null;
            if (int.TryParse(oValue.ToString(), out count))
            {
                result = string.Format("There {0} {1} course{2} in {3}.", count > 1 ? "are" : "is", count, count > 1 ? "s" : null, year);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(2007, 920024.0000)]
        public void WhenIExecuteStoredProcedureGetDepartmentsOfSpecifiedYear_ThenTheBudgetSumIsReturned(int year, decimal expectedBudgetSum)
        {
            string command = "dbo.GetDepartmentsOfSpecifiedYear";

            SqlParameter parameterYear = new SqlParameter("@Year", SqlDbType.Int);
            parameterYear.Value = year;
            SqlParameter parameterBudget = new SqlParameter("@BudgetSum", SqlDbType.Money);
            parameterBudget.Direction = ParameterDirection.Output;

            using (var reader = this.SqlConnection.ExecuteReader(command, CommandType.StoredProcedure, parameterYear, parameterBudget))
            {
                Debug.WriteLine("{0,-20}{1,-20}{2,-20}{3,-20}", "Name", "Budget", "StartDate", "Administrator");
                while (reader.Read())
                {
                    Debug.WriteLine("{0,-20}{1,-20:C}{2,-20:d}{3,-20}", reader["Name"], reader["Budget"], reader["StartDate"], reader["Administrator"]);
                }
            }

            Debug.WriteLine("{0,-20}{1,-20:C}", "Sum:", parameterBudget.Value);

            Assert.That(parameterBudget.Value, Is.EqualTo(expectedBudgetSum));
        }

        [Test]
        [TestCase(4, "1 row is updated.")]
        public void WhenIUpdateCoursesWithCreditsBelowSpecifiedValue_ThenTheNumberOfRowsAffectedIsReturned(int creditsLow, string expectedResult)
        {
            string command = "Update [MySchool].[dbo].[Course] Set Credits=Credits+1 Where Credits<@Credits";
            SqlParameter parameterCredits = new SqlParameter("@Credits", creditsLow);
            int rows = this.SqlConnection.ExecuteNonQuery(command, CommandType.Text, parameterCredits);
            var result = string.Format("{0} row{1} {2} updated.", rows, rows > 1 ? "s" : null, rows > 1 ? "are" : "is");
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static string GetScriptPath()
        {
            var windowsPattern = @"(\\bin\\(Debug|Release)(\\[a-zA-Z0-9.]*)$)";
            var linuxPattern = @"(/bin/(Debug|Release)(/[a-zA-Z0-9.]*)$)";
            var pathRegex = new Regex($"{windowsPattern}|{linuxPattern}", RegexOptions.Compiled);
            var projectPath = pathRegex.Replace(Directory.GetCurrentDirectory(), string.Empty);
            string scriptAbsolutePath = Path.Combine(projectPath, ScriptRelativePath);
            return scriptAbsolutePath;
        }
    }
}
