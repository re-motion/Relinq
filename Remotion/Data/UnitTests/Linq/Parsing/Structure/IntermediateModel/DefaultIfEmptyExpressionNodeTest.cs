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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class DefaultIfEmptyExpressionNodeTest : ExpressionNodeTestBase
  {
    private DefaultIfEmptyExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new DefaultIfEmptyExpressionNode (CreateParseInfo ());
    }

    [Test]
    public void SupportedMethod ()
    {
      AssertSupportedMethod_Generic (DefaultIfEmptyExpressionNode.SupportedMethods, 
          q => q.DefaultIfEmpty (), q => q.DefaultIfEmpty (null), e => e.DefaultIfEmpty (), e => e.DefaultIfEmpty (null));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new DefaultIfEmptyExpressionNode (CreateParseInfo (sourceMock));
      var expression = ExpressionHelper.CreateLambdaExpression ();
      var parameter = ExpressionHelper.CreateParameterExpression ();
      var expectedResult = ExpressionHelper.CreateExpression ();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression, ClauseGenerationContext)).Return (expectedResult);

      var result = node.Resolve (parameter, expression, ClauseGenerationContext);

      sourceMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));      
    }

    private void AssertSupportedMethod_Generic<TResult1, TResult2, TResult3, TResult4> (
       MethodInfo[] supportedMethods,
       Expression<Func<IQueryable<object>, TResult1>> queryableMethodCall1,
       Expression<Func<IQueryable<object>, TResult2>> queryableMethodCall2,
       Expression<Func<IEnumerable<object>, TResult3>> enumerableMethodCall1,
       Expression<Func<IEnumerable<object>, TResult4>> enumerableMethodCall2)
    {
      var queryableMethod1 = GetGenericMethodDefinition (queryableMethodCall1);
      Assert.That (supportedMethods, List.Contains (queryableMethod1));

      var queryableMethod2 = GetGenericMethodDefinition (queryableMethodCall2);
      Assert.That (supportedMethods, List.Contains (queryableMethod2));

      var enumerableMethod1 = GetGenericMethodDefinition_Enumerable (enumerableMethodCall1);
      Assert.That (supportedMethods, List.Contains (enumerableMethod1));

      var enumerableMethod2 = GetGenericMethodDefinition_Enumerable (enumerableMethodCall2);
      Assert.That (supportedMethods, List.Contains (enumerableMethod2));
    }
  }
}