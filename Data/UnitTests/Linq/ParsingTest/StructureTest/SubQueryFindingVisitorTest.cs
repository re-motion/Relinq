// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
{
  [TestFixture]
  public class SubQueryFindingVisitorTest
  {
    private SubQueryFindingVisitor _visitor;
    private List<QueryModel> _subQueryRegistry;

    [SetUp]
    public void SetUp ()
    {
      _subQueryRegistry = new List<QueryModel>();
      _visitor = new SubQueryFindingVisitor (_subQueryRegistry);
    }

    [Test]
    public void TreeWithNoSubquery()
    {
      Expression expression = Expression.Constant ("test");

      Expression newExpression = _visitor.ReplaceSubQueries (expression);
      Assert.That (newExpression, Is.SameAs (expression));
    }

    [Test]
    public void TreeWithSubquery ()
    {
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      Expression newExpression = _visitor.ReplaceSubQueries (surroundingExpression);

      Assert.That (newExpression, Is.Not.SameAs (surroundingExpression));
      Assert.That (newExpression, Is.InstanceOfType (typeof (LambdaExpression)));

      LambdaExpression newLambdaExpression = (LambdaExpression) newExpression;
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));

      SubQueryExpression newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (newSubQueryExpression.QueryModel.GetExpressionTree (), Is.SameAs (subQuery));
    }

    [Test]
    public void SubqueryIsRegistered ()
    {
      Assert.That (_subQueryRegistry, Is.Empty);
      
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      LambdaExpression newLambdaExpression = (LambdaExpression) _visitor.ReplaceSubQueries (surroundingExpression);
      SubQueryExpression newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (_subQueryRegistry, Is.EquivalentTo (new[] { newSubQueryExpression.QueryModel }));
    }
  }
}
