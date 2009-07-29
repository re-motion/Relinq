// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class TakeResultOperatorTest
  {
    private TakeResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new TakeResultOperator (Expression.Constant (2));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_NoIntExpression ()
    {
      new TakeResultOperator (Expression.Constant ("12"));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (TakeResultOperator)));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      IEnumerable items = new[] { 1, 2, 3, 0, 2 };
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
      var result = _resultOperator.ExecuteInMemory<int> (input);

      Assert.That (result.GetTypedSequence<int>().ToArray (), Is.EqualTo (new[] { 1, 2 }));
    }

    [Test]
    public void GetConstantCount ()
    {
      Assert.That (_resultOperator.GetConstantCount (), Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void GetConstantCount_NoConstantExpression ()
    {
      var resultOperator = new TakeResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ()));
      resultOperator.GetConstantCount ();
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = Expression.Constant (0);
      var newExpression = Expression.Constant (1);
      var resultOperator = new TakeResultOperator (oldExpression);

      resultOperator.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (resultOperator.Count, Is.SameAs (newExpression));
    }
  }
}