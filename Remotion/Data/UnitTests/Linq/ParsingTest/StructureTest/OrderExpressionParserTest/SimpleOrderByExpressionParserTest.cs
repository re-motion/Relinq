// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.OrderExpressionParserTest
{
  [TestFixture]
  public class SimpleOrderByExpressionParserTest
  {
    public static void AssertOrderExpressionsEqual (OrderExpressionData one, OrderExpressionData two)
    {
      Assert.AreEqual (one.TypedExpression, two.TypedExpression);
      Assert.AreEqual (one.OrderDirection, two.OrderDirection);
      Assert.AreEqual (one.FirstOrderBy, two.FirstOrderBy);
    }

    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private BodyHelper _bodyOrderByHelper;
    private ParseResultCollector _result;

    [SetUp]
    public void SetUp()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = OrderByTestQueryGenerator.CreateOrderByQueryWithOrderByAndThenBy_OrderByExpression (_querySource);
      _navigator = new ExpressionTreeNavigator (_expression);
      _result = new ParseResultCollector (_expression);
      new OrderByExpressionParser (true).Parse (_result, _expression);
      _bodyOrderByHelper = new BodyHelper (_result.BodyExpressions);
    }

    [Test]
    public void ParsesOrderingExpressions()
    {
      Assert.IsNotNull (_bodyOrderByHelper.OrderingExpressions);
      Assert.AreEqual(3, _bodyOrderByHelper.OrderingExpressions.Count);
      AssertOrderExpressionsEqual (new OrderExpressionData (true, OrderDirection.Asc,
          (LambdaExpression)_navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Expression), _bodyOrderByHelper.OrderingExpressions[0]);
      AssertOrderExpressionsEqual (new OrderExpressionData (false, OrderDirection.Desc,
          (LambdaExpression) _navigator.Arguments[0].Arguments[1].Operand.Expression), _bodyOrderByHelper.OrderingExpressions[1]);
      AssertOrderExpressionsEqual (new OrderExpressionData (false, OrderDirection.Asc,
          (LambdaExpression) _navigator.Arguments[1].Operand.Expression), _bodyOrderByHelper.OrderingExpressions[2]);
    }
        
    [Test]
    public void ParsesFromExpressions()
    {
      Assert.IsNotNull (_bodyOrderByHelper.FromExpressions);
      Assert.That (_bodyOrderByHelper.FromExpressions, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[0].Arguments[0].Expression
          }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _navigator.Arguments[0].Arguments[0].Arguments[0].Expression);
      Assert.AreSame (_querySource, ((ConstantExpression) _bodyOrderByHelper.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers()
    {
      Assert.IsNotNull (_bodyOrderByHelper.FromIdentifiers);
      Assert.That (_bodyOrderByHelper.FromIdentifiers, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Parameters[0].Expression
          }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyOrderByHelper.FromIdentifiers[0]);
      Assert.AreEqual ("s", (_bodyOrderByHelper.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (1, _result.ProjectionExpressions.Count);
      Assert.IsNull (_result.ProjectionExpressions[0]);
    }

    [Test]
    public void ParsesProjectionExpressions_NotTopLevel ()
    {
      _result = new ParseResultCollector (_expression);
      new OrderByExpressionParser (false).Parse (_result, _expression);
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (0, _result.ProjectionExpressions.Count);
    }
  }
}
