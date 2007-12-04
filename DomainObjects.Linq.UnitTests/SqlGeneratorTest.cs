using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  [Ignore]
  public class SqlGeneratorTest
  {
    private IDbConnection _connection;
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp()
    {
      MockRepository repository = new MockRepository();
      _connection = repository.CreateMock<IDbConnection>();

      Expect.Call (_connection.CreateCommand()).Return (repository.Stub<IDbCommand>());

      _databaseInfo = new StubDatabaseInfo();
    }

    [Test]
    public void SimpleQuery()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource());
      QueryExpression parsedQuery = ExpressionHelper.ParseQuery<Student> (query);
      SqlGenerator sqlGenerator = new SqlGenerator(parsedQuery);
      IDbCommand command = sqlGenerator.GetCommand (_databaseInfo, _connection);

      Assert.AreEqual ("SELECT * FROM sourceTable", command.CommandText);
      Assert.AreEqual (CommandType.Text, command.CommandType);
      Assert.IsEmpty (command.Parameters);


    }
  }
}