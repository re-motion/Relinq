// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class QueryParserIntegrationTest
  {
    private IQueryable<Student> _querySource;
    private QueryParser _queryParser;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource();
      _queryParser = new QueryParser();
    }

    [Test]
    public void SimpleSelect ()
    {
      var expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (_querySource);
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (navigator.Arguments[1].Operand.Expression, Is.SameAs (selectClause.Selector));
    }

    [Test]
    public void SimpleWhere ()
    {
      var expression = WhereTestQueryGenerator.CreateSimpleWhereQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      Assert.That (queryModel.MainFromClause.Identifier.Name, Is.EqualTo ("s"));
      Assert.That (queryModel.MainFromClause.JoinClauses, Is.Empty);
      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.EqualTo (whereClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (navigator.Arguments[1].Operand.Expression, Is.SameAs (whereClause.Predicate));

      var parameterExpression = Expression.Parameter (typeof (Student), "s");
      var expectedSelectExpression = Expression.Lambda (typeof (Func<Student, Student>), parameterExpression, parameterExpression);
      Assert.That (expectedSelectExpression.Body.NodeType, Is.EqualTo (selectClause.Selector.Body.NodeType));
      Assert.That (expectedSelectExpression.Body.Type, Is.EqualTo (selectClause.Selector.Body.Type));
    }

    [Test]
    public void ThreeWheres ()
    {
      var expression = WhereTestQueryGenerator.CreateMultiWhereQuery (_querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause1 = (WhereClause) queryModel.BodyClauses[0];
      var whereClause2 = (WhereClause) queryModel.BodyClauses[1];
      var whereClause3 = (WhereClause) queryModel.BodyClauses[2];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (3));
      Assert.That (whereClause1.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (whereClause2.PreviousClause, Is.EqualTo (whereClause1));
      Assert.That (selectClause.PreviousClause, Is.EqualTo (whereClause3));
      Assert.That (whereClause3.PreviousClause, Is.EqualTo (whereClause2));

      var whereMethodCall1 = navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Expression;
      var whereMethodCall2 = navigator.Arguments[0].Arguments[1].Operand.Expression;
      var whereMethodCall3 = navigator.Arguments[1].Operand.Expression;

      Assert.That (whereClause1.Predicate, Is.SameAs (whereMethodCall1));
      Assert.That (whereClause2.Predicate, Is.SameAs (whereMethodCall2));
      Assert.That (whereClause3.Predicate, Is.SameAs (whereMethodCall3));

      var parameterExpression = Expression.Parameter (typeof (Student), "s");
      var expectedSelectExpression = Expression.Lambda (typeof (Func<Student, Student>), parameterExpression, parameterExpression);
      
      Assert.That (expectedSelectExpression.Body.NodeType, Is.EqualTo (selectClause.Selector.Body.NodeType));
      Assert.That (expectedSelectExpression.Body.Type, Is.EqualTo (selectClause.Selector.Body.Type));
    }

    [Test]
    public void WhereWithDifferentComparisons ()
    {
      var expression = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_querySource).Expression;
      var navigator = new ExpressionTreeNavigator (expression);
      var queryModel = _queryParser.GetParsedQuery (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.EqualTo (whereClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (whereClause.Predicate, Is.SameAs (navigator.Arguments[1].Operand.Expression));

      var parameterExpression = Expression.Parameter (typeof (Student), "s");
      var expectedSelectExpression = Expression.Lambda (typeof (Func<Student, Student>), parameterExpression, parameterExpression);

      Assert.That (expectedSelectExpression.Body.NodeType, Is.EqualTo (selectClause.Selector.Body.NodeType));
      Assert.That (expectedSelectExpression.Body.Type, Is.EqualTo (selectClause.Selector.Body.Type));

      Assert.That (navigator.Arguments[0].Expression.NodeType, Is.EqualTo (queryModel.MainFromClause.QuerySource.NodeType));
      Assert.That (navigator.Arguments[0].Expression.Type, Is.EqualTo (queryModel.MainFromClause.QuerySource.Type));
    }

    [Test]
    public void MultiFromsAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereQuery (_querySource, _querySource).Expression;
      var navigator = new ExpressionTreeNavigator (expression);
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var additionalFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      var whereClause = (WhereClause) queryModel.BodyClauses[1];

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (additionalFromClause.PreviousClause, Is.EqualTo (mainFromClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (additionalFromClause));

      var selectMethodCall = navigator;
      var whereMethodCall = selectMethodCall.Arguments[0];
      var selectManyMethodCall = whereMethodCall.Arguments[0];
      var constantExpression = selectManyMethodCall.Arguments[0];

      Assert.That (additionalFromClause.FromExpression, Is.SameAs (selectManyMethodCall.Arguments[1].Operand.Expression));
      Assert.That (whereClause.Predicate, Is.SameAs (whereMethodCall.Arguments[1].Operand.Expression));
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (selectManyMethodCall.Arguments[0].Expression.Type));
    }

    [Test]
    public void GeneralSelectMany ()
    {
      var expression = FromTestQueryGenerator.CreateMultiFromQuery (_querySource, _querySource).Expression;
      var navigator = new ExpressionTreeNavigator (expression);
      var queryModel = _queryParser.GetParsedQuery (expression);

      var mainFromClause = queryModel.MainFromClause;
      var memberFromClause = (MemberFromClause) queryModel.BodyClauses[0];

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      
      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (queryModel.BodyClauses[0], Is.InstanceOfType (typeof (MemberFromClause)));
      Assert.That (selectClause.PreviousClause, Is.EqualTo (memberFromClause));
      Assert.That (memberFromClause.Identifier.Name, Is.EqualTo ("s2"));

      var parameterExpression = Expression.Parameter (typeof (Student), "<generated>_0");
      var expectedSelectExpression = Expression.Lambda (typeof (Func<Student,Student>), parameterExpression, parameterExpression);
      Assert.That (expectedSelectExpression.Body.NodeType, Is.EqualTo (selectClause.Selector.Body.NodeType));
      Assert.That (expectedSelectExpression.Body.Type, Is.EqualTo (selectClause.Selector.Body.Type));
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo(navigator.Arguments[0].Expression.Type));

    }

    [Test]
    public void WhereSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateReverseFromWhere_WhereExpression (_querySource, _querySource);
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var mainFromClause = queryModel.MainFromClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      var memberFromClause = (MemberFromClause) queryModel.BodyClauses[1];

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.EqualTo (memberFromClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (mainFromClause));

      Assert.That (navigator.Arguments[0].Arguments[1].Operand.Expression, Is.EqualTo (whereClause.Predicate));
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (navigator.Arguments[0].Arguments[0].Expression.Type));
      Assert.That (memberFromClause.Identifier.Name, Is.EqualTo ("s2"));
    }

    [Test]
    public void RecursiveSelectMany ()
    {
      var expression = MixedTestQueryGenerator.CreateThreeFromWhereQuery_SelectManyExpression (_querySource, _querySource, _querySource);
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);
      
      var memberFromClause1 = (MemberFromClause) queryModel.BodyClauses[0];
      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      var memberFromClause2 = (MemberFromClause) queryModel.BodyClauses[2];
      var mainFromClause = queryModel.MainFromClause;

      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (memberFromClause1.PreviousClause, Is.EqualTo (mainFromClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (memberFromClause1));
      Assert.That (memberFromClause2.PreviousClause, Is.EqualTo (whereClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.EqualTo (memberFromClause2));

      Assert.That (navigator.Arguments[0].Arguments[1].Operand.Expression, Is.EqualTo (whereClause.Predicate));
      Assert.That (mainFromClause.QuerySource.Type, Is.EqualTo (navigator.Arguments[0].Arguments[0].Arguments[0].Expression.Type));
      Assert.That (memberFromClause1.Identifier.Name, Is.EqualTo ("s2"));
      Assert.That (memberFromClause2.Identifier.Name, Is.EqualTo ("s3"));
    }

    [Test]
    public void Let ()
    {
      var expression = LetTestQueryGenerator.CreateSimpleSelect_LetExpression (_querySource);
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var letClause = (LetClause) queryModel.BodyClauses[0];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (letClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.EqualTo (letClause));

      Assert.That (letClause.ProjectionExpression.Body.NodeType, Is.EqualTo (navigator.Arguments[0].Arguments[1].Operand.Body.Expression.NodeType));
      Assert.That (letClause.ProjectionExpression.Body.Type, Is.EqualTo (navigator.Arguments[0].Arguments[1].Operand.Body.Expression.Type));
    }

    [Test]
    public void OrderByAndThenBy ()
    {
      var expression = OrderByTestQueryGenerator.CreateOrderByQueryWithOrderByAndThenBy_OrderByExpression (_querySource);
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);

      var orderByClause = (OrderByClause) queryModel.BodyClauses[0];
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.PreviousClause, Is.Null);
      Assert.That (orderByClause.OrderingList.Count, Is.EqualTo (3));
      Assert.That (orderByClause.PreviousClause, Is.EqualTo (mainFromClause));
      Assert.That (selectClause.PreviousClause, Is.EqualTo (orderByClause));

      var parameterExpression = Expression.Parameter (typeof (Student), "s");
      var expectedSelectExpression = Expression.Lambda (typeof (Func<Student, Student>), parameterExpression, parameterExpression);
      Assert.That (selectClause.Selector.Body.NodeType, Is.EqualTo (expectedSelectExpression.Body.NodeType));
      Assert.That (selectClause.Selector.Body.Type, Is.EqualTo (expectedSelectExpression.Body.Type));
      Assert.That (mainFromClause.QuerySource.NodeType, Is.EqualTo (navigator.Arguments[0].Arguments[0].Arguments[0].Expression.NodeType));
    }

    [Test]
    public void MultipleOrderBys ()
    {
      var query = OrderByTestQueryGenerator.CreateOrderByQueryWithMultipleOrderBys (_querySource);
      var queryModel = _queryParser.GetParsedQuery (query.Expression);

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      var orderByClause1 = (OrderByClause) queryModel.BodyClauses[0];
      Assert.That (orderByClause1.OrderingList.Count, Is.EqualTo (3));
      var orderByClause2 = (OrderByClause) queryModel.BodyClauses[1];
      Assert.That (orderByClause2.OrderingList.Count, Is.EqualTo (1));

      Assert.That (orderByClause1.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (orderByClause2.PreviousClause, Is.EqualTo (orderByClause1));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.EqualTo (orderByClause2));
    }

    [Test]
    public void OrderByAndWhere ()
    {
      var expression = MixedTestQueryGenerator.CreateOrderByQueryWithWhere_OrderByExpression (_querySource);
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);


      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      var orderByClause = (OrderByClause) queryModel.BodyClauses[1];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (whereClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (orderByClause.PreviousClause, Is.EqualTo (whereClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.EqualTo (orderByClause));

      Assert.That (navigator.Arguments[0].Arguments[1].Operand.Expression, Is.EqualTo (whereClause.Predicate));
    }

    [Test]
    public void MultiFromsWithOrderBy ()
    {
      var expression = MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (_querySource, _querySource).Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);
      var navigator = new ExpressionTreeNavigator (expression);
      
      var memberFromClause = (AdditionalFromClause) queryModel.BodyClauses[0];
      var whereClause = (WhereClause) queryModel.BodyClauses[1];
      var orderByClause = (OrderByClause) queryModel.BodyClauses[2];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (memberFromClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (memberFromClause));
      Assert.That (orderByClause.PreviousClause, Is.EqualTo (whereClause));
      Assert.That (queryModel.SelectOrGroupClause.PreviousClause, Is.EqualTo (orderByClause));

      Assert.That (navigator.Arguments[0].Arguments[0].Arguments[0].Arguments[1].Operand.Expression, Is.EqualTo (whereClause.Predicate));
    }

    
  }
}