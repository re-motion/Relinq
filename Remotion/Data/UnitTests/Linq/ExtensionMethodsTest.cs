// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// This framework is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this framework; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.Linq.ExtensionMethods;
using Remotion.Data.Linq.QueryProviderImplementation;
using Remotion.Data.UnitTests.Linq.ParsingTest;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class ExtensionMethodsTest
  {
    [Test]
    public void Fetch ()
    {
      var query = ExpressionHelper.CreateQuerySource();
      query.Fetch (s => s.Scores);
      var castQuery = (QueryableBase<Student>) query;

      Assert.That (castQuery.FetchRequests.Count(), Is.EqualTo (1));
      Expression<Func<Student, IEnumerable<int>>> expectedExpression = s => s.Scores;
      ExpressionTreeComparer.CheckAreEqualTrees (castQuery.FetchRequests.First().RelatedObjectSelector, expectedExpression);
    }

    [Test]
    public void Fetch_WithSelectDifferentFromQueryBase ()
    {
      var query = from sd in ExpressionHelper.CreateQuerySource_Detail ()
                  select sd.IndustrialSector;
      query.Fetch (i => i.Students);
      var castQuery = (QueryableBase<IndustrialSector>) query;

      Assert.That (castQuery.FetchRequests.Count (), Is.EqualTo (1));
      
      var fetchRequest = castQuery.FetchRequests.First ();
      Assert.That (fetchRequest, Is.InstanceOfType (typeof (FetchRequest<Student>)));

      Expression<Func<IndustrialSector, IEnumerable<Student>>> expectedExpression = i => i.Students;
      ExpressionTreeComparer.CheckAreEqualTrees (fetchRequest.RelatedObjectSelector, expectedExpression);
    }

    [Test]
    public void Fetch_MultipleTimes ()
    {
      var query = ExpressionHelper.CreateQuerySource ();
      query.Fetch (s => s.Scores);
      query.Fetch (s => s.Friends);
      query.Fetch (s => s.Scores);
      var castQuery = (QueryableBase<Student>) query;

      Assert.That (castQuery.FetchRequests.Count (), Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument query has type System.Linq.EnumerableQuery`1[System.Int32] when " 
        + "type Remotion.Data.Linq.QueryProviderImplementation.QueryableBase`1[System.Int32] was expected.\r\nParameter name: query")]
    public void Fetch_NoQueryable ()
    {
      var query = new[] { 1, 2, 3 }.AsQueryable ();
      query.Fetch (i => (int[]) null);
    }

    [Test]
    public void Fetch_Result ()
    {
      var query = ExpressionHelper.CreateQuerySource_IndustrialSector ();
      query
          .Fetch (i => i.Students)
          .Fetch (s => s.Friends)
          .Fetch (s => s.Scores);
      var castQuery = (QueryableBase<IndustrialSector>) query;

      var request1 = (FetchRequest<Student>) castQuery.FetchRequests.Single ();
      var request2 = (FetchRequest<Student>) request1.InnerFetchRequests.Single ();
      var request3 = (FetchRequest<int>) request2.InnerFetchRequests.Single ();

      Expression<Func<IndustrialSector, IEnumerable<Student>>> expectedExpression1 = i => i.Students;
      ExpressionTreeComparer.CheckAreEqualTrees (request1.RelatedObjectSelector, expectedExpression1);

      Expression<Func<Student, IEnumerable<Student>>> expectedExpression2 = s => s.Friends;
      ExpressionTreeComparer.CheckAreEqualTrees (request2.RelatedObjectSelector, expectedExpression2);

      Expression<Func<Student, IEnumerable<int>>> expectedExpression3 = s => s.Scores;
      ExpressionTreeComparer.CheckAreEqualTrees (request3.RelatedObjectSelector, expectedExpression3);

    }
  }
}