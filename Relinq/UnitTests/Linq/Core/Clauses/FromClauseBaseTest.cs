// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses
{
  [TestFixture]
  public class FromClauseBaseTest
  {
    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var clause = new MainFromClause ("x", typeof (Cook), oldExpression);

      clause.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (clause.FromExpression, Is.SameAs (newExpression));
    }

    [Test]
    public new void ToString ()
    {
      var fromClause = new MainFromClause ("x", typeof (Cook), Expression.Constant (0));
      Assert.That (fromClause.ToString (), Is.EqualTo ("from Cook x in 0"));
    }

  }
}
