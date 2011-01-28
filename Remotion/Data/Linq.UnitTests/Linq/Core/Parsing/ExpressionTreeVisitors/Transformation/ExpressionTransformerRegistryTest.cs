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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using System.Linq;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
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

      Assert.That (registry.RegisteredTransformerCount, Is.EqualTo (9));
      
      var equalTranformations = registry.GetAllTransformations (ExpressionType.Equal);
      var equalTransformers = GetTransformersFromTransformations (equalTranformations);
      Assert.That (equalTransformers, List.Some.TypeOf (typeof (VBCompareStringExpressionTransformer)));

      var callTranformations = registry.GetAllTransformations (ExpressionType.Call);
      var callTransformers = GetTransformersFromTransformations (callTranformations);
      Assert.That (callTransformers, List.Some.TypeOf (typeof (VBInformationIsNothingExpressionTransformer)));

      var invocationTranformations = registry.GetAllTransformations (ExpressionType.Invoke);
      var invocationTransformers = GetTransformersFromTransformations (invocationTranformations);
      Assert.That (invocationTransformers, List.Some.TypeOf (typeof (InvocationOfLambdaExpressionTransformer)));

      var nullableTranformations = registry.GetAllTransformations (ExpressionType.MemberAccess);
      var nullableTransformers = GetTransformersFromTransformations (nullableTranformations);
      Assert.That (nullableTransformers, List.Some.TypeOf (typeof (NullableValueTransformer)));
    }

    [Test]
    public void RegisteredTransformerCount ()
    {
      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (0));

      var transformerStub = CreateTransformerStub<Expression> (1, ExpressionType.Constant);
      _registry.Register (transformerStub);

      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (1));

      _registry.Register (transformerStub);

      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (2));

      var transformerStub2 = CreateTransformerStub<Expression> (2, ExpressionType.Equal);
      _registry.Register (transformerStub2);

      Assert.That (_registry.RegisteredTransformerCount, Is.EqualTo (3));
    }

    [Test]
    public void GetAllTransformations ()
    {
      Assert.That (_registry.GetAllTransformations (ExpressionType.Constant), Is.Empty);

      var transformerStub1 = CreateTransformerStub<Expression> (1, ExpressionType.Constant);
      _registry.Register (transformerStub1);

      var transformerStub2 = CreateTransformerStub<Expression> (1, ExpressionType.Constant);
      _registry.Register (transformerStub2);

      var transformerStub3 = CreateTransformerStub<Expression> (1, ExpressionType.Equal);
      _registry.Register (transformerStub3);

      var transformationsForConstant = _registry.GetAllTransformations (ExpressionType.Constant);
      Assert.That (GetTransformersFromTransformations (transformationsForConstant), Is.EqualTo (new[] { transformerStub1, transformerStub2 }));

      var transformationsForEqual = _registry.GetAllTransformations (ExpressionType.Equal);
      Assert.That (GetTransformersFromTransformations (transformationsForEqual), Is.EqualTo (new[] { transformerStub3 }));
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
      var transformerStub = CreateTransformerStub<Expression> (1, expression.NodeType);
      
      _registry.Register (transformerStub);

      var result = _registry.GetTransformations (expression).ToArray();

      Assert.That (result.Length, Is.EqualTo (1));
      CheckTransformationMatchesTransformer (result[0], transformerStub);
    }

    [Test]
    public void GetTransformers_UsesNodeTypeToMatchTransformers ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub = CreateTransformerStub<Expression> (1, ExpressionType.OrElse);

      _registry.Register (transformerStub);

      var result = _registry.GetTransformations (expression).ToArray ();

      Assert.That (result, Is.Empty);
    }

    [Test]
    public void GetTransformers_TwoTransformerRegistered ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub1 = CreateTransformerStub<Expression> (1, expression.NodeType);
      var transformerStub2 = CreateTransformerStub<Expression> (2, expression.NodeType);

      _registry.Register (transformerStub1);
      _registry.Register (transformerStub2);
      
      var result = _registry.GetTransformations (expression).ToArray ();

      Assert.That (result.Length, Is.EqualTo (2));
      CheckTransformationMatchesTransformer (result[0], transformerStub1);
      CheckTransformationMatchesTransformer (result[1], transformerStub2);
    }

    [Test]
    public void GetTransformers_TransformerRegisteredForTwoExpressionTypes ()
    {
      var expression1 = CreateSimpleExpression (0);
      var expression2 = Expression.UnaryPlus (CreateSimpleExpression (1));
      
      var transformerStub = CreateTransformerStub<Expression> (1, expression1.NodeType, expression2.NodeType);

      _registry.Register (transformerStub);

      var result1 = _registry.GetTransformations (expression1).ToArray ();

      Assert.That (result1.Length, Is.EqualTo (1));
      CheckTransformationMatchesTransformer (result1[0], transformerStub);

      var result2 = _registry.GetTransformations (expression2).ToArray ();

      Assert.That (result2.Length, Is.EqualTo (1));
      CheckTransformationMatchesTransformer (result2[0], transformerStub);
    }

    [Test]
    public void GetTransformers_OnlyGenericTransformersRegistered ()
    {
      var expression1 = CreateSimpleExpression (0);
      var expression2 = Expression.UnaryPlus (CreateSimpleExpression (1));

      var transformerStub1 = CreateTransformerStub<Expression> (2, null);
      var transformerStub2 = CreateTransformerStub<Expression> (3, null);

      _registry.Register (transformerStub1);
      _registry.Register (transformerStub2);

      var result1 = _registry.GetTransformations (expression1).ToArray ();
      var result2 = _registry.GetTransformations (expression2).ToArray ();
      
      Assert.That (result1.Length, Is.EqualTo (2));
      CheckTransformationMatchesTransformer (result1[0], transformerStub1);
      CheckTransformationMatchesTransformer (result1[1], transformerStub2);
      Assert.That (result2.Length, Is.EqualTo (2));
      CheckTransformationMatchesTransformer (result2[0], transformerStub1);
      CheckTransformationMatchesTransformer (result2[1], transformerStub2);
    }

    [Test]
    public void GetTransformers_GenericAndSpecificTransformersRegistered ()
    {
      var expression1 = CreateSimpleExpression (0);
      var expression2 = Expression.UnaryPlus (CreateSimpleExpression (1));

      var transformerStub1 = CreateTransformerStub<Expression> (2, null);
      var transformerStub2 = CreateTransformerStub<Expression> (3, null);
      var transformerStub3 = CreateTransformerStub<Expression> (3, ExpressionType.Constant);

      _registry.Register (transformerStub1);
      _registry.Register (transformerStub2);
      _registry.Register (transformerStub3);

      var result1 = _registry.GetTransformations (expression1).ToArray ();
      var result2 = _registry.GetTransformations (expression2).ToArray ();

      Assert.That (result1.Length, Is.EqualTo (3));
      CheckTransformationMatchesTransformer (result1[0], transformerStub3);
      CheckTransformationMatchesTransformer (result1[1], transformerStub1);
      CheckTransformationMatchesTransformer (result1[2], transformerStub2);

      Assert.That (result2.Length, Is.EqualTo (2));
      CheckTransformationMatchesTransformer (result2[0], transformerStub1);
      CheckTransformationMatchesTransformer (result2[1], transformerStub2);
    }

    [Test]
    public void GetTransformers_GenericTransformerRegistered_WithCustomExpressionType ()
    {
      var expression = new UnknownExpression (typeof (int));

      var transformerStub = CreateTransformerStub<Expression> (2, null);

      _registry.Register (transformerStub);

      var result1 = _registry.GetTransformations (expression).ToArray ();

      Assert.That (result1.Length, Is.EqualTo (1));
      CheckTransformationMatchesTransformer (result1[0], transformerStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
      "Cannot register an IExpressionTransformer<BinaryExpression> as a generic transformer. Generic transformers must implement "
      +"IExpressionTransformer<Expression>.\r\nParameter name: transformer")]
    public void Register_CheckGenericHandlerType ()
    {
      var transformerStub = CreateTransformerStub<BinaryExpression> (1, null);

      _registry.Register (transformerStub);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "A 'ConstantExpression' with node type 'Constant' cannot be handled by the IExpressionTransformer<BinaryExpression>. "
        + "The transformer was probably registered for a wrong ExpressionType.")]
    public void Register_AddsExpressionTypeCheck ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub = CreateTransformerStub<BinaryExpression> (1, expression.NodeType);

      _registry.Register (transformerStub);

      var result = _registry.GetTransformations (expression).ToArray ();

      result[0] (expression);
    }

    private IExpressionTransformer<T> CreateTransformerStub<T> (int id, params ExpressionType[] expressionTypes) 
        where T : Expression
    {
      var transformationResult = CreateSimpleExpression (id);
      var transformerStub = MockRepository.GenerateStub<IExpressionTransformer<T>>();
      transformerStub.Stub (stub => stub.Transform (Arg<T>.Is.Anything)).Return (transformationResult);
      transformerStub.Stub (stub => stub.SupportedExpressionTypes).Return (expressionTypes);
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

    private object[] GetTransformersFromTransformations (IEnumerable<ExpressionTransformation> transformations)
    {
      var targets = transformations.Select (t => t.Target);
      return targets.Select (t => t.GetType ().GetField ("transformer").GetValue (t)).ToArray ();
    }
  }
}