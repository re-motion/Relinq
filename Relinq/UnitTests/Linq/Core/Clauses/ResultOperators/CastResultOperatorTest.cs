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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators
{
  [TestFixture]
  public class CastResultOperatorTest
  {
    private CastResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new CastResultOperator (typeof (Chef));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOf (typeof (CastResultOperator)));
      Assert.That (((CastResultOperator) clone).CastItemType, Is.SameAs (_resultOperator.CastItemType));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var student1 = new Chef ();
      var student2 = new Chef ();
      IEnumerable items = new Cook[] { student1, student2 };
      var itemExpression = Expression.Constant (student1, typeof (Cook));
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (Cook[]), itemExpression));

      var result = _resultOperator.ExecuteInMemory<Cook> (input);

      var sequence = result.GetTypedSequence<Chef>();
      Assert.That (sequence.ToArray (), Is.EquivalentTo (new[] { student1, student2 }));
      Assert.That (result.DataInfo.ItemExpression.Type, Is.EqualTo (typeof (Chef)));
      Assert.That (((UnaryExpression) result.DataInfo.ItemExpression).Operand, Is.SameAs (itemExpression));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Cook());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<Chef>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.InstanceOf (typeof (UnaryExpression)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression.NodeType, Is.EqualTo (ExpressionType.Convert));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression.Type, Is.SameAs (typeof (Chef)));
      Assert.That (((UnaryExpression) ((StreamedSequenceInfo) result).ItemExpression).Operand, Is.SameAs (input.ItemExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      _resultOperator.GetOutputDataInfo (input);
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_resultOperator.ToString (), Is.EqualTo ("Cast<Remotion.Linq.UnitTests.Linq.Core.TestDomain.Chef>()"));
    }
  }
}
