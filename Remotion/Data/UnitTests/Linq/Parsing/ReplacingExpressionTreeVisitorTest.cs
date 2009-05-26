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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;

namespace Remotion.Data.UnitTests.Linq.Parsing
{
  [TestFixture]
  public class ReplacingExpressionTreeVisitorTest
  {
    private ParameterExpression _replacedNode;
    private ParameterExpression _replacementNode;

    [SetUp]
    public void SetUp ()
    {
      _replacedNode = ExpressionHelper.CreateParameterExpression ("replaced node");
      _replacementNode = ExpressionHelper.CreateParameterExpression ("replacement node");
    }

    [Test]
    public void ReplacesGivenNode_ByGivenReplacement ()
    {
      var tree = _replacedNode;

      var result = ReplacingExpressionTreeVisitor.Replace (_replacedNode, _replacementNode, tree);
      Assert.That (result, Is.SameAs (_replacementNode));
    }

    [Test]
    public void IgnoresTree_WhenReplacedNodeDoesNotExist ()
    {
      var tree = ExpressionHelper.CreateLambdaExpression();

      var result = ReplacingExpressionTreeVisitor.Replace (_replacedNode, _replacementNode, tree);
      Assert.That (result, Is.SameAs (tree));
    }

    [Test]
    public void ReplacesTreePart ()
    {
      var tree = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (0), _replacedNode);

      var result = ReplacingExpressionTreeVisitor.Replace (_replacedNode, _replacementNode, tree);

      var expectedResult = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (0), _replacementNode);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}