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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Remotion.Utilities;
using Remotion.Development.UnitTesting;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class ExpressionTreeParserTest
  {
    private MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private ExpressionTreeParser _expressionTreeParser;
    private IQueryable<int> _intSource;

    [SetUp]
    public void SetUp ()
    {
      _nodeTypeRegistry = new MethodCallExpressionNodeTypeRegistry();

      _nodeTypeRegistry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));
      _nodeTypeRegistry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      _nodeTypeRegistry.Register (TakeExpressionNode.SupportedMethods, typeof (TakeExpressionNode));

      _expressionTreeParser = new ExpressionTreeParser (_nodeTypeRegistry);

      _intSource = new[] { 1, 2, 3 }.AsQueryable ();
    }

    [Test]
    public void Parse_ConstantExpression ()
    {
      var constantExpression = Expression.Constant (_intSource);

      var result = _expressionTreeParser.Parse (constantExpression, null);

      Assert.That (result, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) result).Value, Is.SameAs (_intSource));
      Assert.That (((ConstantExpressionNode) result).QuerySourceElementType, Is.SameAs (typeof (int)));
      Assert.That (((ConstantExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("<generated>_0"));
    }

    [Test]
    public void Parse_ConstantExpression_IdentifierNameGetsIncremented ()
    {
      var constantExpression = Expression.Constant (_intSource);

      var result1 = _expressionTreeParser.Parse (constantExpression, null);
      var result2 = _expressionTreeParser.Parse (constantExpression, null);

      Assert.That (((ConstantExpressionNode) result1).AssociatedIdentifier, Is.EqualTo ("<generated>_0"));
      Assert.That (((ConstantExpressionNode) result2).AssociatedIdentifier, Is.EqualTo ("<generated>_1"));
    }

    [Test]
    public void Parse_ConstantExpression_TypeNotInferrableFromValue ()
    {
      var constantExpression = Expression.Constant (null, typeof (int[]));

      var result = _expressionTreeParser.Parse (constantExpression, null);

      Assert.That (result, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) result).Value, Is.Null);
      Assert.That (((ConstantExpressionNode) result).QuerySourceElementType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void Parse_MethodCallExpression ()
    {
      var querySource = ExpressionHelper.CreateQuerySource();
      Expression<Func<Student, int>> selector = s => s.ID;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (selector));

      var result = _expressionTreeParser.Parse (expression, null);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));

      var source = ((SelectExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) source).Value, Is.SameAs (querySource));
    }

    [Test]
    public void Parse_MethodCallExpression_GetsGeneratedIdentifier ()
    {
      var querySource = ExpressionHelper.CreateQuerySource ();
      Expression<Func<Student, int>> selector = s => s.ID;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (selector));

      var result = _expressionTreeParser.Parse (expression, null);

      Assert.That (((SelectExpressionNode) result).AssociatedIdentifier, NUnit.Framework.SyntaxHelpers.Text.StartsWith ("<generated>_"));
    }

    [Test]
    public void Parse_MethodCallExpression_PropagatesNameToSourceNode ()
    {
      var querySource = ExpressionHelper.CreateQuerySource ();
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (s => s.ID)); // "s" gets propagated to ConstantExpressionNode

      var result = _expressionTreeParser.Parse (expression, null);

      var source = ((SelectExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) source).AssociatedIdentifier, Is.EqualTo ("s"));
    }

    [Test]
    public void Parse_ComplexMethodCallExpression ()
    {
      var querySource = ExpressionHelper.CreateQuerySource();
      Expression<Func<Student, int>> selector = s => s.ID;
      Expression<Func<Student, bool>> predicate = s => s.HasDog;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Where (predicate).Select (selector));

      var result = _expressionTreeParser.Parse (expression, null);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));

      var where = ((SelectExpressionNode) result).Source;
      Assert.That (where, Is.InstanceOfType (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) where).Predicate, Is.SameAs (predicate));

      var source = ((WhereExpressionNode) where).Source;
      Assert.That (source, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) source).Value, Is.SameAs (querySource));
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void Parse_InvalidExpression ()
    {
      _expressionTreeParser.Parse (ExpressionHelper.CreateLambdaExpression (), null);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Cannot parse expression '1.ToString()' because it calls the unsupported method "
        + "'ToString'. Only query methods whose first parameter represents the remaining query chain are supported.")]
    public void Parse_InvalidMethodCall_NonQueryMethod ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => 1.ToString ());
      _expressionTreeParser.Parse (methodCallExpression, null);
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void Parse_InvalidMethodCall_UnknownMethod ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Sum());
      _expressionTreeParser.Parse (methodCallExpression, null);
    }

    [Test]
    public void InferAssociatedIdentifierForSource_TooFewArguments ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Sum ());
      Assert.That (methodCallExpression.Arguments.Count, Is.Not.GreaterThan (1));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.Null);
    }

    [Test]
    public void InferAssociatedIdentifierForSource_NoUnaryExpression ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Take (5));
      Assert.That (methodCallExpression.Arguments[1], Is.Not.InstanceOfType (typeof (UnaryExpression)));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.Null);
    }

    [Test]
    public void InferAssociatedIdentifierForSource_NoLambdaInUnaryExpression ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Take (new [] {1, 2, 3}.Length));
      Assert.That (((UnaryExpression) methodCallExpression.Arguments[1]).Operand, Is.Not.InstanceOfType (typeof (LambdaExpression)));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.Null);
    }

    [Test]
    public void InferAssociatedIdentifierForSource_LambdaHasNotExactlyOneParameter ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => TestQueryableMethod_WithLambda_WithMoreThanOneParameter (_intSource, (i, j) => i));
      Assert.That (((LambdaExpression)((UnaryExpression) methodCallExpression.Arguments[1]).Operand).Parameters.Count, Is.Not.EqualTo (1));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.Null);
    }

    [Test]
    public void InferAssociatedIdentifierForSource ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Select (i => i));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.EqualTo ("i"));
    }

    private int TestQueryableMethod_WithLambda_WithMoreThanOneParameter (IEnumerable<int> source, Expression<Func<int, int, int>> func)
    {
      return 0;
    }
  }
}