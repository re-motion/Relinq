// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure
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
      _nodeTypeRegistry.Register (CountExpressionNode.SupportedMethods, typeof (CountExpressionNode));

      _expressionTreeParser = new ExpressionTreeParser (_nodeTypeRegistry);

      _intSource = new[] { 1, 2, 3 }.AsQueryable ();
    }

    [Test]
    public void ParseTree_Expression ()
    {
      var expression = Expression.MakeMemberAccess (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Cook ()), typeof (Cook).GetProperty ("Assistants"));

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (result, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((MainSourceExpressionNode) result).ParsedExpression, Is.SameAs (expression));
      Assert.That (((MainSourceExpressionNode) result).QuerySourceElementType, Is.SameAs (typeof (Cook)));
      Assert.That (((MainSourceExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("<generated>_0"));
    }

    [Test]
    public void ParseTree_NonQueryOperatorExpression ()
    {
      var constantExpression = Expression.Constant (_intSource);

      var result = _expressionTreeParser.ParseTree (constantExpression);

      Assert.That (result, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((MainSourceExpressionNode) result).ParsedExpression, Is.SameAs (constantExpression));
      Assert.That (((MainSourceExpressionNode) result).QuerySourceElementType, Is.SameAs (typeof (int)));
      Assert.That (((MainSourceExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("<generated>_0"));
    }

    [Test]
    public void ParseTree_NonQueryOperatorExpression_IdentifierNameGetsIncremented ()
    {
      var constantExpression = Expression.Constant (_intSource);

      var result1 = _expressionTreeParser.ParseTree (constantExpression);
      var result2 = _expressionTreeParser.ParseTree (constantExpression);

      Assert.That (((MainSourceExpressionNode) result1).AssociatedIdentifier, Is.EqualTo ("<generated>_0"));
      Assert.That (((MainSourceExpressionNode) result2).AssociatedIdentifier, Is.EqualTo ("<generated>_1"));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Cannot parse expression '0' as it has an unsupported type. Only query sources "
                                                                    + "(that is, expressions that implement IEnumerable) and query operators can be parsed.")]
    public void ParseTree_InvalidNonQueryOperatorExpression ()
    {
      _expressionTreeParser.ParseTree (Expression.Constant (0));
    }

    [Test]
    public void ParseTree_MethodCallExpression ()
    {
      var querySource = ExpressionHelper.CreateCookQueryable();
      Expression<Func<Cook, int>> selector = s => s.ID;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (selector));

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));

      var source = ((SelectExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((ConstantExpression) ((MainSourceExpressionNode) source).ParsedExpression).Value, Is.SameAs (querySource));
    }

    [Test]
    public void ParseTree_MethodCallExpression_GetsGeneratedIdentifier ()
    {
      var querySource = ExpressionHelper.CreateCookQueryable ();
      Expression<Func<Cook, int>> selector = s => s.ID;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (selector));

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (((SelectExpressionNode) result).AssociatedIdentifier, NUnit.Framework.SyntaxHelpers.Text.StartsWith ("<generated>_"));
    }

    [Test]
    public void ParseTree_MethodCallExpression_PropagatesNameToSourceNode ()
    {
      var querySource = ExpressionHelper.CreateCookQueryable ();
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (s => s.ID)); // "s" gets propagated to MainSourceExpressionNode

      var result = _expressionTreeParser.ParseTree (expression);

      var source = ((SelectExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((MainSourceExpressionNode) source).AssociatedIdentifier, Is.EqualTo ("s"));
    }

    [Test]
    public void ParseTree_MethodCallExpression_WithInstanceMethod ()
    {
      var instanceMethod = typeof (NonGenericFakeCollection).GetMethod ("Contains"); // use non-generic class
      _nodeTypeRegistry.Register (new[] { instanceMethod }, typeof (ContainsExpressionNode));

      var querySourceExpression = Expression.Parameter (typeof (NonGenericFakeCollection), "querySource");
      var itemExpression = Expression.Constant (null);
      var expression = Expression.Call (querySourceExpression, instanceMethod, itemExpression);

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (result, Is.InstanceOfType (typeof (ContainsExpressionNode)));
      Assert.That (((ContainsExpressionNode) result).Item, Is.SameAs (itemExpression));

      var source = ((ContainsExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((MainSourceExpressionNode) source).ParsedExpression, Is.SameAs (querySourceExpression));
    }

    [Test]
    public void ParseTree_MethodCallExpression_WithInstanceMethod_InGenericType ()
    {
      var containsMethodInTypeDefinition = typeof (List<>).GetMethod ("Contains");
      _nodeTypeRegistry.Register (new[] { containsMethodInTypeDefinition }, typeof (ContainsExpressionNode));

      var containsMethod = typeof (List<int>).GetMethod ("Contains");
      var querySourceExpression = Expression.Parameter (typeof (List<int>), "querySource");
      var itemExpression = Expression.Constant (4);
      var expression = Expression.Call (querySourceExpression, containsMethod, itemExpression);

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (result, Is.InstanceOfType (typeof (ContainsExpressionNode)));
      Assert.That (((ContainsExpressionNode) result).Item, Is.SameAs (itemExpression));

      var source = ((ContainsExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((MainSourceExpressionNode) source).ParsedExpression, Is.SameAs (querySourceExpression));
    }

    [Test]
    public void ParseTree_ComplexMethodCallExpression ()
    {
      var querySource = ExpressionHelper.CreateCookQueryable();
      Expression<Func<Cook, int>> selector = s => s.ID;
      Expression<Func<Cook, bool>> predicate = s => s.IsStarredCook;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Where (predicate).Select (selector));

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));

      var where = ((SelectExpressionNode) result).Source;
      Assert.That (where, Is.InstanceOfType (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) where).Predicate, Is.SameAs (predicate));

      var source = ((WhereExpressionNode) where).Source;
      Assert.That (source, Is.InstanceOfType (typeof (MainSourceExpressionNode)));
      Assert.That (((ConstantExpression) ((MainSourceExpressionNode) source).ParsedExpression).Value, Is.SameAs (querySource));
    }

    [Test]
    public void ParseTree_MemberExpression_WithProperty ()
    {
      _nodeTypeRegistry.Register (new[] { typeof (QueryableFakeWithCount<>).GetMethod ("get_Count") }, typeof (CountExpressionNode));
      var querySource = new QueryableFakeWithCount<int> ();
      var expression = ExpressionHelper.MakeExpression (() => querySource.Count);

      var result = _expressionTreeParser.ParseTree (expression);

      Assert.That (result, Is.InstanceOfType (typeof (CountExpressionNode)));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expressions of type void ('WriteLine()') are not supported.")]
    public void ParseTree_VoidExpression ()
    {
      MethodCallExpression expression = Expression.Call (typeof (Console), "WriteLine", Type.EmptyTypes);
      _expressionTreeParser.ParseTree (expression);
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void ParseTree_InvalidMethodCall_UnknownMethod ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Sum());
      _expressionTreeParser.ParseTree (methodCallExpression);
    }

    [Test]
    public void ParseTree_SimplifiesTree ()
    {
      // ReSharper disable ConvertToConstant.Local
      var outerI = 1;
      // ReSharper restore ConvertToConstant.Local
      var expression = _intSource.Where (i => 1 > outerI).Expression;

      var result = (WhereExpressionNode) _expressionTreeParser.ParseTree (expression);
      Assert.That (((ConstantExpression) result.Predicate.Body).Value, Is.EqualTo (false));
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
    public void InferAssociatedIdentifierForSource_WithUnary ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => _intSource.Select (i => i));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.EqualTo ("i"));
    }

    [Test]
    public void InferAssociatedIdentifierForSource_WithUnary_AfterConstant ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (
                                                            () => _intSource.Join (ExpressionHelper.CreateCookQueryable(), i => i, s => s.ID, (i, s) => i));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);
      Assert.That (identifier, Is.EqualTo ("i"));
    }

    [Test]
    public void InferAssociatedIdentifierForSource_WithLambda ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => ((IEnumerable<int>)_intSource).Select (i => i));

      var identifier = (string) PrivateInvoke.InvokeNonPublicMethod (_expressionTreeParser, "InferAssociatedIdentifierForSource", methodCallExpression);

      Assert.That (identifier, Is.EqualTo ("i"));
    }

    [Test]
    public void GetQueryOperatorExpression_MethodCallExpression ()
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (() => ((IEnumerable<int>) _intSource).Select (i => i));
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (methodCallExpression);
      Assert.That (queryOperatorExpression, Is.SameAs (methodCallExpression));
    }

    [Test]
    public void GetQueryOperatorExpression_MemberExpression_Registered ()
    {
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression (() => new List<int>().Count);
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Not.Null);
      Assert.That (queryOperatorExpression.Method, Is.SameAs (typeof (List<int>).GetProperty ("Count").GetGetMethod()));
      Assert.That (queryOperatorExpression.Arguments, Is.Empty);
      Assert.That (queryOperatorExpression.Object, Is.SameAs (memberExpression.Expression));
    }

    [Test]
    public void GetQueryOperatorExpression_MemberExpression_NotRegistered ()
    {
      var expressionTreeParser = new ExpressionTreeParser (new MethodCallExpressionNodeTypeRegistry ());
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression (() => new List<int> ().Count);
      var queryOperatorExpression = expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Null);
    }

    [Test]
    public void GetQueryOperatorExpression_MemberExpression_NoAccessibleGetter ()
    {
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression (() => new QueryableFakeWithCount<int> ().InternalProperty);
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Null);
    }

    [Test]
    public void GetQueryOperatorExpression_MemberExpression_Field ()
    {
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression (() => new QueryableFakeWithCount<int>().Field);
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Null);
    }

    [Test]
    public void GetQueryOperatorExpression_ArrayLength ()
    {
      var memberExpression = (UnaryExpression) ExpressionHelper.MakeExpression (() => new int[0].Length);
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Not.Null);
      Assert.That (queryOperatorExpression.Method, Is.EqualTo (typeof (Array).GetMethod ("get_Length")));
    }

    [Test]
    public void GetQueryOperatorExpression_ArrayLength_NotRegistered ()
    {
      var expressionTreeParser = new ExpressionTreeParser (new MethodCallExpressionNodeTypeRegistry ());
      var memberExpression = (UnaryExpression) ExpressionHelper.MakeExpression (() => new int[0].Length);
      var queryOperatorExpression = expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Null);
    }

    [Test]
    public void GetQueryOperatorExpression_ArrayLongLength ()
    {
      _nodeTypeRegistry.Register (LongCountExpressionNode.SupportedMethods, typeof (LongCountExpressionNode));
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression (() => new int[0].LongLength);
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (memberExpression);

      Assert.That (queryOperatorExpression, Is.Not.Null);
      Assert.That (queryOperatorExpression.Method, Is.EqualTo (typeof (Array).GetMethod ("get_LongLength")));
    }

    [Test]
    public void GetQueryOperatorExpression_OtherExpression ()
    {
      var expression = Expression.Constant (new int[0]);
      var queryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (expression);
      Assert.That (queryOperatorExpression, Is.Null);
    }

    public int TestQueryableMethod_WithLambda_WithMoreThanOneParameter (IEnumerable<int> source, Expression<Func<int, int, int>> func)
    {
      return 0;
    }
  }
}