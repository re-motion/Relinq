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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  public abstract class QueryTestBase<T>
  {
    public Expression SourceExpression { get; private set; }
    public ExpressionTreeNavigator SourceExpressionNavigator { get; private set; }
    public QueryModel ParsedQuery { get; private set; }
    public IQueryable<Student> QuerySource { get; private set; }
    public ConstantExpression QuerySourceExpression { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
      QuerySource = ExpressionHelper.CreateQuerySource();
      QuerySourceExpression = Expression.Constant (QuerySource);
      SourceExpression = CreateQuery().Expression;
      SourceExpressionNavigator = new ExpressionTreeNavigator (SourceExpression);
      QueryParser parser = new QueryParser (SourceExpression);
      ParsedQuery = parser.GetParsedQuery ();
    }

    protected abstract IQueryable<T> CreateQuery ();

    [Test]
    public void ParseResultIsNotNull()
    {
      Assert.IsNotNull (ParsedQuery);
    }

    [Test]
    public virtual void CheckMainFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.MainFromClause);
      Assert.AreEqual ("s", ParsedQuery.MainFromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.MainFromClause.Identifier.Type);
      ExpressionTreeComparer.CheckAreEqualTrees (QuerySourceExpression, ParsedQuery.MainFromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.MainFromClause.JoinClauses.Count);
    }
    
    [Test]
    public virtual void OutputResult ()
    {
      Console.WriteLine (ParsedQuery);
    }

    [Test]
    public void TranslateBack()
    {
      Expression builtExpressionTree = ParsedQuery.GetExpressionTree();
      ExpressionTreeComparer.CheckAreEqualTrees (builtExpressionTree, SourceExpression);
    }

    public abstract void CheckBodyClauses ();
    public abstract void CheckSelectOrGroupClause ();

    protected void AssertEquivalent (Expression left, Expression right)
    {
      if (left == right)
        return;

      var simplifiedLeft = ParseResultCollector.Simplify (left, new List<QueryModel> ());
      var simplifiedRight = ParseResultCollector.Simplify (right, new List<QueryModel> ());
      ExpressionTreeComparer.CheckAreEqualTrees (simplifiedLeft, simplifiedRight);
    }
  }
}
