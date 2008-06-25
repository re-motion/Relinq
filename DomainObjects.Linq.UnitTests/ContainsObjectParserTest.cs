using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.UnitTests;
using Remotion.Data.DomainObjects.UnitTests;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.UnitTests.ParsingTest;
using Rhino.Mocks;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class ContainsObjectParserTest : ClientTransactionBaseTest
  {
    //private QueryModel _queryModel;
    private OrderItem _orderItem1;
    private IQueryable<Order> _query;
    private MethodCallExpression _containsObjectCallExpression;
    private ContainsObjectParser _parser;
    private ParameterExpression _queriedObjectExpression;

    private MockRepository _mockRepository;
    private WhereConditionParserRegistry _registryStub;
    private IWhereConditionParser _containsParserMock;

    public override void SetUp ()
    {
      base.SetUp ();

      _mockRepository = new MockRepository ();
      _registryStub = 
        _mockRepository.Stub<WhereConditionParserRegistry>(ExpressionHelper.CreateQueryModel(),StubDatabaseInfo.Instance,new JoinedTableContext());
      _containsParserMock = _mockRepository.CreateMock<IWhereConditionParser> ();

      _orderItem1 = OrderItem.GetObject (DomainObjectIDs.OrderItem1);
      _query = GetQuery();
      _containsObjectCallExpression = (MethodCallExpression) new ExpressionTreeNavigator (_query.Expression).Arguments[1].Operand.Body.Expression;
      _queriedObjectExpression =
          (ParameterExpression) new ExpressionTreeNavigator (_containsObjectCallExpression).Object.MemberAccess_Expression.Expression;

      _parser = new ContainsObjectParser (_registryStub);
    }

    [Test]
    public void CreateFromClause_Identifier ()
    {
      MainFromClause fromClause = _parser.CreateFromClause (typeof (OrderItem));
      Assert.That (fromClause.Identifier.Type, Is.EqualTo (typeof (OrderItem)));
      Assert.That (fromClause.Identifier.Name, NUnit.Framework.SyntaxHelpers.Text.StartsWith ("<<generated>>"));
    }

    [Test]
    public void CreateFromClause_QuerySource ()
    {
      MainFromClause fromClause = _parser.CreateFromClause (typeof (OrderItem));
      Assert.That (fromClause.QuerySource, Is.InstanceOfType (typeof (ConstantExpression)));
      object value = ((ConstantExpression) fromClause.QuerySource).Value;
      Assert.That (value, Is.InstanceOfType (typeof (DomainObjectQueryable<OrderItem>)));
    }

    [Test]
    public void CreateWhereComparison_CreatesBinaryEqualsExpression ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (OrderItem), "oi");
      PropertyInfo foreignKeyProperty = typeof (OrderItem).GetProperty ("Order");
      ParameterExpression queriedObject = Expression.Parameter (typeof (Order), "o");
      LambdaExpression whereComparison = _parser.CreateWhereComparison (identifier, foreignKeyProperty, queriedObject);

      Assert.That (whereComparison.Body, Is.InstanceOfType (typeof (BinaryExpression)));
      BinaryExpression boolExpression = (BinaryExpression) whereComparison.Body;
      Assert.That (boolExpression.NodeType, Is.EqualTo (ExpressionType.Equal));
    }

    [Test]
    public void CreateWhereComparison_CreatesBinaryEqualsExpression_LeftSide ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (OrderItem), "oi");
      PropertyInfo foreignKeyProperty = typeof (OrderItem).GetProperty ("Order");
      ParameterExpression queriedObject = Expression.Parameter (typeof (Order), "o");
      LambdaExpression whereComparison = _parser.CreateWhereComparison (identifier, foreignKeyProperty, queriedObject);
      BinaryExpression boolExpression = (BinaryExpression) whereComparison.Body;
      
      Assert.That (boolExpression.Left, Is.InstanceOfType (typeof (MemberExpression)));
      MemberExpression memberExpression = (MemberExpression) boolExpression.Left;
      Assert.That (memberExpression.Expression, Is.SameAs (identifier));
      Assert.That (memberExpression.Member, Is.SameAs (foreignKeyProperty));
    }

    [Test]
    public void CreateWhereComparison_CreatesBinaryEqualsExpression_RightSide ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (OrderItem), "oi");
      PropertyInfo foreignKeyProperty = typeof (OrderItem).GetProperty ("Order");
      ParameterExpression queriedObject = Expression.Parameter (typeof (Order), "o");
      LambdaExpression whereComparison = _parser.CreateWhereComparison (identifier, foreignKeyProperty, queriedObject);
      BinaryExpression boolExpression = (BinaryExpression) whereComparison.Body;

      Assert.That (boolExpression.Right, Is.SameAs (queriedObject));
    }

    [Test]
    public void CreateWhereClause ()
    {
      MainFromClause mainFromClause = _parser.CreateFromClause (typeof (OrderItem));
      PropertyInfo foreignKeyProperty = typeof (OrderItem).GetProperty ("Order");
      ParameterExpression queriedObject = Expression.Parameter (typeof (Order), "o");
      LambdaExpression expectedWhereComparison = _parser.CreateWhereComparison (mainFromClause.Identifier, foreignKeyProperty, queriedObject);

      WhereClause whereClause = _parser.CreateWhereClause (mainFromClause, foreignKeyProperty, queriedObject);
      Assert.That (whereClause.PreviousClause, Is.SameAs (mainFromClause));
      ExpressionTreeComparer.CheckAreEqualTrees (whereClause.BoolExpression, expectedWhereComparison);
    }

    [Test]
    public void CreateSelectClause ()
    {
      MainFromClause mainFromClause = _parser.CreateFromClause (typeof (OrderItem));
      PropertyInfo foreignKeyProperty = typeof (OrderItem).GetProperty ("Order");
      ParameterExpression queriedObject = Expression.Parameter (typeof (Order), "o");
      WhereClause whereClause = _parser.CreateWhereClause (mainFromClause, foreignKeyProperty, queriedObject);

      SelectClause selectClause = _parser.CreateSelectClause (whereClause, mainFromClause.Identifier);
      Assert.That (selectClause.PreviousClause, Is.SameAs (whereClause));
      Assert.That (selectClause.ProjectionExpression.Body, Is.SameAs(mainFromClause.Identifier));
    }

    [Test]
    public void GetForeignKeyProperty ()
    {
      PropertyInfo collectionProperty = typeof (Order).GetProperty ("OrderItems");
      PropertyInfo expectedForeignKey = typeof (OrderItem).GetProperty ("Order");
      Assert.That (_parser.GetForeignKeyProperty (collectionProperty), Is.EqualTo (expectedForeignKey));
    }

    [Test]
    public void CreateQueryModel_Clauses ()
    {
      QueryModel queryModel = _parser.CreateQueryModel (_containsObjectCallExpression);

      MainFromClause fromClause = queryModel.MainFromClause;
      Assert.That (fromClause.Identifier.Type, Is.EqualTo (typeof (OrderItem)));
      Assert.That (fromClause.Identifier.Name, NUnit.Framework.SyntaxHelpers.Text.StartsWith ("<<generated>>"));

      WhereClause whereClause = (WhereClause) queryModel.BodyClauses[0];
      Assert.That (whereClause.BoolExpression.Body, Is.InstanceOfType (typeof (BinaryExpression)));
      BinaryExpression binaryExpression = (BinaryExpression) whereClause.BoolExpression.Body;
      Assert.That (binaryExpression.Left, Is.InstanceOfType (typeof (MemberExpression)));
      MemberExpression memberExpression = (MemberExpression) binaryExpression.Left;
      Assert.That (memberExpression.Expression, Is.SameAs (fromClause.Identifier));
      Assert.That (memberExpression.Member, Is.EqualTo (typeof (OrderItem).GetProperty ("Order")));
      Assert.That (binaryExpression.Right, Is.SameAs (_queriedObjectExpression));

      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (selectClause.ProjectionExpression.Body, Is.EqualTo(fromClause.Identifier));
    }

    [Test]
    public void CreateQueryModel_ResultType ()
    {
      QueryModel queryModel = _parser.CreateQueryModel (_containsObjectCallExpression);
      Assert.That (queryModel.ResultType, Is.EqualTo (typeof (IQueryable<OrderItem>)));
    }

    [Test]
    [Ignore ("TODO: Implement adding of parentQuery")]
    public void CreateQueryModel_ParentQuery ()
    {
      QueryModel parentQuery = ExpressionHelper.CreateQueryModel();
      QueryModel queryModel = _parser.CreateQueryModel (_containsObjectCallExpression);
      Assert.That (queryModel.ParentQuery, Is.SameAs (parentQuery));
    }

    [Test]
    public void CreateEqualSubQuery_CreatesSubQuery_WithQueryModel ()
    {
      SubQueryExpression subQuery = _parser.CreateEqualSubQuery (_containsObjectCallExpression);
      Assert.That (subQuery.QueryModel, Is.Not.Null);
    }

    [Test]
    public void CreateExpressionForContainsParser ()
    {
      SubQueryExpression subQueryExpression1 = _parser.CreateEqualSubQuery (_containsObjectCallExpression);
      Expression queryParameterExpression = Expression.Constant (null, typeof (OrderItem));
      MethodCallExpression methodCallExpression = _parser.CreateExpressionForContainsParser (subQueryExpression1, queryParameterExpression);
      Assert.That (methodCallExpression.Object, Is.Null);
      MethodInfo containsMethod = GetContainsMethod();
      Assert.That (methodCallExpression.Method, Is.EqualTo (containsMethod));
      Assert.That (methodCallExpression.Arguments, Is.EqualTo (new[] { subQueryExpression1, queryParameterExpression }));
    }

    private MethodInfo GetContainsMethod ()
    {
      return ParserUtility.GetMethod (() => (from oi in DataContext.Entity<OrderItem> () select oi).Contains (_orderItem1));
    }

    [Test]
    public void CanParse ()
    {
      Assert.That (_parser.CanParse (_containsObjectCallExpression), Is.True);
      Assert.That (_parser.CanParse (Expression.Constant (0)), Is.False);
      Assert.That (_parser.CanParse (Expression.Call (typeof (DateTime).GetMethod ("get_Now"))), Is.False);
    }

    [Test]
    public void Parse ()
    {
      List<FieldDescriptor> expectedFieldDescriptors = new List<FieldDescriptor>();
      ICriterion expectedResult = new Constant (false);

      SetupResult.For (_registryStub.GetParser (null)).IgnoreArguments().Return (_containsParserMock);
      Func<Expression, List<FieldDescriptor>, ICriterion> action = (expression, fieldDescriptors) =>
      {
        Assert.That (expression, Is.InstanceOfType (typeof (MethodCallExpression)));
        Assert.That (((MethodCallExpression) expression).Method, Is.EqualTo (GetContainsMethod()));
        Assert.That (fieldDescriptors, Is.SameAs (expectedFieldDescriptors));
        return expectedResult;
      };
      Expect.Call (_containsParserMock.Parse (null, null))
        .IgnoreArguments()
        .Do (action);

      _mockRepository.ReplayAll ();
      ICriterion result = _parser.Parse (_containsObjectCallExpression, expectedFieldDescriptors);
      Assert.That (result, Is.SameAs (expectedResult));
      _mockRepository.VerifyAll ();
    }

    private IQueryable<Order> GetQuery ()
    {
      return from o in DataContext.Entity<Order> ()
             where o.OrderItems.ContainsObject (_orderItem1)
             select o;
    }
  }
}