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
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
{
  [TestFixture]
  public class AggregateFromSeedResultOperatorTest
  {
    private AggregateFromSeedResultOperator _resultOperatorWithoutResultSelector;
    private AggregateFromSeedResultOperator _resultOperatorWithResultSelector;

    private IQuerySource _querySource;
    private QuerySourceReferenceExpression _sourceExpression;

    private LambdaExpression _func;
    private Expression _seed;
    private LambdaExpression _resultSelector;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateMainFromClause_Int ();
      _sourceExpression = new QuerySourceReferenceExpression (_querySource);

      var originalFunc = ExpressionHelper.CreateLambdaExpression<int, int, int> ((total, i) => total + i);
      _func = Expression.Lambda (
          ReplacingExpressionTreeVisitor.Replace (originalFunc.Parameters[1], _sourceExpression, originalFunc.Body), 
          originalFunc.Parameters[0]);
      _resultSelector = ExpressionHelper.CreateLambdaExpression<int, string> (total => total.ToString ());

      _seed = Expression.Constant (12);
      _resultOperatorWithoutResultSelector = new AggregateFromSeedResultOperator (_seed, _func, null);
      _resultOperatorWithResultSelector = new AggregateFromSeedResultOperator (_seed, _func, _resultSelector);
    }

    [Test]
    public void Func ()
    {
      var func = Expression.Lambda (
          typeof (Func<int, int>),
          Expression.Constant (0),
          Expression.Parameter (typeof (int), "i"));
      _resultOperatorWithResultSelector.Func = func;

      Assert.That (_resultOperatorWithResultSelector.Func, Is.SameAs (func));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is "
        + "'System.Reflection.MemberFilter'.\r\nParameter name: value")]
    public void Func_NonGeneric ()
    {
      var func = Expression.Lambda (
          typeof (MemberFilter),
          Expression.Constant (true),
          Expression.Parameter (typeof (MemberInfo), "m"),
          Expression.Parameter (typeof (object), "filterCriteria"));

      _resultOperatorWithResultSelector.Func = func;
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is "
        + "'System.Func`1[System.Boolean]'.\r\nParameter name: value")]
    public void Func_Generic_WrongDefinition ()
    {
      var func = Expression.Lambda (typeof (Func<bool>), Expression.Constant (true));

      _resultOperatorWithResultSelector.Func = func;
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is "
        + "'System.Func`2[System.Int32,System.Boolean]'.\r\nParameter name: value")]
    public void Func_Generic_WrongReturnType ()
    {
      var func = Expression.Lambda (typeof (Func<int, bool>), Expression.Constant (true), Expression.Parameter (typeof (int), "i"));

      _resultOperatorWithResultSelector.Func = func;
    }

    [Test]
    public void GetConstantSeed ()
    {
      Assert.That (_resultOperatorWithResultSelector.GetConstantSeed<int> (), Is.EqualTo (12));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "The seed expression ('[main]') is no ConstantExpression, it is a QuerySourceReferenceExpression.")]
    public void GetConstantSeed_NoConstantExpression ()
    {
      var resultOperator = new AggregateFromSeedResultOperator (
          new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()),
          _func,
          _resultSelector);
      resultOperator.GetConstantSeed<object> ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "The value stored by the seed expression ('12') is not of type 'System.DateTime', it is of type 'System.Int32'.")]
    public void GetConstantSeed_NotExpectedType ()
    {
      _resultOperatorWithResultSelector.GetConstantSeed<DateTime> ();
    }

    [Test]
    public void ResultSelector ()
    {
      var resultSelector = Expression.Lambda (
          typeof (Func<int, string>),
          Expression.Constant ("0"),
          Expression.Parameter (typeof (int), "i"));
      _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector;

      Assert.That (_resultOperatorWithResultSelector.OptionalResultSelector, Is.SameAs (resultSelector));
    }

    [Test]
    public void ResultSelector_Null ()
    {
      _resultOperatorWithResultSelector.OptionalResultSelector = null;

      Assert.That (_resultOperatorWithResultSelector.OptionalResultSelector, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The result selector must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TResult>', but it is "
        + "'System.Reflection.MemberFilter'.\r\nParameter name: value")]
    public void ResultSelector_NonGeneric ()
    {
      var resultSelector = Expression.Lambda (
          typeof (MemberFilter),
          Expression.Constant (true),
          Expression.Parameter (typeof (MemberInfo), "m"),
          Expression.Parameter (typeof (object), "filterCriteria"));

      _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector;
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The result selector must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TResult>', but it is "
        + "'System.Func`1[System.Boolean]'.\r\nParameter name: value")]
    public void ResultSelector_Generic_WrongDefinition ()
    {
      var resultSelector = Expression.Lambda (typeof (Func<bool>), Expression.Constant (true));

      _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector;
    }

    [Test]
    public void GetOutputDataInfo_WithResultSelector ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperatorWithResultSelector.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (string)));
    }

    [Test]
    public void GetOutputDataInfo_WithoutResultSelector ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperatorWithoutResultSelector.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperatorWithResultSelector.GetOutputDataInfo (input);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The seed expression and the aggregating function don't have matching types. The seed is of type 'System.String', but the function aggregates "
        + "'System.Int32'.")]
    public void GetOutputDataInfo_NonMatchingFunc ()
    {
      _resultOperatorWithResultSelector.Seed = Expression.Constant ("0");

      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      _resultOperatorWithResultSelector.GetOutputDataInfo (input);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The aggregating function and the result selector don't have matching types. The function aggregates type 'System.Int32', but the "
        + "result selector takes 'System.String'.")]
    public void GetOutputDataInfo_NonMatchingResultSelector ()
    {
      var resultSelector = Expression.Lambda (
          typeof (Func<string, int>),
          Expression.Constant (0),
          Expression.Parameter (typeof (string), "i"));
      _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector;

      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      _resultOperatorWithResultSelector.GetOutputDataInfo (input);
    }

    [Test]
    public void ExecuteInMemory_WithResultSelector ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), _sourceExpression));
      var result = _resultOperatorWithResultSelector.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo ("22"));
    }

    [Test]
    public void ExecuteInMemory_WithoutResultSelector ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), _sourceExpression));
      var result = _resultOperatorWithoutResultSelector.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo (22));
    }

    [Test]
    public void Clone_WithResultSelector ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperatorWithResultSelector.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (AggregateFromSeedResultOperator)));
      Assert.That (((AggregateFromSeedResultOperator) clone).Seed, Is.SameAs (_resultOperatorWithResultSelector.Seed));
      Assert.That (((AggregateFromSeedResultOperator) clone).Func, Is.SameAs (_resultOperatorWithResultSelector.Func));
      Assert.That (((AggregateFromSeedResultOperator) clone).OptionalResultSelector, Is.SameAs (_resultOperatorWithResultSelector.OptionalResultSelector));
    }

    [Test]
    public void Clone_WithoutResultSelector ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperatorWithoutResultSelector.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (AggregateFromSeedResultOperator)));
      Assert.That (((AggregateFromSeedResultOperator) clone).Seed, Is.SameAs (_resultOperatorWithResultSelector.Seed));
      Assert.That (((AggregateFromSeedResultOperator) clone).Func, Is.SameAs (_resultOperatorWithResultSelector.Func));
      Assert.That (((AggregateFromSeedResultOperator) clone).OptionalResultSelector, Is.Null);
    }

    [Test]
    public void TransformExpressions_WithResultSelector ()
    {
      var newSeed = Expression.Constant (1);
      var newFunc = ExpressionHelper.CreateLambdaExpression<int, int> (total => total + 1);
      var newResultSelector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString () + "!");

      _resultOperatorWithResultSelector.TransformExpressions (ex =>
      {
        if (ex == _seed)
          return newSeed;
        else if (ex == _func)
          return newFunc;
        else
        {
          Assert.That (ex, Is.SameAs (_resultSelector), "Expected one of the expressions.");
          return newResultSelector;
        }
      });

      Assert.That (_resultOperatorWithResultSelector.Seed, Is.SameAs (newSeed));
      Assert.That (_resultOperatorWithResultSelector.Func, Is.SameAs (newFunc));
      Assert.That (_resultOperatorWithResultSelector.OptionalResultSelector, Is.SameAs (newResultSelector));
    }

    [Test]
    public void TransformExpressions_WithoutResultSelector ()
    {
      var newSeed = Expression.Constant (0);
      var newFunc = ExpressionHelper.CreateLambdaExpression<int, int> (total => total + 1);

      _resultOperatorWithoutResultSelector.TransformExpressions (ex =>
      {
        if (ex == _seed)
          return newSeed;
        else
        {
          Assert.That (ex, Is.SameAs (_func), "Expected one of the expressions.");
          return newFunc;
        }
      });

      Assert.That (_resultOperatorWithoutResultSelector.Seed, Is.SameAs (newSeed));
      Assert.That (_resultOperatorWithoutResultSelector.Func, Is.SameAs (newFunc));
      Assert.That (_resultOperatorWithoutResultSelector.OptionalResultSelector, Is.Null);
    }

    [Test]
    public void ToString_WithResultSelector ()
    {
      Assert.That (_resultOperatorWithResultSelector.ToString (), Is.EqualTo ("Aggregate(12, total => (total + [main]), total => total.ToString())"));
    }

    [Test]
    public void ToString_WithoutResultSelector ()
    {
      Assert.That (_resultOperatorWithoutResultSelector.ToString (), Is.EqualTo ("Aggregate(12, total => (total + [main]))"));
    }

  }
}
