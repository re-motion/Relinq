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
using System.Web.UI.WebControls;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class ExpressionTreeParserTest
  {
    private ExpressionNodeTypeRegistry _nodeTypeRegistry;
    private ExpressionTreeParser _expressionTreeParser;

    [SetUp]
    public void SetUp ()
    {
      _nodeTypeRegistry = new ExpressionNodeTypeRegistry();

      _nodeTypeRegistry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));
      _nodeTypeRegistry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      _nodeTypeRegistry.Register (TakeExpressionNode.SupportedMethods, typeof (TakeExpressionNode));

      _expressionTreeParser = new ExpressionTreeParser (_nodeTypeRegistry);
    }

    [Test]
    public void Parse_ConstantExpression ()
    {
      var value = new[] { 1, 2, 3 };
      var constantExpression = Expression.Constant (value);

      var result = _expressionTreeParser.Parse (constantExpression);

      Assert.That (result, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) result).Value, Is.SameAs (value));
      Assert.That (((ConstantExpressionNode) result).QuerySourceElementType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void Parse_ConstantExpression_TypeNotInferrableFromValue ()
    {
      var constantExpression = Expression.Constant (null, typeof (int[]));

      var result = _expressionTreeParser.Parse (constantExpression);

      Assert.That (result, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) result).Value, Is.Null);
      Assert.That (((ConstantExpressionNode) result).QuerySourceElementType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void Parse_MethodCallExpression ()
    {
      var querySource = ExpressionHelper.CreateQuerySource();
      Expression<Func<Student, int>> selector = s => s.ID;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Select (selector));

      var result = _expressionTreeParser.Parse (expression);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));

      var source = ((SelectExpressionNode) result).Source;
      Assert.That (source, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) source).Value, Is.SameAs (querySource));
    }

    [Test]
    public void Parse_ComplexMethodCallExpression ()
    {
      var querySource = ExpressionHelper.CreateQuerySource();
      Expression<Func<Student, int>> selector = s => s.ID;
      Expression<Func<Student, bool>> predicate = s => s.HasDog;
      var expression = ExpressionHelper.MakeExpression (() => querySource.Where (predicate).Select (selector));

      var result = _expressionTreeParser.Parse (expression);

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));

      var where = ((SelectExpressionNode) result).Source;
      Assert.That (where, Is.InstanceOfType (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) where).Predicate, Is.SameAs (predicate));

      var source = ((WhereExpressionNode) where).Source;
      Assert.That (source, Is.InstanceOfType (typeof (ConstantExpressionNode)));
      Assert.That (((ConstantExpressionNode) source).Value, Is.SameAs (querySource));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException),
        ExpectedMessage =
            "Argument expression has type System.Func`1[System.Int32] when type System.Collections.Generic.IEnumerable`1[T] was expected.\r\nParameter name: expression"
        )]
    public void Parse_InvalidExpression ()
    {
      _expressionTreeParser.Parse (ExpressionHelper.CreateLambdaExpression());
    }
  }
}