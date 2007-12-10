using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Collections;

namespace Rubicon.Data.Linq.UnitTests
{
  [TestFixture]
  public class SqlGeneratorTest
  {
    private IDbConnection _connection;
    private IDatabaseInfo _databaseInfo;
    private IQueryable<Student> _source;

    [SetUp]
    public void SetUp()
    {
      MockRepository repository = new MockRepository();
      _connection = repository.CreateMock<IDbConnection>();

      IDataParameterCollection parameterCollection = new StubParameterCollection();
      
      IDbCommand command = repository.Stub<IDbCommand>();
      SetupResult.For (command.Parameters).Return (parameterCollection);

      Expect.Call (_connection.CreateCommand()).Return (command);
      repository.ReplayAll();

      _databaseInfo = new StubDatabaseInfo();

      _source = ExpressionHelper.CreateQuerySource();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void SimpleQuery_WithNonDBFieldProjection ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQueryWithNonDBProjection (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      new SqlGenerator (parsedQuery).GetCommandString(_databaseInfo);
    }

    [Test]
    public void SimpleQuery()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (_source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator(parsedQuery);
      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s]", sqlGenerator.GetCommandString(_databaseInfo));
      
      IDbCommand command = sqlGenerator.GetCommand (_databaseInfo, _connection);

      Assert.AreEqual ("SELECT [s].* FROM [sourceTable] [s]", command.CommandText);
      Assert.AreEqual (CommandType.Text, command.CommandType);
      Assert.IsEmpty (command.Parameters);
    }

    [Test]
    public void MultiFromQueryWithProjection ()
    {
      IQueryable<Tuple<string, string, int>> query = TestQueryGenerator.CreateMultiFromQueryWithProjection (_source, _source, _source);
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery (query);
      SqlGenerator sqlGenerator = new SqlGenerator (parsedQuery);
      Assert.AreEqual ("SELECT [s1].[FirstColumn], [s2].[LastColumn], [s3].[IDColumn] FROM [sourceTable] [s1], [sourceTable] [s2], [sourceTable] [s3]",
        sqlGenerator.GetCommandString(_databaseInfo));

      IDbCommand command = sqlGenerator.GetCommand (_databaseInfo, _connection);

      Assert.AreEqual ("SELECT [s1].[FirstColumn], [s2].[LastColumn], [s3].[IDColumn] FROM [sourceTable] [s1], [sourceTable] [s2], [sourceTable] [s3]",
        command.CommandText);
      Assert.AreEqual (CommandType.Text, command.CommandType);
      Assert.IsEmpty (command.Parameters);
    }
  }
}