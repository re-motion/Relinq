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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
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
      object items = new[] { 1, 2, 3, 4};
      IExecuteInMemoryData input = new ExecuteInMemorySequenceData (items, Expression.Constant (0));
      var result = _resultOperator.ExecuteInMemory (input);

      Assert.That (result.GetCurrentSingleValue<bool>(), Is.True);
    }

    [Test]
    public void ExecutionStrategy ()
    {
      Assert.That (_resultOperator.ExecutionStrategy, Is.SameAs (ScalarExecutionStrategy.Instance));
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
      var resultOperator = new ContainsResultOperator (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ()));
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
    public void GetResultType ()
    {
      Assert.That (_resultOperator.GetResultType (typeof (IQueryable<int>)), Is.SameAs (typeof (bool)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetResultType_InvalidInput ()
    {
      _resultOperator.GetResultType (typeof (Student));
    }
  }
}