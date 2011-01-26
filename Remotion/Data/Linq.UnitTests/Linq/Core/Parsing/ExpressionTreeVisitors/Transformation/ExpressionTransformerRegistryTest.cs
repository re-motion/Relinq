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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Collections;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using System.Linq;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation
{
  [TestFixture]
  public class ExpressionTransformerRegistryTest
  {
    private ExpressionTransformerRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new ExpressionTransformerRegistry();
    }
     
    [Test]
    public void CreateDefault ()
    {
      var registry = ExpressionTransformerRegistry.CreateDefault();
      Assert.That (registry.RegisteredTransformerCount, Is.EqualTo (0));
    }

    [Test]
    public void RegisteredTransformerCount ()
    {
      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (0));

      var transformerStub = CreateTransformerStub<Expression> (1);
      _registry.Register (transformerStub, ExpressionType.Constant);

      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (1));

      _registry.Register (transformerStub, ExpressionType.Constant);

      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (2));

      _registry.Register (transformerStub, ExpressionType.Equal);

      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (3));
    }

    [Test]
    public void GetTransformers_NoTransformerRegistered ()
    {
      var expression = CreateSimpleExpression(0);

      var result = _registry.GetTransformations (expression).ToArray();

      Assert.That (result, Is.Empty);
    }

    [Test]
    public void GetTransformers_OneTransformerRegistered ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub = CreateTransformerStub<Expression> (1);
      
      _registry.Register (transformerStub, expression.NodeType);

      var result = _registry.GetTransformations (expression).ToArray();

      Assert.That (result.Length, Is.EqualTo (1));
      CheckTransformationMatchesTransformer (result[0], transformerStub);
    }

    [Test]
    public void GetTransformers_UsesNodeTypeToMatchTransformers ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub = CreateTransformerStub<Expression> (1);

      _registry.Register (transformerStub, ExpressionType.OrElse);

      var result = _registry.GetTransformations (expression).ToArray ();

      Assert.That (result, Is.Empty);
    }

    [Test]
    public void GetTransformers_TwoTransformerRegistered ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub1 = CreateTransformerStub<Expression> (1);
      var transformerStub2 = CreateTransformerStub<Expression> (2);

      _registry.Register (transformerStub1, expression.NodeType);
      _registry.Register (transformerStub2, expression.NodeType);
      
      var result = _registry.GetTransformations (expression).ToArray ();

      Assert.That (result.Length, Is.EqualTo (2));
      CheckTransformationMatchesTransformer (result[0], transformerStub1);
      CheckTransformationMatchesTransformer (result[1], transformerStub2);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "A 'ConstantExpression' with node type 'Constant' cannot be handled by the IExpressionTransformer<BinaryExpression>. "
        + "The transformer was probably registered for a wrong ExpressionType.")]
    public void Register_AddsExpressionTypeCheck ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub = CreateTransformerStub<BinaryExpression> (1);

      _registry.Register (transformerStub, expression.NodeType);

      var result = _registry.GetTransformations (expression).ToArray ();

      result[0] (expression);
    }

    private IExpressionTransformer<T> CreateTransformerStub<T> (int id) 
        where T : Expression
    {
      var transformationResult = CreateSimpleExpression (id);
      var transformerStub = MockRepository.GenerateStub<IExpressionTransformer<T>>();
      transformerStub.Stub (stub => stub.Transform (Arg<T>.Is.Anything)).Return (transformationResult);
      return transformerStub;
    }

    private void CheckTransformationMatchesTransformer<T> (
        ExpressionTransformation transformation,
        IExpressionTransformer<T> expectedTransformer) 
        where T : Expression
    {
      var transformationResult = transformation (null);
      var expectedResult = expectedTransformer.Transform (null);
      Assert.That (transformationResult, Is.SameAs (expectedResult));
    }

    private Expression CreateSimpleExpression (int id)
    {
      return Expression.Constant (id);
    }
  }
}