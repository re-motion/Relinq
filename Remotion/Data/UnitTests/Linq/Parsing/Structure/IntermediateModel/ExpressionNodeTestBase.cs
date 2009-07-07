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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  public abstract class ExpressionNodeTestBase
  {
    [SetUp]
    public virtual void SetUp ()
    {
      SourceNode = ExpressionNodeObjectMother.CreateMainSource();
      QuerySourceClauseMapping = new QuerySourceClauseMapping();
      ClauseGenerationContext = new ClauseGenerationContext(
          QuerySourceClauseMapping, 
          MethodCallExpressionNodeTypeRegistry.CreateDefault());

      SourceClause = SourceNode.CreateMainFromClause (ClauseGenerationContext);
      SourceReference = new QuerySourceReferenceExpression (SourceClause);

      QueryModel = new QueryModel (typeof (IQueryable<Student>), SourceClause, new SelectClause (SourceReference));
    }

    public MainSourceExpressionNode SourceNode { get; private set; }
    public MainFromClause SourceClause { get; private set; }
    public QuerySourceReferenceExpression SourceReference { get; private set; }
    public ClauseGenerationContext ClauseGenerationContext { get; private set; }
    public QuerySourceClauseMapping QuerySourceClauseMapping { get; private set; }
    public QueryModel QueryModel { get; private set; }


    public Expression<Func<int, string>> OptionalSelector
    {
      get { return (i => i.ToString ()); }
    }

    protected MethodInfo GetGenericMethodDefinition<TReturn> (Expression<Func<IQueryable<object>, TReturn>> methodCallLambda)
    {
      return GetMethod (methodCallLambda).GetGenericMethodDefinition ();
    }

    protected MethodInfo GetMethod<TReturn> (Expression<Func<IQueryable<object>, TReturn>> methodCallLambda)
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (methodCallLambda);
      return methodCallExpression.Method;
    }

    protected MethodInfo GetGenericMethodDefinition_Enumerable<TReturn> (Expression<Func<IEnumerable<object>, TReturn>> methodCallLambda)
    {
      return GetMethod_Enumerable (methodCallLambda).GetGenericMethodDefinition ();
    }

    protected MethodInfo GetMethod_Enumerable<TReturn> (Expression<Func<IEnumerable<object>, TReturn>> methodCallLambda)
    {
      var methodCallExpression = (MethodCallExpression) ExpressionHelper.MakeExpression (methodCallLambda);
      return methodCallExpression.Method;
    }
    
    protected void TestApply (ResultOperatorExpressionNodeBase node, Type expectedResultModificationType)
    {
      var result = node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var selectClause = (SelectClause) QueryModel.SelectOrGroupClause;
      Assert.That (selectClause.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (selectClause.ResultOperators[0], Is.InstanceOfType (expectedResultModificationType));

      TestApply_DoesNotWrapQueryModel_AfterResultModification (node);
    }

    public void TestApply_DoesNotWrapQueryModel_AfterResultModification (ResultOperatorExpressionNodeBase node)
    {
      ((SelectClause) QueryModel.SelectOrGroupClause).ResultOperators.Add (new DistinctResultOperator ());
      var result = node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));
    }

    protected void TestApply_WithOptionalPredicate (ResultOperatorExpressionNodeBase node)
    {
      node.Apply (QueryModel, ClauseGenerationContext);

      var newWhereClause = (WhereClause) QueryModel.BodyClauses[0];
      Assert.That (newWhereClause.Predicate, Is.SameAs (node.GetResolvedOptionalPredicate (ClauseGenerationContext)));
    }

    protected void TestApply_WithOptionalSelector (ResultOperatorExpressionNodeBase node)
    {
      var expectedNewSelector = (MethodCallExpression) ExpressionHelper.Resolve<int, string> (SourceClause, i => i.ToString());
      node.Apply (QueryModel, ClauseGenerationContext);
      
      var selectClause = (SelectClause) QueryModel.SelectOrGroupClause;
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNewSelector, selectClause.Selector);
    }

    protected MethodCallExpressionParseInfo CreateParseInfo ()
    {
      return CreateParseInfo (SourceNode);
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (IExpressionNode source)
    {
      return CreateParseInfo (source, "x");
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (IExpressionNode source, string associatedIdentifier)
    {
      return new MethodCallExpressionParseInfo (associatedIdentifier, source, ExpressionHelper.CreateMethodCallExpression ());
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (MethodInfo method)
    {
      var arguments = from p in method.GetParameters ()
                      let t = p.ParameterType
                      let defaultValue = t.IsValueType ? Activator.CreateInstance (t) : null
                      select Expression.Constant (defaultValue, t);
      var methodCallExpression = Expression.Call (method, arguments.ToArray());

      return new MethodCallExpressionParseInfo ("x", SourceNode, methodCallExpression);
    }

    protected void AssertSupportedMethod_Generic<TResult1, TResult2> (
        MethodInfo[] supportedMethods, 
        Expression<Func<IQueryable<object>, TResult1>> queryableMethodCall,
        Expression<Func<IEnumerable<object>, TResult2>> enumerableMethodCall)
    {
      var queryableMethod = GetGenericMethodDefinition (queryableMethodCall);
      Assert.That (supportedMethods, List.Contains (queryableMethod));

      var enumerableMethod = GetGenericMethodDefinition_Enumerable (enumerableMethodCall);
      Assert.That (supportedMethods, List.Contains (enumerableMethod));
    }

    protected void AssertSupportedMethod_NonGeneric<TResult> (
        MethodInfo[] supportedMethods,
        Expression<Func<IQueryable<object>, TResult>> queryableMethodCall,
        Expression<Func<IEnumerable<object>, TResult>> enumerableMethodCall)
    {
      var queryableMethod = GetMethod (queryableMethodCall);
      Assert.That (supportedMethods, List.Contains (queryableMethod));

      var enumerableMethod = GetMethod_Enumerable (enumerableMethodCall);
      Assert.That (supportedMethods, List.Contains (enumerableMethod));
    }
  }
}