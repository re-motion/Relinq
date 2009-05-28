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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SingleExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Single ());
      Assert.That (SingleExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Single (i => i > 5));
      Assert.That (SingleExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      var node = new SingleExpressionNode (SourceStub, null);
      node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void GetResolvedPredicate ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SingleExpressionNode (sourceMock, predicate);

      var expectedResult = ExpressionHelper.CreateLambdaExpression ();
      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Return (expectedResult);

      var result = node.GetResolvedExpression ();

      sourceMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException),ExpectedMessage = "Predicate must not be null.\r\nParameter name: OptionalPredicate")]
    public void GetResolvedPredicate_ThrowsArgumentNullException ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new SingleExpressionNode (sourceMock, null);
      node.GetResolvedExpression();
    }
  }
}