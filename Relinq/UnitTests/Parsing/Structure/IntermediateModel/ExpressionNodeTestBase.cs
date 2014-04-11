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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  public abstract class ExpressionNodeTestBase
  {
    [SetUp]
    public virtual void SetUp ()
    {
      SourceNode = ExpressionNodeObjectMother.CreateMainSource();
      ClauseGenerationContext = new ClauseGenerationContext(ExpressionTreeParser.CreateDefaultNodeTypeProvider());

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
      Assert.That (QueryModel.ResultOperators[0], Is.InstanceOf (expectedResultOperatorType));
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
      return new MethodCallExpressionParseInfo (associatedIdentifier, source, ExpressionHelper.CreateMethodCallExpression<Cook>());
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
        Assert.That (supportedMethods, Has.Member (queryableMethod));
      }

      var enumerableMethod = GetGenericMethodDefinition_Enumerable (enumerableMethodCall);
      Assert.That (supportedMethods, Has.Member (enumerableMethod));
    }

    protected void AssertSupportedMethod_NonGeneric<TResult> (
        MethodInfo[] supportedMethods,
        Expression<Func<IQueryable<object>, TResult>> queryableMethodCall,
        Expression<Func<IEnumerable<object>, TResult>> enumerableMethodCall)
    {
      if (queryableMethodCall != null)
      {
        var queryableMethod = GetMethod (queryableMethodCall);
        Assert.That (supportedMethods, Has.Member (queryableMethod));
      }

      if (enumerableMethodCall != null)
      {
        var enumerableMethod = GetMethod_Enumerable (enumerableMethodCall);
        Assert.That (supportedMethods, Has.Member (enumerableMethod));
      }
    }

    protected void AssertSupportedMethods_ByName<T> (IEnumerable<NameBasedRegistrationInfo> supportedMethodNames, params Expression<Func<T>>[] methodExpressions)
    {
      var nameBasedRegistry = new MethodNameBasedNodeTypeRegistry();
      nameBasedRegistry.Register (supportedMethodNames, typeof (object));

      foreach (var methodExpression in methodExpressions)
      {
        var methodInfo = ((MethodCallExpression) methodExpression.Body).Method;
        Assert.That (
            nameBasedRegistry.GetNodeType (methodInfo), 
            Is.Not.Null, 
            string.Format ("Method '{0}.{1}' is not supported ('{2}').", methodInfo.DeclaringType.Name, methodInfo.Name, methodExpression));
      }
    }

    protected void AssertNotSupportedMethods_ByName<T> (IEnumerable<NameBasedRegistrationInfo> supportedMethodNames, params Expression<Func<T>>[] methodExpressions)
    {
      var nameBasedRegistry = new MethodNameBasedNodeTypeRegistry ();
      nameBasedRegistry.Register (supportedMethodNames, typeof (object));

      foreach (var methodExpression in methodExpressions)
      {
        var methodInfo = ((MethodCallExpression) methodExpression.Body).Method;
        Assert.That (
            nameBasedRegistry.GetNodeType (methodInfo),
            Is.Null,
            string.Format ("Method '{0}.{1}' is supported.", methodInfo.DeclaringType.Name, methodInfo.Name));
      }
    }
  }
}
