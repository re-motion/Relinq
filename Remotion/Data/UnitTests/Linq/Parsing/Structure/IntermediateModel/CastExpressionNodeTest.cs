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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class CastExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Cast<int> ());
      Assert.That (CastExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new CastExpressionNode (CreateParseInfo (sourceMock));
      var expression = ExpressionHelper.CreateLambdaExpression ();
      var parameter = ExpressionHelper.CreateParameterExpression ();
      var expectedResult = ExpressionHelper.CreateExpression ();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression, null)).Return (expectedResult);

      var result = node.Resolve (parameter, expression, null);

      sourceMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void CreateParameterForOutput ()
    {
      var source = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3, 4, 5 });
      var node = new CastExpressionNode (CreateParseInfo (source, "Test"));
      
      var parameter = node.CreateParameterForOutput ();

      Assert.That (parameter.Name, Is.EqualTo ("x"));
      Assert.That (parameter.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    public void CreateClause ()
    {
      var previousClause = ExpressionHelper.CreateClause ();
      var node = new CastExpressionNode (CreateParseInfo());

      var clause = node.CreateClause (previousClause, null);

      Assert.That (clause, Is.SameAs (previousClause));
    }
  }
}