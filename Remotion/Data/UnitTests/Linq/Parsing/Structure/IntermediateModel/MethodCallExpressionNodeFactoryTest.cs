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
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MethodCallExpressionNodeFactoryTest
  {
    private IExpressionNode _source;
    private MethodCallExpressionParseInfo _parseInfo;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionNodeObjectMother.CreateConstant();
      _parseInfo = new MethodCallExpressionParseInfo ("foo", _source, ExpressionHelper.CreateMethodCallExpression());
    }

    [Test]
    public void CreateExpressionNode ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression<int, int> (i => i);
      var result = MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (SelectExpressionNode), _parseInfo, new object[] { selector });

      Assert.That (result, Is.InstanceOfType (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));
      Assert.That (((SelectExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("foo"));
    }

    [Test]
    public void CreateExpressionNode_NullSupplied ()
    {
      var result = MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (FirstExpressionNode), _parseInfo, new object[0]);

      Assert.That (result, Is.InstanceOfType (typeof (FirstExpressionNode)));
      Assert.That (((FirstExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((FirstExpressionNode) result).OptionalPredicate, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CreateExpressionNode_InvalidType ()
    {
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (MethodCallExpressionNodeFactoryTest), _parseInfo, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Expression node type 'Remotion.Data.UnitTests.Linq.Parsing.Structure."
        + "IntermediateModel.TestDomain.ExpressionNodeWithTooManyCtors' contains too many constructors. It must only contain a single constructor, allowing null "
        + "to be passed for any optional arguments.\r\nParameter name: nodeType")]
    public void CreateExpressionNode_MoreThanOneCtor ()
    {
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (ExpressionNodeWithTooManyCtors), _parseInfo, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The constructor of expression node type 'Remotion.Data.Linq.Parsing.Structure."
          + "IntermediateModel.SelectExpressionNode' only takes 2 parameters, but you specified 3 (including the parse info parameter).\r\n"
          + "Parameter name: additionalConstructorParameters")]
    public void CreateExpressionNode_TooManyParameters ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression ();
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (SelectExpressionNode), _parseInfo, new object[] { selector, selector });
    }
  }
}