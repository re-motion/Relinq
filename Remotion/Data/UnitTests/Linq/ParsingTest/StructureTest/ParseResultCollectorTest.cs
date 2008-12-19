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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
{
  [TestFixture]
  public class ParseResultCollectorTest
  {
    private ParseResultCollector _collector;
    private Expression _root;

    [SetUp]
    public void SetUp()
    {
      _root = ExpressionHelper.CreateExpression();
      _collector = new ParseResultCollector (_root);
    }

    [Test]
    public void ExpressionTreeRoot ()
    {
      Assert.AreSame (_root, _collector.ExpressionTreeRoot);
    }
    
    [Test]
    public void ResultModifiers ()
    {
      Assert.That (_collector.ResultModifierData, Is.Empty);
      
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (query));
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo, query.Expression);

      _collector.AddResultModifierData (methodCallExpression);

      Assert.That (_collector.ResultModifierData, Is.EqualTo (new[] { methodCallExpression }));
    }
    
    [Test]
    public void BodyExpressions()
    {
      Assert.That (_collector.BodyExpressions, Is.Empty);
      FromExpressionData expression1 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      FromExpressionData expression2 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      _collector.AddBodyExpression (expression1);
      _collector.AddBodyExpression (expression2);
      Assert.That (_collector.BodyExpressions, Is.EqualTo (new[] {expression1, expression2}));
    }

    [Test]
    public void ExtractMainFromExpression()
    {
      FromExpressionData expression1 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      FromExpressionData expression2 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      _collector.AddBodyExpression (expression1);
      _collector.AddBodyExpression (expression2);

      FromExpressionData mainFromExpressionData = _collector.ExtractMainFromExpression();
      Assert.AreSame (expression1, mainFromExpressionData);
      Assert.That (_collector.BodyExpressions, Is.EqualTo (new[] { expression2 }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "There are no body expressions to be extracted.")]
    public void ExtractMainFromExpression_NoBodyExpressions ()
    {
      _collector.ExtractMainFromExpression ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The first body expression is no FromExpressionData.")]
    public void ExtractMainFromExpression_NotFromExpression ()
    {
      WhereExpressionData expression1 = new WhereExpressionData (ExpressionHelper.CreateLambdaExpression());
      _collector.AddBodyExpression (expression1);
      _collector.ExtractMainFromExpression ();
    }

    [Test]
    public void ProjectionExpressions ()
    {
      Assert.That (_collector.ProjectionExpressions, Is.Empty);
      LambdaExpression expression1 = ExpressionHelper.CreateLambdaExpression();
      LambdaExpression expression2 = ExpressionHelper.CreateLambdaExpression ();
      _collector.AddProjectionExpression (expression1);
      _collector.AddProjectionExpression (null);
      _collector.AddProjectionExpression (expression2);
      Assert.That (_collector.ProjectionExpressions, Is.EqualTo (new[] { expression1, null, expression2 }));
    }

    [Test]
    public void Simplify_FindsSubQueries ()
    {
      List<QueryModel> registry = new List<QueryModel> ();
      Expression inner = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource()).Expression;
      LambdaExpression outer = Expression.Lambda (inner);
      _collector.AddProjectionExpression (outer);
      _collector.Simplify(registry);

      Assert.That (_collector.ProjectionExpressions.Count, Is.EqualTo (1));
      Assert.That (_collector.ProjectionExpressions[0].Body, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (registry, Is.EqualTo (new[] {((SubQueryExpression) _collector.ProjectionExpressions[0].Body).QueryModel}));
    }

    [Test]
    public void Simplify_EvaluatesExpressions ()
    {
      List<QueryModel> registry = new List<QueryModel> ();
      Expression inner = Expression.Add (Expression.Constant (1), Expression.Constant (1));
      LambdaExpression outer = Expression.Lambda (inner);
      _collector.AddProjectionExpression (outer);
      _collector.Simplify (registry);

      Assert.That (_collector.ProjectionExpressions.Count, Is.EqualTo (1));
      Assert.That (_collector.ProjectionExpressions[0].Body, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression)_collector.ProjectionExpressions[0].Body).Value, Is.EqualTo (2));
      Assert.That (registry, Is.Empty);
    }

    [Test]
    public void Simplify_SimplifiesProjectionExpressions ()
    {
      List<QueryModel> registry = new List<QueryModel> ();
      Expression inner = Expression.Add (Expression.Constant (1), Expression.Constant (1));
      LambdaExpression outer = Expression.Lambda (inner);
      _collector.AddProjectionExpression (outer);
      _collector.Simplify (registry);

      Assert.That (_collector.ProjectionExpressions[0].Body, Is.InstanceOfType (typeof (ConstantExpression)));
    }

    [Test]
    public void Simplify_SimplifiesBodyExpressions ()
    {
      List<QueryModel> registry = new List<QueryModel> ();
      Expression inner = Expression.Add (Expression.Constant (1), Expression.Constant (1));
      FromExpressionData bodyExpression = new FromExpressionData (inner, Expression.Parameter (typeof (string), "s"));
      _collector.AddBodyExpression (bodyExpression);
      _collector.Simplify (registry);

      Assert.That (_collector.BodyExpressions[0].Expression, Is.InstanceOfType (typeof (ConstantExpression)));
    }

    [Test]
    public void Simplify_IgnoresNullExpressions ()
    {
      List<QueryModel> registry = new List<QueryModel> ();
      _collector.AddProjectionExpression (null);
      _collector.Simplify (registry);

      Assert.That (_collector.ProjectionExpressions[0], Is.Null);
    }

  }
}
