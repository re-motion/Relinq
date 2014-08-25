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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Parsing;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class GroupResultOperatorTest
  {
    private GroupResultOperator _resultOperator;
    private CloneContext _cloneContext;
    private MainFromClause _fromClause1;
    private MainFromClause _fromClause2;
    private Expression _keySelector;
    private Expression _elementSelector;

    [SetUp]
    public void SetUp ()
    {
      _fromClause1 = ExpressionHelper.CreateMainFromClause_Int ("i", typeof (int), ExpressionHelper.CreateIntQueryable ());
      _fromClause2 = ExpressionHelper.CreateMainFromClause_Int ("j", typeof (int), ExpressionHelper.CreateIntQueryable ());

      _keySelector = ExpressionHelper.Resolve<int, int> (_fromClause2, j => j % 3);
      _elementSelector = ExpressionHelper.Resolve<int, string> (_fromClause1, i => i.ToString ());
      _resultOperator = new GroupResultOperator ("groupings", _keySelector, _elementSelector);

      _cloneContext = new CloneContext (new QuerySourceMapping ());
    }

    [Test]
    public void ItemType ()
    {
      var expectedItemType = typeof (IGrouping<,>).MakeGenericType (
          _resultOperator.KeySelector.Type, 
          _resultOperator.ElementSelector.Type);

      Assert.That (_resultOperator.ItemType, Is.SameAs (expectedItemType));
    }

    [Test]
    public void Clone ()
    {
      var clone = (GroupResultOperator) _resultOperator.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_resultOperator));

      Assert.That (clone.KeySelector, Is.SameAs (_resultOperator.KeySelector));
      Assert.That (clone.ElementSelector, Is.SameAs (_resultOperator.ElementSelector));
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_resultOperator.ToString (), Is.EqualTo ("GroupBy(([j] % 3), [i].ToString())"));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      // group [i].ToString() by [j] % 3
      // expected input: new AnonymousType ( a = [i], b = [j] )

      var expectedInput = Expression.New (
        typeof (AnonymousType).GetConstructor (new[] {typeof (int), typeof (int) }), 
        new Expression[] { new QuerySourceReferenceExpression (_fromClause1), new QuerySourceReferenceExpression (_fromClause2) },
        new MemberInfo[] { typeof (AnonymousType).GetProperty ("a"), typeof (AnonymousType).GetProperty ("b") });

      var items = new[] { 
          new AnonymousType (111, 1), 
          new AnonymousType (222, 2), 
          new AnonymousType (333, 3), 
          new AnonymousType (444, 4), 
          new AnonymousType (555, 5) 
      };

      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (AnonymousType[]), expectedInput));

      var result = _resultOperator.ExecuteInMemory<AnonymousType> (input);
      var sequence = result.GetTypedSequence<IGrouping<int, string>> ();

      var resultArray = sequence.ToArray ();
      Assert.That (resultArray.Length, Is.EqualTo (3));
      Assert.That (resultArray[0].ToArray (), Is.EqualTo (new[] { "111", "444" }));
      Assert.That (resultArray[1].ToArray (), Is.EqualTo (new[] { "222", "555" }));
      Assert.That (resultArray[2].ToArray (), Is.EqualTo (new[] { "333" }));

      Assert.That (result.DataInfo.ItemExpression, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) result.DataInfo.ItemExpression).ReferencedQuerySource, Is.SameAs (_resultOperator));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var intExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), intExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<IGrouping<int, string>>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) ((StreamedSequenceInfo) result).ItemExpression).ReferencedQuerySource,
          Is.SameAs (_resultOperator));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Parameter 'inputInfo' has type 'Remotion.Linq.Clauses.StreamedData.StreamedScalarValueInfo' "
        + "when type 'Remotion.Linq.Clauses.StreamedData.StreamedSequenceInfo' was expected."
        + "\r\nParameter name: inputInfo")]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression1 = ExpressionHelper.CreateExpression ();
      var oldExpression2 = ExpressionHelper.CreateExpression ();
      var newExpression1 = ExpressionHelper.CreateExpression ();
      var newExpression2 = ExpressionHelper.CreateExpression ();
      var resultOperator = new GroupResultOperator ("x", oldExpression1, oldExpression2);

      resultOperator.TransformExpressions (ex =>
      {
        if (ex == oldExpression1)
          return newExpression1;
        else
        {
          Assert.That (ex, Is.SameAs (oldExpression2));
          return newExpression2;
        }
      });

      Assert.That (resultOperator.KeySelector, Is.SameAs (newExpression1));
      Assert.That (resultOperator.ElementSelector, Is.SameAs (newExpression2));
    }
  }
}
