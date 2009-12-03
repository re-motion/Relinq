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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class OfTypeResultOperatorTest
  {
    private OfTypeResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new OfTypeResultOperator (typeof (GoodStudent));
    }

    [Test]
    public void Clone ()
    {
      var clonedClauseMapping = new QuerySourceMapping ();
      var cloneContext = new CloneContext (clonedClauseMapping);
      var clone = _resultOperator.Clone (cloneContext);

      Assert.That (clone, Is.InstanceOfType (typeof (OfTypeResultOperator)));
      Assert.That (((OfTypeResultOperator) clone).SearchedItemType, Is.SameAs (_resultOperator.SearchedItemType));
    }

    [Test]
    public void ExecuteInMemory ()
    {
      var student1 = new GoodStudent ();
      var student2 = new GoodStudent ();
      var student3 = new Student ();
      IEnumerable items = new Student[] { student1, student2, student3 };
      var itemExpression = Expression.Constant (student3, typeof (Student));
      var input = new StreamedSequence (items, new StreamedSequenceInfo (typeof (Student[]), itemExpression));

      var result = _resultOperator.ExecuteInMemory<Student> (input);

      var sequence = result.GetTypedSequence<GoodStudent> ();
      Assert.That (sequence.ToArray (), Is.EquivalentTo (new[] { student1, student2 }));
      Assert.That (result.DataInfo.ItemExpression.Type, Is.EqualTo (typeof (GoodStudent)));
      Assert.That (((UnaryExpression) result.DataInfo.ItemExpression).Operand, Is.SameAs (itemExpression));
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Student ());
      var input = new StreamedSequenceInfo (typeof (Student[]), studentExpression);
      var result = _resultOperator.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOfType (typeof (StreamedSequenceInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (IQueryable<GoodStudent>)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression, Is.InstanceOfType (typeof (UnaryExpression)));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression.NodeType, Is.EqualTo (ExpressionType.Convert));
      Assert.That (((StreamedSequenceInfo) result).ItemExpression.Type, Is.SameAs (typeof (GoodStudent)));
      Assert.That (((UnaryExpression) ((StreamedSequenceInfo) result).ItemExpression).Operand, Is.SameAs (input.ItemExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Student));
      _resultOperator.GetOutputDataInfo (input);
    }
  }
}
