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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  public abstract class ExpressionNodeTestBase
  {
    [SetUp]
    public virtual void SetUp ()
    {
      SourceNode = ExpressionNodeObjectMother.CreateMainSource();
      ClauseGenerationContext = new ClauseGenerationContext(CompoundNodeTypeProvider.CreateDefault());

      QueryModel = SourceNode.Apply (null, ClauseGenerationContext);
      SourceClause = QueryModel.MainFromClause;
      SourceReference = (QuerySourceReferenceExpression) QueryModel.SelectClause.Selector;
    }

    public MainSourceExpressionNode SourceNode { get; private set; }
    public MainFromClause SourceClause { get; private set; }
    public QuerySourceReferenceExpression SourceReference { get; private set; }
    public ClauseGenerationContext ClauseGenerationContext { get; private set; }
    public QueryModel QueryModel { get; private set; }

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
    
    protected void TestApply (ResultOperatorExpressionNodeBase node, Type expectedResultOperatorType)
    {
      var result = node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));
      
      Assert.That (QueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (QueryModel.ResultOperators[0], Is.InstanceOfType (expectedResultOperatorType));
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

    protected MethodCallExpressionParseInfo CreateParseInfo (IExpressionNode source, string associatedIdentifier, MethodInfo method)
    {
      var arguments = from p in method.GetParameters ()
                      let t = p.ParameterType
                      let defaultValue = t.IsValueType ? Activator.CreateInstance (t) : null
                      select Expression.Constant (defaultValue, t);
      var methodCallExpression = Expression.Call (method, arguments.ToArray ());

      return new MethodCallExpressionParseInfo (associatedIdentifier, source, methodCallExpression);
    }

    protected MethodCallExpressionParseInfo CreateParseInfo (MethodInfo method)
    {
      return CreateParseInfo (SourceNode, "x", method);
    }

    protected void AssertSupportedMethod_Generic<TResult1, TResult2> (
        MethodInfo[] supportedMethods, 
        Expression<Func<IQueryable<object>, TResult1>> queryableMethodCall,
        Expression<Func<IEnumerable<object>, TResult2>> enumerableMethodCall)
    {
      if (queryableMethodCall != null)
      {
        var queryableMethod = GetGenericMethodDefinition (queryableMethodCall);
        Assert.That (supportedMethods, List.Contains (queryableMethod));
      }

      var enumerableMethod = GetGenericMethodDefinition_Enumerable (enumerableMethodCall);
      Assert.That (supportedMethods, List.Contains (enumerableMethod));
    }

    protected void AssertSupportedMethod_NonGeneric<TResult> (
        MethodInfo[] supportedMethods,
        Expression<Func<IQueryable<object>, TResult>> queryableMethodCall,
        Expression<Func<IEnumerable<object>, TResult>> enumerableMethodCall)
    {
      if (queryableMethodCall != null)
      {
        var queryableMethod = GetMethod (queryableMethodCall);
        Assert.That (supportedMethods, List.Contains (queryableMethod));
      }

      if (enumerableMethodCall != null)
      {
        var enumerableMethod = GetMethod_Enumerable (enumerableMethodCall);
        Assert.That (supportedMethods, List.Contains (enumerableMethod));
      }
    }
  }
}
