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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MethodCallExpressionNodeFactoryTest
  {
    private IExpressionNode _source;
    private MethodCallExpressionParseInfo _parseInfo;

    [SetUp]
    public void SetUp ()
    {
      _source = ExpressionNodeObjectMother.CreateMainSource();
      _parseInfo = new MethodCallExpressionParseInfo ("foo", _source, ExpressionHelper.CreateMethodCallExpression<Cook>());
    }

    [Test]
    public void CreateExpressionNode ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression<int, int> (i => i);
      var result = MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (SelectExpressionNode), _parseInfo, new object[] { selector });

      Assert.That (result, Is.InstanceOf (typeof (SelectExpressionNode)));
      Assert.That (((SelectExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((SelectExpressionNode) result).Selector, Is.SameAs (selector));
      Assert.That (((SelectExpressionNode) result).AssociatedIdentifier, Is.EqualTo ("foo"));
    }

    [Test]
    public void CreateExpressionNode_NullSupplied ()
    {
      var result = MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (FirstExpressionNode), _parseInfo, new object[0]);

      Assert.That (result, Is.InstanceOf (typeof (FirstExpressionNode)));
      Assert.That (((FirstExpressionNode) result).Source, Is.SameAs (_source));
      Assert.That (((FirstExpressionNode) result).Source, Is.SameAs (_parseInfo.Source));
    }

    [Test]
    public void CreateExpressionNode_OnlyOnePublicInstanceCtor ()
    {
      var result = MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (ExpressionNodeWithOnePublicInstanceCtor), _parseInfo, new object[0]);

      Assert.That (result, Is.InstanceOf (typeof (ExpressionNodeWithOnePublicInstanceCtor)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Parameter 'nodeType' is a 'Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.MethodCallExpressionNodeFactoryTest', "
        + "which cannot be assigned to type 'Remotion.Linq.Parsing.Structure.IntermediateModel.IExpressionNode'."
        + "\r\nParameter name: nodeType")]
    public void CreateExpressionNode_InvalidType ()
    {
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (MethodCallExpressionNodeFactoryTest), _parseInfo, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Expression node type 'Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.TestDomain.ExpressionNodeWithTooManyCtors' "
        + "contains too many constructors. It must only contain a single constructor, allowing null to be passed for any optional arguments."
        + "\r\nParameter name: nodeType")]
    public void CreateExpressionNode_MoreThanOneCtor ()
    {
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (ExpressionNodeWithTooManyCtors), _parseInfo, new object[0]);
    }

    [Test]
    [ExpectedException (typeof (ExpressionNodeInstantiationException), ExpectedMessage = 
        "The constructor of expression node type 'Remotion.Linq.Parsing.Structure.IntermediateModel.SelectExpressionNode' "
        + "only takes 2 parameters, but you specified 3 (including the parse info parameter).")]
    public void CreateExpressionNode_TooManyParameters ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression ();
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (SelectExpressionNode), _parseInfo, new object[] { selector, selector });
    }

    [Test]
    [ExpectedException (typeof (ExpressionNodeInstantiationException), ExpectedMessage =
        "The given arguments did not match the expected arguments: Object of type "
        + "'Remotion.Linq.Development.UnitTesting.Clauses.Expressions.TestExtensionExpression' cannot be converted to type "
        + "'System.Linq.Expressions.LambdaExpression'.")]
    public void CreateExpressionNode_InvalidNodeParameterType ()
    {
      var selector = new TestExtensionExpression (Expression.Constant (0));
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (SelectExpressionNode), _parseInfo, new object[] { selector });
    }

    [Test]
    [ExpectedException (typeof (ExpressionNodeInstantiationException), ExpectedMessage = 
        "Object of type 'System.Linq.Expressions.ConstantExpression' cannot be converted to type 'System.Linq.Expressions.LambdaExpression'. "
        + "If you tried to pass a delegate instead of a LambdaExpression, this is not supported because delegates are not parsable expressions.")]
    public void CreateExpressionNode_InvalidNodeParameterType_ConstantDelegateInsteadOfLambda ()
    {
      var selector = Expression.Constant ((Func<int, int>) (i => i));
      MethodCallExpressionNodeFactory.CreateExpressionNode (typeof (SelectExpressionNode), _parseInfo, new object[] { selector });
    }
  }
}
