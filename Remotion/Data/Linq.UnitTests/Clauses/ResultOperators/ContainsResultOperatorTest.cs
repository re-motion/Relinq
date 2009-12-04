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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class ContainsResultOperatorTest
  {
    private ContainsResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new ContainsResultOperator (Expression.Constant (2));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (ContainsResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      IEnumerable items = new[] { 1, 2, 3, 4 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.Value, Is.True);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (0);
      var input = new StreamedSequenceInfo (typeof (int[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (bool)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Student));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "The input sequence must have items of type 'System.Int32', but it has "
        + "items of type 'Remotion.Data.Linq.UnitTests.TestDomain.Student'.\r\nParameter name: inputInfo")]
    public void GetOutputDataInfo_InvalidInput_DoesntMatchItem ()
    {
      var input = new StreamedSequenceInfo (typeof (Student[]), Expression.Constant (new Student()));
      _resultOperator.GetOutputDataInfo (input);
      Assert.Fail ("Expected exception.");
    }

    [Test]
    public void GetConstantItem ()
    {
      Assert.That (_resultOperator.GetConstantItem<int> (), Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "Item ('[main]') is no ConstantExpression, it is a QuerySourceReferenceExpression.")]
    public void GetConstantItem_NoConstantExpression ()
    {
      var resultOperator = new ContainsResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
      resultOperator.GetConstantItem<object> ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "The value stored by Item ('2') is not of type 'System.DateTime', it is of type 'System.Int32'.")]
    public void GetConstantItem_NotExpectedType ()
    {
      _resultOperator.GetConstantItem<DateTime> ();
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var resultOperator = new ContainsResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.Item, Is.SameAs (newExpression));
    }
  }
}
