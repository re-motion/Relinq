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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    private ExpressionNodeTypeRegistry _nodeTypeRegistry;
    private MethodCallExpressionParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _nodeTypeRegistry = new ExpressionNodeTypeRegistry ();

      _nodeTypeRegistry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));
      _nodeTypeRegistry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      _nodeTypeRegistry.Register (TakeExpressionNode.SupportedMethods, typeof (TakeExpressionNode));

      _parser = new MethodCallExpressionParser (_nodeTypeRegistry);
    }

    [Test]
    public void Parse ()
    {
      var source = new ConstantExpressionNode (typeof (int), new[] { 1, 2, 3 });
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Where (i => i > 5));
      var whereCondition = (LambdaExpression) ((UnaryExpression) methodCallExpression.Arguments[1]).Operand;

      var result = _parser.Parse (source, methodCallExpression);

      Assert.That (result, Is.InstanceOfType (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) result).Source, Is.SameAs (source));
      Assert.That (((WhereExpressionNode) result).Predicate, Is.SameAs (whereCondition));
    }

    [Test]
    public void Parse_DifferentMethod()
    {
      var source = new ConstantExpressionNode (typeof (int), new[] { 1, 2, 3 });
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var selectProjection = (LambdaExpression) ((UnaryExpression) methodCallExpression.Arguments[1]).Operand;

      var result = _parser.Parse (source, methodCallExpression);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Source, Is.SameAs (source));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selectProjection));
    }

    [Test]
    public void Parse_TakeMethod ()
    {
      var source = new ConstantExpressionNode (typeof (int), new[] { 1, 2, 3 });
      var outer = 4;
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Take (outer + 1));

      var result = _parser.Parse (source, methodCallExpression);

      Assert.That (result, Is.InstanceOfType (typeof (TakeExpressionNode)));
      Assert.That (((TakeExpressionNode) result).Source, Is.SameAs (source));
      Assert.That (((TakeExpressionNode) result).Count, Is.EqualTo (5));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Could not parse expression 'q.First()': This overload of the method 'System.Linq.Queryable.First' " 
        + "is currently not supported, but you can register your own parser if needed.")]
    public void Parse_UnknownMethod ()
    {
      var source = new ConstantExpressionNode (typeof (int), new[] { 1, 2, 3 });
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, int> (q => q.First());

      _parser.Parse (source, methodCallExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Could not parse expression 'q.Select((i, j) => i)': This overload of the method 'System.Linq.Queryable.Select' "
        + "is currently not supported, but you can register your own parser if needed.")]
    public void Parse_UnknownOverload ()
    {
      var source = new ConstantExpressionNode (typeof (int), new[] { 1, 2, 3 });
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select ((i, j) => i));

      _parser.Parse (source, methodCallExpression);
    }

  }
}