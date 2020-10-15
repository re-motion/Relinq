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
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;

namespace Remotion.Linq.UnitTests.Clauses
{
  [TestFixture]
  public class QuerySourceMappingTest
  {
    private QuerySourceMapping _mapping;
    private MainFromClause _clause1;
    private MainFromClause _clause2;
    private MainFromClause _clause3;

    private QuerySourceReferenceExpression _querySourceReferenceExpression1;
    private QuerySourceReferenceExpression _querySourceReferenceExpression2;
    private QuerySourceReferenceExpression _querySourceReferenceExpression3;

    [SetUp]
    public void SetUp ()
    {
      _mapping = new QuerySourceMapping ();
      _clause1 = ExpressionHelper.CreateMainFromClause_Int ();
      _clause2 = ExpressionHelper.CreateMainFromClause_Int ();
      _clause3 = ExpressionHelper.CreateMainFromClause_Int ();

      _querySourceReferenceExpression1 = new QuerySourceReferenceExpression (_clause1);
      _querySourceReferenceExpression2 = new QuerySourceReferenceExpression (_clause2);
      _querySourceReferenceExpression3 = new QuerySourceReferenceExpression (_clause3);
    }

    [Test]
    public void AddMapping ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression1);
      Assert.That (_mapping.GetExpression (_clause1), Is.SameAs (_querySourceReferenceExpression1));
    }

    [Test]
    public void AddMapping_Twice ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression1);
      Assert.That (
          () => _mapping.AddMapping (_clause1, _querySourceReferenceExpression2),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("Query source (from Int32 main in TestQueryable<Int32>()) has already been associated with an expression."));
    }

    [Test]
    public void ReplaceMapping ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression2);
      _mapping.ReplaceMapping (_clause1, _querySourceReferenceExpression3);

      Assert.That (_mapping.GetExpression (_clause1), Is.SameAs (_querySourceReferenceExpression3));
    }

    [Test]
    public void ReplaceMapping_WithoutAdding ()
    {
      Assert.That (
          () => _mapping.ReplaceMapping (_clause1, _querySourceReferenceExpression2),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Query source (from Int32 main in TestQueryable<Int32>()) has not been associated with an expression, cannot replace its mapping."));
    }

    [Test]
    public void RemoveMapping ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression2);
      _mapping.RemoveMapping (_clause1);

       Assert.That (_mapping.ContainsMapping (_clause1), Is.False);
    }

    [Test]
    public void RemoveMapping_WithoutAdding ()
    {
      Assert.That (
          () => _mapping.RemoveMapping (_clause1),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Query source (from Int32 main in TestQueryable<Int32>()) has not been associated with an expression, cannot remove its mapping."));
    }

    [Test]
    public void ContainsMapping_True ()
    {
      _mapping.AddMapping (_clause1, _querySourceReferenceExpression2);
      Assert.That (_mapping.ContainsMapping (_clause1), Is.True);
    }

    [Test]
    public void ContainsMapping_False ()
    {
      Assert.That (_mapping.ContainsMapping (_clause1), Is.False);
    }

    [Test]
    public void GetExpression_WithoutAssociatedClause ()
    {
      Assert.That (
          () => _mapping.GetExpression (_clause1),
          Throws.InstanceOf<KeyNotFoundException>()
              .With.Message.EqualTo ("Query source (from Int32 main in TestQueryable<Int32>()) has not been associated with an expression."));
    }
  }
}