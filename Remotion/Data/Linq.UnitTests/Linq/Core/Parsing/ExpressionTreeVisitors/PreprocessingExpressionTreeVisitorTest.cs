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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class PreprocessingExpressionTreeVisitorTest
  {
    private MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;

    [SetUp]
    public void SetUp ()
    {
      _nodeTypeRegistry = MethodCallExpressionNodeTypeRegistry.CreateDefault();
    }

    [Test]
    public void TreeWithNoSubquery ()
    {
      Expression expression = Expression.Constant ("test");

      Expression newExpression = PreprocessingExpressionTreeVisitor.ReplaceSubQueries (expression, _nodeTypeRegistry);
      Assert.That (newExpression, Is.SameAs (expression));
    }

    [Test]
    public void TreeWithSubquery ()
    {
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateCookQueryable ()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      Expression newExpression = PreprocessingExpressionTreeVisitor.ReplaceSubQueries (surroundingExpression, _nodeTypeRegistry);

      Assert.That (newExpression, Is.Not.SameAs (surroundingExpression));
      Assert.That (newExpression, Is.InstanceOfType (typeof (LambdaExpression)));

      var newLambdaExpression = (LambdaExpression) newExpression;
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));

      var newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (((QuerySourceReferenceExpression) newSubQueryExpression.QueryModel.SelectClause.Selector).ReferencedQuerySource,
          Is.SameAs (newSubQueryExpression.QueryModel.MainFromClause));
    }

    [Test]
    public void VisitorUsesNodeTypeRegistry_ToParseAndAnalyzeSubQueries ()
    {
      Expression subQuery = ExpressionHelper.MakeExpression (() => CustomSelect (ExpressionHelper.CreateCookQueryable (), s => s));
      Expression surroundingExpression = Expression.Lambda (subQuery);

      var emptyNodeTypeRegistry = new MethodCallExpressionNodeTypeRegistry ();
      emptyNodeTypeRegistry.Register (new[] { ((MethodCallExpression) subQuery).Method }, typeof (SelectExpressionNode));

      var newLambdaExpression =
          (LambdaExpression) PreprocessingExpressionTreeVisitor.ReplaceSubQueries (surroundingExpression, emptyNodeTypeRegistry);
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));
    }

    [Test]
    public void VisitorUsesExpressionTreeVisitor_ToGetPotentialQueryOperator ()
    {
      _nodeTypeRegistry.Register (new[] { typeof (QueryableFakeWithCount<>).GetMethod ("get_Count") }, typeof (CountExpressionNode));
      Expression subQuery = ExpressionHelper.MakeExpression (() => new QueryableFakeWithCount<int>().Count);
      Expression surroundingExpression = Expression.Lambda (subQuery);

      var newLambdaExpression =
          (LambdaExpression) PreprocessingExpressionTreeVisitor.ReplaceSubQueries (surroundingExpression, _nodeTypeRegistry);
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = PreprocessingExpressionTreeVisitor.ReplaceSubQueries (expression, _nodeTypeRegistry);

      Assert.That (result, Is.SameAs (expression));
    }

    public static IQueryable<Cook> CustomSelect (IQueryable<Cook> source, Expression<Func<Cook, Cook>> selector)
    {
      throw new NotImplementedException ();
    }
  }
}
