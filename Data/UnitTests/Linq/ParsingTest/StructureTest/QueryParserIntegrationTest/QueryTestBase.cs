/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
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
  }
}
