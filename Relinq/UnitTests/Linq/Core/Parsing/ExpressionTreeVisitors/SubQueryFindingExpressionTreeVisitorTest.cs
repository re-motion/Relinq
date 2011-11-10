// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.TestQueryGenerators;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class SubQueryFindingExpressionTreeVisitorTest
  {
    private MethodInfoBasedNodeTypeRegistry _methodInfoBasedNodeTypeRegistry;

    [SetUp]
    public void SetUp ()
    {
      _methodInfoBasedNodeTypeRegistry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (typeof(SelectExpressionNode).Assembly.GetTypes());
    }

    [Test]
    public void Initialization_InnerParserHasNoTransformations ()
    {
      var visitorInstance = Activator.CreateInstance (
          typeof (SubQueryFindingExpressionTreeVisitor), BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { _methodInfoBasedNodeTypeRegistry }, null);

      var innerParser = (QueryParser) PrivateInvoke.GetNonPublicField (visitorInstance, "_queryParser");
      Assert.That (innerParser.ExpressionTreeParser.Processor, Is.TypeOf (typeof (NullExpressionTreeProcessor)));
    }

    [Test]
    public void TreeWithNoSubquery ()
    {
      Expression expression = Expression.Constant ("test");

      Expression newExpression = SubQueryFindingExpressionTreeVisitor.Process (expression, _methodInfoBasedNodeTypeRegistry);
      Assert.That (newExpression, Is.SameAs (expression));
    }

    [Test]
    public void TreeWithSubquery ()
    {
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateCookQueryable()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      Expression newExpression = SubQueryFindingExpressionTreeVisitor.Process (surroundingExpression, _methodInfoBasedNodeTypeRegistry);

      Assert.That (newExpression, Is.Not.SameAs (surroundingExpression));
      Assert.That (newExpression, Is.InstanceOf (typeof (LambdaExpression)));

      var newLambdaExpression = (LambdaExpression) newExpression;
      Assert.That (newLambdaExpression.Body, Is.InstanceOf (typeof (SubQueryExpression)));

      var newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (
          ((QuerySourceReferenceExpression) newSubQueryExpression.QueryModel.SelectClause.Selector).ReferencedQuerySource,
          Is.SameAs (newSubQueryExpression.QueryModel.MainFromClause));
    }

    [Test]
    public void VisitorUsesNodeTypeRegistry_ToParseAndAnalyzeSubQueries ()
    {
      Expression subQuery = ExpressionHelper.MakeExpression (() => CustomSelect (ExpressionHelper.CreateCookQueryable(), s => s));
      Expression surroundingExpression = Expression.Lambda (subQuery);
      // evaluate the ExpressionHelper.CreateCookQueryable () method
      var inputExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (surroundingExpression);

      var emptyNodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry();
      emptyNodeTypeRegistry.Register (new[] { ((MethodCallExpression) subQuery).Method }, typeof (SelectExpressionNode));

      var newLambdaExpression =
          (LambdaExpression) SubQueryFindingExpressionTreeVisitor.Process (inputExpression, emptyNodeTypeRegistry);
      Assert.That (newLambdaExpression.Body, Is.InstanceOf (typeof (SubQueryExpression)));
    }

    [Test]
    public void VisitorUsesExpressionTreeVisitor_ToGetPotentialQueryOperator ()
    {
      _methodInfoBasedNodeTypeRegistry.Register (new[] { typeof (QueryableFakeWithCount<>).GetMethod ("get_Count") }, typeof (CountExpressionNode));
      Expression subQuery = ExpressionHelper.MakeExpression (() => new QueryableFakeWithCount<int>().Count);
      Expression surroundingExpression = Expression.Lambda (subQuery);

      var newLambdaExpression =
          (LambdaExpression) SubQueryFindingExpressionTreeVisitor.Process (surroundingExpression, _methodInfoBasedNodeTypeRegistry);
      Assert.That (newLambdaExpression.Body, Is.InstanceOf (typeof (SubQueryExpression)));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = SubQueryFindingExpressionTreeVisitor.Process (expression, _methodInfoBasedNodeTypeRegistry);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression_ChildrenAreEvaluated ()
    {
      var subQuery = ExpressionHelper.MakeExpression (() => (from s in ExpressionHelper.CreateCookQueryable () select s).Any());
      var extensionExpression = new VBStringComparisonExpression (subQuery, true);
      // evaluate the ExpressionHelper.CreateCookQueryable () method
      var inputExpression = PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (extensionExpression);

      var result = SubQueryFindingExpressionTreeVisitor.Process (inputExpression, _methodInfoBasedNodeTypeRegistry);

      Assert.That (((VBStringComparisonExpression) result).Comparison, Is.TypeOf (typeof (SubQueryExpression)));
    }

    public static IQueryable<Cook> CustomSelect (IQueryable<Cook> source, Expression<Func<Cook, Cook>> selector)
    {
      throw new NotImplementedException();
    }
  }
}