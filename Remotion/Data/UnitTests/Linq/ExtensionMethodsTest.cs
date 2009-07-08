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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.UnitTests.Linq.Parsing;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class ExtensionMethodsTest
  {
    [Test]
    public void FetchMany ()
    {
      var query = ExpressionHelper.CreateQuerySource();
      Expression<Func<Student, IEnumerable<int>>> relatedObjectSelector = s => s.Scores;
      var fetchedQuery = query.FetchMany (relatedObjectSelector);

      Assert.That (fetchedQuery.Expression, Is.InstanceOfType (typeof (FetchManyExpression)));

      var fetchExpression = (FetchExpression) fetchedQuery.Expression;
      Assert.That (fetchExpression.Operand, Is.SameAs (query.Expression));
      Assert.That (fetchExpression.RelatedObjectSelector, Is.SameAs (relatedObjectSelector));
    }

    [Test]
    public void FetchOne ()
    {
      var query = ExpressionHelper.CreateQuerySource ();
      Expression<Func<Student, Student>> relatedObjectSelector = s => s.OtherStudent;
      var fetchedQuery = query.FetchOne (relatedObjectSelector);

      Assert.That (fetchedQuery.Expression, Is.InstanceOfType (typeof (FetchOneExpression)));

      var fetchExpression = (FetchExpression) fetchedQuery.Expression;
      Assert.That (fetchExpression.Operand, Is.SameAs (query.Expression));
      Assert.That (fetchExpression.RelatedObjectSelector, Is.SameAs (relatedObjectSelector));
    }

    [Test]
    public void Fetch_WithSelectDifferentFromQueryBase ()
    {
      var query = from sd in ExpressionHelper.CreateQuerySource_Detail ()
                  select sd.IndustrialSector;
      Expression<Func<IndustrialSector, IEnumerable<Student>>> relatedObjectSelector = i => i.Students;
      var fetchedQuery = query.FetchMany (relatedObjectSelector);

      Assert.That (fetchedQuery.Expression, Is.InstanceOfType (typeof (FetchExpression)));

      var fetchExpression = (FetchExpression) fetchedQuery.Expression;
      Assert.That (fetchExpression.Operand, Is.SameAs (query.Expression));
      Assert.That (fetchExpression.RelatedObjectSelector, Is.SameAs (relatedObjectSelector));
    }

    [Test]
    public void Fetch_MultipleTimes ()
    {
      var query = ExpressionHelper.CreateQuerySource ();
      var f1 = query.FetchMany (s => s.Scores);
      var f2 = f1.FetchMany (s => s.Friends);
      var f3 = query.FetchMany (s => s.Scores);

      Assert.That (f1.Expression, Is.InstanceOfType (typeof (FetchExpression)));
      var fetchExpression1 = (FetchExpression) f1.Expression;

      Assert.That (f2.Expression, Is.InstanceOfType (typeof (FetchExpression)));
      var fetchExpression2 = (FetchExpression) f2.Expression;

      Assert.That (f3.Expression, Is.InstanceOfType (typeof (FetchExpression)));
      var fetchExpression3 = (FetchExpression) f3.Expression;

      Assert.That (fetchExpression1.Operand, Is.SameAs (query.Expression));
      Assert.That (fetchExpression2.Operand, Is.SameAs (f1.Expression));
      Assert.That (fetchExpression3.Operand, Is.SameAs (query.Expression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument query.Provider has type System.Linq.EnumerableQuery`1[System.Int32] when " 
        + "type Remotion.Data.Linq.QueryProviderBase was expected.\r\nParameter name: query.Provider")]
    public void Fetch_NoQueryable ()
    {
      var query = new[] { 1, 2, 3 }.AsQueryable ();
      query.FetchMany (i => (int[]) null);
    }

    [Test]
    public void Fetch_Result ()
    {
      var query = ExpressionHelper.CreateQuerySource_IndustrialSector ();
      Expression<Func<IndustrialSector, IEnumerable<Student>>> relatedObjectSelector1 = i => i.Students;
      Expression<Func<Student, IEnumerable<Student>>> relatedObjectSelector2 = s => s.Friends;
      Expression<Func<Student, IEnumerable<int>>> relatedObjectSelector3 = s => s.Scores;

      var expressionTree = query
          .FetchMany (relatedObjectSelector1)
          .ThenFetchMany (relatedObjectSelector2)
          .ThenFetchMany (relatedObjectSelector3).Expression;

      var expectedFetchExpression = new FetchManyExpression (query.Expression, relatedObjectSelector1);
      var expectedThenFetchExpression1 = new ThenFetchManyExpression (expectedFetchExpression, relatedObjectSelector2);
      var expectedThenFetchExpression2 = new ThenFetchManyExpression (expectedThenFetchExpression1, relatedObjectSelector3);

      ExpressionTreeComparer.CheckAreEqualTrees (expressionTree, expectedThenFetchExpression2);
    }
  }
}