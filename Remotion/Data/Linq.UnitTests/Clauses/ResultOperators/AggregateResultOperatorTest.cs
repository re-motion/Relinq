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
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class AggregateResultOperatorTest
  {
    private IQuerySource _querySource;
    private QuerySourceReferenceExpression _sourceExpression;

    private AggregateResultOperator _resultOperator;
    private LambdaExpression _func;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateMainFromClause_Int ();
      _sourceExpression = new QuerySourceReferenceExpression (_querySource);

      var originalFunc = ExpressionHelper.CreateLambdaExpression<int, int, int> ((total, i) => total + i);
      _func = Expression.Lambda (
          ReplacingExpressionTreeVisitor.Replace (originalFunc.Parameters[1], _sourceExpression, originalFunc.Body), 
          originalFunc.Parameters[0]);
      _resultOperator = new AggregateResultOperator (_func);
    }

    [Test]
    public void Func ()
    {
      var func = Expression.Lambda (typeof (Func<int, int>), Expression.Constant (0), Expression.Parameter (typeof (int), "i"));
      _resultOperator.Func = func;

      Assert.That (_resultOperator.Func, Is.SameAs (func));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<T,T>', but it is "
        + "'System.Reflection.MemberFilter'.\r\nParameter name: value")]
    public void Func_NonGeneric ()
    {
      var func = Expression.Lambda (
          typeof (MemberFilter),
          Expression.Constant (true),
          Expression.Parameter (typeof (MemberInfo), "m"),
          Expression.Parameter (typeof (object), "filterCriteria"));

      _resultOperator.Func = func;
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<T,T>', but it is "
        + "'System.Func`1[System.Boolean]'.\r\nParameter name: value")]
    public void Func_Generic_WrongDefinition ()
    {
      var func = Expression.Lambda (typeof (Func<bool>), Expression.Constant (true));

      _resultOperator.Func = func;
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage =
        "The aggregating function must be a LambdaExpression that describes an instantiation of 'Func<T,T>', but it is "
        + "'System.Func`2[System.Int32,System.Boolean]'.\r\nParameter name: value")]
    public void Func_Generic_WrongReturnType ()
    {
      var func = Expression.Lambda (typeof (Func<int, bool>), Expression.Constant (true), Expression.Parameter (typeof (int), "i"));

      _resultOperator.Func = func;
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var itemExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), itemExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "The input sequence must have items of type 'System.Int32', but it has "
        + "items of type 'Remotion.Data.Linq.UnitTests.TestDomain.Cook'.\r\nParameter name: inputInfo")]
    public void GetOutputDataInfo_InvalidInput_DoesntMatchItem ()
    {
      var input = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    public void ExecuteInMemory ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), _sourceExpression));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.EqualTo (10));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (AggregateResultOperator)));
      Assert.That (((AggregateResultOperator) clone).Func, Is.SameAs (_resultOperator.Func));
    }

    [Test]
    public void TransformExpressions ()
    {
      var newFunc = ExpressionHelper.CreateLambdaExpression<int, int> (total => total + 1);

      _resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (_func));
        return newFunc;
      });

      Assert.That (_resultOperator.Func, Is.SameAs (newFunc));
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_resultOperator.ToString (), Is.EqualTo ("Aggregate(total => (total + [main]))"));
    }

  }
}
