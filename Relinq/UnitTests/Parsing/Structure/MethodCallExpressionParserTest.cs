// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    private MethodInfoBasedNodeTypeRegistry _methodInfoBasedNodeTypeRegistry;
    private MethodCallExpressionParser _parser;
    private MainSourceExpressionNode _source;

    [SetUp]
    public void SetUp ()
    {
      _methodInfoBasedNodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry ();

      _methodInfoBasedNodeTypeRegistry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));
      _methodInfoBasedNodeTypeRegistry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      _methodInfoBasedNodeTypeRegistry.Register (TakeExpressionNode.SupportedMethods, typeof (TakeExpressionNode));
      _methodInfoBasedNodeTypeRegistry.Register (CountExpressionNode.SupportedMethods, typeof (CountExpressionNode));
      _methodInfoBasedNodeTypeRegistry.Register (JoinExpressionNode.SupportedMethods, typeof (JoinExpressionNode));

      _parser = new MethodCallExpressionParser (_methodInfoBasedNodeTypeRegistry);

      _source = ExpressionNodeObjectMother.CreateMainSource();
    }

    [Test]
    public void Parse_WithUnary ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Where (i => i > 5));

      var result = ParseMethodCallExpression (methodCallExpression);

      var whereCondition = (LambdaExpression) ((UnaryExpression) methodCallExpression.Arguments[1]).Operand;
      Assert.That (result, Is.InstanceOf (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("x"));
      Assert.That (((WhereExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((WhereExpressionNode) result).Predicate, Is.SameAs (whereCondition));
    }

    [Test]
    public void Parse_WithLambda ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IEnumerable<Cook>, IEnumerable<Cook>> (e => e.Select (s => s));

      var result = ParseMethodCallExpression (methodCallExpression);

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("x"));
      Assert.That (((SelectExpressionNode) result).Source, Is.SameAs (_source));

      var expectedSelector = ExpressionHelper.CreateLambdaExpression<Cook, Cook>(s => s);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, ((SelectExpressionNode) result).Selector);
    }

    [Test]
    public void Parse_DifferentMethod()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var selectProjection = (LambdaExpression) ((UnaryExpression) methodCallExpression.Arguments[1]).Operand;

      var result = ParseMethodCallExpression (methodCallExpression);

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selectProjection));
    }

    [Test]
    public void Parse_ParsedExpression ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, int> (q => q.Select (i => i + 1).Count());
      var result = (CountExpressionNode) ParseMethodCallExpression (methodCallExpression);

      Assert.That (result.ParsedExpression, Is.SameAs (methodCallExpression));
    }

    [Test]
    public void Parse_WithConstantExpression ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (
          q => q.Join (ExpressionHelper.CreateQueryable<Cook>(), i => i, s => s.ID, (i, s) => i));

      var result = ParseMethodCallExpression (methodCallExpression);

      Assert.That (result, Is.InstanceOf (typeof (JoinExpressionNode)));
      Assert.That (((JoinExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((JoinExpressionNode) result).InnerSequence, Is.InstanceOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void Parse_WithConstantExpression_ContainingAnExpression ()
    {
      var selectMethod = ReflectionUtility.GetMethod (() => ((IQueryable<int>) null).Select (i => i));
      var p = Expression.Parameter (typeof (int), "i");
      var methodCallExpression = Expression.Call (
          selectMethod,
          Expression.Parameter (typeof (IQueryable<int>), "e"),
          Expression.Constant (Expression.Lambda<Func<int, int>> (p, p)));

      var result = ParseMethodCallExpression (methodCallExpression);

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((SelectExpressionNode) result).Selector, Is.InstanceOf (typeof (LambdaExpression)));
    }

    [Test]
    public void Parse_WithNonEvaluatedParameter ()
    {
      var innerSequence = ExpressionHelper.CreateQueryable<Cook> ();
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (
          q => q.Join (innerSequence, i => i, s => s.ID, (i, s) => i));

      var result = ParseMethodCallExpression(methodCallExpression);

      Assert.That (result, Is.InstanceOf (typeof (JoinExpressionNode)));
      Assert.That (((JoinExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((JoinExpressionNode) result).InnerSequence, Is.InstanceOf (typeof (MemberExpression)));
    }

    [Test]
    public void Parse_WithSubQuery ()
    {
      var expression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<Cook>, IQueryable<int>> (
          q => q.Select (s => (from c in s.Assistants select s).Count ()));
      var selector = ((UnaryExpression) expression.Arguments[1]).Operand;

      var result = ParseMethodCallExpression (expression);

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.Not.SameAs (selector));
      Assert.That (((SelectExpressionNode) result).Selector.Body, Is.InstanceOf (typeof (SubQueryExpression)));
    }

    [Test]
    public void Parse_WithSubQuery_WithinConstantExpression ()
    {
      Expression<Func<Cook, int>> subQuery = s => (from c in s.Assistants select s).Count ();
      var selectMethod = ReflectionUtility.GetMethod (() => ((IQueryable<Cook>) null).Select (i => 5));
      var methodCallExpression = Expression.Call (
          selectMethod,
          Expression.Parameter (typeof (IQueryable<Cook>), "e"),
          Expression.Constant (subQuery));

      var selector = ((ConstantExpression) methodCallExpression.Arguments[1]).Value;

      var result = ParseMethodCallExpression (methodCallExpression);

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.Not.SameAs (selector));
      Assert.That (((SelectExpressionNode) result).Selector.Body, Is.InstanceOf (typeof (SubQueryExpression)));
    }

    [Test]
    public void Parse_WithSubQuery_UsesNodeTypeRegistry ()
    {
      var emptyNodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry ();
      emptyNodeTypeRegistry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      var parser = new MethodCallExpressionParser (emptyNodeTypeRegistry);

      var expression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<Cook>, IQueryable<int>> (
          q => q.Select (s => s.Assistants.Count ()));

      var result = parser.Parse ("t", _source, expression.Arguments.Skip (1), expression);

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.Not.TypeOf (typeof (SubQueryExpression)),
          "The given nodeTypeRegistry does not know any query methods, so no SubQueryExpression is generated.");
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Could not parse expression 'q.First()': This overload of the method 'System.Linq.Queryable.First' is currently not supported.")]
    public void Parse_UnknownMethod_ThrowsNotSupportedException ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, int> (q => q.First());

      ParseMethodCallExpression (methodCallExpression);
    }

    [Test]
    public void Parse_UnknownMethod_ExceptionCanBeSerialized ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, int> (q => q.First());

      var exception = Assert.Throws<NotSupportedException> (() => ParseMethodCallExpression (methodCallExpression));
      var deserialized = Serializer.SerializeAndDeserialize (exception);
      Assert.That (deserialized.Message, Is.EqualTo (exception.Message));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Could not parse expression 'q.Select((i, j) => i)': This overload of the method 'System.Linq.Queryable.Select' is currently not supported.")]
    public void Parse_UnknownOverload_ThrowsNotSupportedException ()
    {
      var methodCallExpression =
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select ((i, j) => i));

      ParseMethodCallExpression (methodCallExpression);
    }

    [Test]
    public void Parse_UnknownOverload_ExceptionCanBeSerialized ()
    {
      var methodCallExpression =
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select ((i, j) => i));

      var exception = Assert.Throws<NotSupportedException> (() => ParseMethodCallExpression (methodCallExpression));
      var deserialized = Serializer.SerializeAndDeserialize (exception);
      Assert.That (deserialized.Message, Is.EqualTo (exception.Message));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Could not parse expression 'q.Select(value(System.Func`2[System.Int32,System.Int32]))': "
        + "Object of type 'System.Linq.Expressions.ConstantExpression' cannot be converted to type 'System.Linq.Expressions.LambdaExpression'. "
        + "If you tried to pass a delegate instead of a LambdaExpression, this is not supported because delegates are not parsable expressions.")]
    public void Parse_InvalidParameters_ThrowsNotSupportedException ()
    {
      Func<int, int> func = i => i;
      var methodCallExpression = (MethodCallExpression)
          PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (
              ExpressionHelper.MakeExpression<IQueryable<int>, IEnumerable<int>> (q => q.Select (func)));

      ParseMethodCallExpression (methodCallExpression);
    }

    [Test]
    public void Parse_InvalidParameters_ExceptionCanBeSerialized ()
    {
      Func<int, int> func = i => i;
      var methodCallExpression = (MethodCallExpression)
          PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (
              ExpressionHelper.MakeExpression<IQueryable<int>, IEnumerable<int>> (q => q.Select (func)));

      var exception = Assert.Throws<NotSupportedException> (() => ParseMethodCallExpression (methodCallExpression));
      var deserialized = Serializer.SerializeAndDeserialize (exception);
      Assert.That (deserialized.Message, Is.EqualTo (exception.Message));
    }

    private IExpressionNode ParseMethodCallExpression (MethodCallExpression methodCallExpression)
    {
      return _parser.Parse ("x", _source, methodCallExpression.Arguments.Skip (1), methodCallExpression);
    }
  }
}
