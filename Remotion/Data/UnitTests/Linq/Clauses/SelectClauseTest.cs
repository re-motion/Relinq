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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class SelectClauseTest
  {
    private Expression _selector;
    private SelectClause _selectClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _selector = ExpressionHelper.CreateExpression();
      _selectClause = new SelectClause (_selector);
      _cloneContext = new CloneContext (new ClauseMapping());
    }

    [Test]
    public void InitializeWithExpression ()
    {
      Assert.That (_selectClause.Selector, Is.EqualTo (_selector));
    }

    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      Assert.That (_selectClause, Is.InstanceOfType (typeof (ISelectGroupClause)));
    }
        
    [Test]
    public void Accept()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryModelVisitor>();
      visitorMock.VisitSelectClause (_selectClause, queryModel);
      repository.ReplayAll();
      _selectClause.Accept (visitorMock, queryModel);
      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var clone = _selectClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_selectClause));
      Assert.That (clone.Selector, Is.SameAs (_selectClause.Selector));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      var fromClause = ExpressionHelper.CreateMainFromClause ();
      _selectClause.Selector = new QuerySourceReferenceExpression (fromClause);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      _cloneContext.ClauseMapping.AddMapping (fromClause, newReferenceExpression);

      var clone = ((ISelectGroupClause) _selectClause).Clone (_cloneContext);
      Assert.That (((SelectClause) clone).Selector, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var oldReferencedExpression = ExpressionHelper.CreateMainFromClause();
      var selector = new QuerySourceReferenceExpression (oldReferencedExpression);
      var selectClause = new SelectClause (selector);

      var newReferencedExpression = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClauseMapping.AddMapping (oldReferencedExpression, new QuerySourceReferenceExpression (newReferencedExpression));

      var clone = selectClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.Selector).ReferencedClause, Is.SameAs (newReferencedExpression));
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var clause = new SelectClause (oldExpression);

      clause.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (clause.Selector, Is.SameAs (newExpression));
    }

    [Test]
    public void GetExecutionStrategy ()
    {
      Assert.That (_selectClause.GetExecutionStrategy (), Is.SameAs (CollectionExecutionStrategy.Instance));
    }

    [Test]
    public new void ToString ()
    {
      var selectClause = new SelectClause (Expression.Constant (0));
      Assert.That (selectClause.ToString (), Is.EqualTo ("select 0"));
    }
  }
}
