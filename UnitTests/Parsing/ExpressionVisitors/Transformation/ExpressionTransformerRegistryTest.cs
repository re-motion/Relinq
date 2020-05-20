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
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.Transformation
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

      var equalTranformations = registry.GetAllTransformations (ExpressionType.Equal);
      var equalTransformers = GetTransformersFromTransformations (equalTranformations);
      Assert.That (equalTransformers, Has.Some.TypeOf (typeof (VBCompareStringExpressionTransformer)));

      var callTranformations = registry.GetAllTransformations (ExpressionType.Call);
      var callTransformers = GetTransformersFromTransformations (callTranformations);
      Assert.That (callTransformers, Has.Some.TypeOf (typeof (VBInformationIsNothingExpressionTransformer)));
      Assert.That (callTransformers, Has.Some.TypeOf (typeof (AttributeEvaluatingExpressionTransformer)));

      var invocationTranformations = registry.GetAllTransformations (ExpressionType.Invoke);
      var invocationTransformers = GetTransformersFromTransformations (invocationTranformations);
      Assert.That (invocationTransformers, Has.Some.TypeOf (typeof (InvocationOfLambdaExpressionTransformer)));

      var memberTranformations = registry.GetAllTransformations (ExpressionType.MemberAccess);
      var memberTransformers = GetTransformersFromTransformations (memberTranformations);
      Assert.That (memberTransformers, Has.Some.TypeOf (typeof (NullableValueTransformer)));
      Assert.That (memberTransformers, Has.Some.TypeOf (typeof (AttributeEvaluatingExpressionTransformer)));

      var newTranformations = registry.GetAllTransformations (ExpressionType.New);
      var newTransformers = GetTransformersFromTransformations (newTranformations);
      Assert.That (newTransformers, Has.Some.TypeOf (typeof (KeyValuePairNewExpressionTransformer)));
      Assert.That (newTransformers, Has.Some.TypeOf (typeof (DictionaryEntryNewExpressionTransformer)));
      Assert.That (newTransformers, Has.Some.TypeOf (typeof (TupleNewExpressionTransformer)));

      var memberAddingTransformers = newTransformers.OfType<MemberAddingNewExpressionTransformerBase>().ToArray();
      Assert.That (memberAddingTransformers.Length, Is.EqualTo (3));

      Assert.That (registry.RegisteredTransformerCount, Is.EqualTo (14));
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
    public void Register_CheckGenericHandlerType ()
    {
      var transformerStub = CreateTransformerStub<BinaryExpression> (1, null);
      Assert.That (
          () => _registry.Register (transformerStub),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Cannot register an IExpressionTransformer<BinaryExpression> as a generic transformer. Generic transformers must implement "
                  +"IExpressionTransformer<Expression>.\r\nParameter name: transformer"));
    }

    [Test]
    public void Register_AddsExpressionTypeCheck ()
    {
      var expression = CreateSimpleExpression (0);
      var transformerStub = CreateTransformerStub<BinaryExpression> (1, expression.NodeType);

      _registry.Register (transformerStub);

      var result = _registry.GetTransformations (expression).ToArray ();
      Assert.That (
          () => result[0] (expression),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "A 'ConstantExpression' with node type 'Constant' cannot be handled by the IExpressionTransformer<BinaryExpression>. "
                  + "The transformer was probably registered for a wrong ExpressionType."));
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