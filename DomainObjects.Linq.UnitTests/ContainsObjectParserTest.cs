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
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.UnitTests;
using Remotion.Data.DomainObjects.UnitTests;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  [Ignore ("TODO: Implement ContainsObjectParser")]
  public class ContainsObjectParserTest : ClientTransactionBaseTest
  {
    [Test]
    public void ParseContainsObject ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();

      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      Student item = new Student ();

      ParameterExpression parameter = Expression.Parameter (typeof (Order), "o");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Order).GetProperty ("ID"));
      
      OrderItem orderItem = OrderItem.GetObject (DomainObjectIDs.OrderItem1);
      
      ConstantExpression checkedExpression = Expression.Constant (orderItem);
      MemberExpression itemExpression = Expression.MakeMemberAccess (checkedExpression, typeof (Order).GetProperty("ID"));

      ObjectList<OrderItem> orderItems = new ObjectList<OrderItem>();

      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);
      
      MethodCallExpression methodCallExpression = Expression.Call (
          subQueryExpression,
          typeof (ObjectList<OrderItem>).GetMethod ("ContainsObject"),
          Expression.Constant (orderItem)
      );

      Console.WriteLine (methodCallExpression);
      

    }

    private QueryModel _queryModel;
    private OrderItem _orderItem1;
    private IQueryable<Order> _query;
    private MemberExpression _implicitSubQueryExpression;
    private ConstantExpression _containedItemExpression;
    private MethodCallExpression _containsObjectCallExpression;

    public override void SetUp ()
    {
      base.SetUp ();
      _orderItem1 = OrderItem.GetObject (DomainObjectIDs.OrderItem1);
      _query = GetQuery();
      _containsObjectCallExpression = (MethodCallExpression) new ExpressionTreeNavigator (_query.Expression).Arguments[1].Operand.Expression;
      _implicitSubQueryExpression = (MemberExpression) new ExpressionTreeNavigator (_query.Expression).Arguments[1].Operand.Object.Expression;
      _containedItemExpression = (ConstantExpression) new ExpressionTreeNavigator (_query.Expression).Arguments[1].Operand.Arguments[0].Expression;
      MainFromClause fromClause = new MainFromClause (Expression.Parameter (typeof (Order), "o"), Expression.Constant (null, typeof (Order)));
      _queryModel = ExpressionHelper.CreateQueryModel();
    }

    private IQueryable<Order> GetQuery ()
    {
      return from o in DataContext.Entity<Order>()
             where o.OrderItems.ContainsObject (_orderItem1)
             select o;
    }

    [Test]
    [Ignore]
    public void ContainsObjectParser_CreatesImplicitQuery ()
    {
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (_queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (_queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (_queryModel, resolver));

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();

      ContainsObjectParser parser = new ContainsObjectParser (_query.Expression, parserRegistry);
      BinaryCondition criterion = (BinaryCondition) parser.Parse (_containsObjectCallExpression, fieldCollection);

      Assert.That (criterion, Is.Not.Null);
      Assert.That (criterion.Kind, Is.EqualTo (BinaryCondition.ConditionKind.Contains));

      IValue rightSide = criterion.Right;
      Assert.That (rightSide, Is.EqualTo (new Constant (_orderItem1.ID)));
      
      //ImplicitSubQuery leftSide = criterion.Left as ImplicitSubQuery;
      //Assert.That (leftSide, Is.Not.Null);
      //Assert.That (leftSide.QueriedObject, Is.EqualTo (new Column (_queryModel.MainFromClause.GetFromSource (DatabaseInfo.Instance), "ID")));
      //Assert.That (leftSide.ForeignKey, Is.EqualTo ( ...));
      //Assert.That (leftSide.PrimaryKey, Is.EqualTo (new Column (_queryModel.SelectOrGroupClause.)));
    }

    //[Test]
    //public void name ()
    //{
    //  ParameterExpression fromClauseIdentifier = null;
    //  MemberExpression implicitSubQueryExpression = Expression.MakeMemberAccess (fromClauseIdentifier, typeof (Order).GetProperty ("OrderItems"));
    //  ClauseFieldResolver resolver = null;

    //  Expression queriedObject = implicitSubQueryExpression.Expression;
    //  ICriterion parsedQueriedObject = _parserRegistry.GetParser (queriedObject).Parse (queriedObject, fieldDescriptorCollection);
    //  ICriterion parsedSubQueryExpression = _parserRegistry.GetParser (implicitSubQueryExpression).Parse (
    //      implicitSubQueryExpression, fieldDescriptorCollection);

    //  ClassDefinition subQueryClassDefinition =
    //      MappingConfiguration.Current.ClassDefinitions.GetMandatory (implicitSubQueryExpression.Type.GetGenericArguments()[0]);
    //  Table subQueryTable = new Table (subQueryClassDefinition.GetEntityName(), "<generated Subquery>");
    //  Column subQueryPrimaryKey = new Column (subQueryTable, "ID");

    //  string statement = string.Format ("SELECT {0} FROM {1} WHERE {2} = {3}", subQueryPrimaryKey.Name, subQueryTable.Name, ((Column)parsedSubQueryExpression).Name, ((Column)parsedQueriedObject));

    //}

    public static bool ContainsObject () { return true; }

  }
}