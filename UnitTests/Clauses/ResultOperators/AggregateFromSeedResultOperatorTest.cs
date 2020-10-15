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
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
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
          ReplacingExpressionVisitor.Replace (originalFunc.Parameters[1], _sourceExpression, originalFunc.Body), 
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
    public void Func_NonGeneric ()
    {
      var func = Expression.Lambda (
          typeof (MemberFilter),
          Expression.Constant (true),
          Expression.Parameter (typeof (MemberInfo), "m"),
          Expression.Parameter (typeof (object), "filterCriteria"));
      Assert.That (
          () => _resultOperatorWithResultSelector.Func = func,
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is "
                  + "'System.Reflection.MemberFilter'.\r\nParameter name: value"));
    }

    [Test]
    public void Func_Generic_WrongDefinition ()
    {
      var func = Expression.Lambda (typeof (Func<bool>), Expression.Constant (true));
      Assert.That (
          () => _resultOperatorWithResultSelector.Func = func,
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is "
                  + "'System.Func`1[System.Boolean]'.\r\nParameter name: value"));
    }

    [Test]
    public void Func_Generic_WrongReturnType ()
    {
      var func = Expression.Lambda (typeof (Func<int, bool>), Expression.Constant (true), Expression.Parameter (typeof (int), "i"));
      Assert.That (
          () => _resultOperatorWithResultSelector.Func = func,
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TAccumulate>', but it is "
                  + "'System.Func`2[System.Int32,System.Boolean]'.\r\nParameter name: value"));
    }

    [Test]
    public void GetConstantSeed ()
    {
      Assert.That (_resultOperatorWithResultSelector.GetConstantSeed<int> (), Is.EqualTo (12));
    }

    [Test]
    public void GetConstantSeed_NoConstantExpression ()
    {
      var resultOperator = new AggregateFromSeedResultOperator (
          new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()),
          _func,
          _resultSelector);
      Assert.That (
          () => resultOperator.GetConstantSeed<object> (),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The seed expression ('[main]') is no ConstantExpression, it is a QuerySourceReferenceExpression.\r\nParameter name: expression"));
    }

    [Test]
    public void GetConstantSeed_NotExpectedType ()
    {
      Assert.That (
          () => _resultOperatorWithResultSelector.GetConstantSeed<DateTime> (),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The value stored by the seed expression ('12') is not of type 'System.DateTime', it is of type 'System.Int32'.\r\nParameter name: expression"));
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
    public void ResultSelector_NonGeneric ()
    {
      var resultSelector = Expression.Lambda (
          typeof (MemberFilter),
          Expression.Constant (true),
          Expression.Parameter (typeof (MemberInfo), "m"),
          Expression.Parameter (typeof (object), "filterCriteria"));
      Assert.That (
          () => _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector,
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The result selector must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TResult>', but it is "
                  + "'System.Reflection.MemberFilter'.\r\nParameter name: value"));
    }

    [Test]
    public void ResultSelector_Generic_WrongDefinition ()
    {
      var resultSelector = Expression.Lambda (typeof (Func<bool>), Expression.Constant (true));
      Assert.That (
          () => _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector,
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "The result selector must be a LambdaExpression that describes an instantiation of 'Func<TAccumulate,TResult>', but it is "
                  + "'System.Func`1[System.Boolean]'.\r\nParameter name: value"));
    }

    [Test]
    public void GetOutputDataInfo_WithResultSelector ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperatorWithResultSelector.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (string)));
    }

    [Test]
    public void GetOutputDataInfo_WithoutResultSelector ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperatorWithoutResultSelector.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void GetOutputDataInfo_WithDifferentAssignableSeedType ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);

      var seed = Expression.Constant ("string");
      var func = Expression.Lambda (
          Expression.Constant (null, typeof (object)), 
          Expression.Parameter (typeof (object), "acc"));
      var resultOperator = new AggregateFromSeedResultOperator (seed, func, null);

      var result = resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      Assert.That (
          () => _resultOperatorWithResultSelector.GetOutputDataInfo (input),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'inputInfo' has type 'Remotion.Linq.Clauses.StreamedData.StreamedScalarValueInfo' "
                  + "when type 'Remotion.Linq.Clauses.StreamedData.StreamedSequenceInfo' was expected."
                  + "\r\nParameter name: inputInfo"));
    }

    [Test]
    public void GetOutputDataInfo_NonMatchingFunc ()
    {
      _resultOperatorWithResultSelector.Seed = Expression.Constant ("0");

      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      Assert.That (
          () => _resultOperatorWithResultSelector.GetOutputDataInfo (input),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "The seed expression and the aggregating function don't have matching types. The seed is of type 'System.String', but the function aggregates "
                  + "'System.Int32'."));
    }

    [Test]
    public void GetOutputDataInfo_NonMatchingResultSelector ()
    {
      var resultSelector = Expression.Lambda (
          typeof (Func<string, int>),
          Expression.Constant (0),
          Expression.Parameter (typeof (string), "i"));
      _resultOperatorWithResultSelector.OptionalResultSelector = resultSelector;

      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      Assert.That (
          () => _resultOperatorWithResultSelector.GetOutputDataInfo (input),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "The aggregating function and the result selector don't have matching types. The function aggregates type 'System.Int32', but the "
                  + "result selector takes 'System.String'."));
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

      Assert.That (clone, Is.InstanceOf (typeof (AggregateFromSeedResultOperator)));
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

      Assert.That (clone, Is.InstanceOf (typeof (AggregateFromSeedResultOperator)));
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
