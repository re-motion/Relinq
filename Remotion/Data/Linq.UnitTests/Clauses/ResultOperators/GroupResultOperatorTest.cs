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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Parsing;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Clauses.ResultOperators
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
      _fromClause1 = ExpressionHelper.CreateMainFromClause ("i", typeof (int), ExpressionHelper.CreateIntQueryable ());
      _fromClause2 = ExpressionHelper.CreateMainFromClause ("j", typeof (int), ExpressionHelper.CreateIntQueryable ());

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

      Assert.That (result.DataInfo.ItemExpression, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) result.DataInfo.ItemExpression).ReferencedQuerySource, Is.SameAs (_resultOperator));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<IGrouping<int, string>>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.InstanceOfType (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) ((StreamedSequenceInfo) result).ItemExpression).ReferencedQuerySource,
          Is.SameAs (_resultOperator));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Student));
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
