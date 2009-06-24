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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details
{
  [TestFixture]
  public class OrderingFieldParserTest
  {
    private JoinedTableContext _joinedTableContext;
    private OrderingFieldParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _joinedTableContext = new JoinedTableContext();
      _parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
    }

    [Test]
    public void SimpleOrderingClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      var ordering = orderBy.Orderings.First ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      var result = _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);

      var expectedFieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.That (result, Is.EqualTo (new OrderingField (expectedFieldDescriptor, OrderingDirection.Asc)));
    }

    [Test]
    public void TwoOrderingClause_FirstClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      var ordering = orderBy.Orderings.First ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      var result = _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);

      var expectedFieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.That (result, Is.EqualTo (new OrderingField (expectedFieldDescriptor, OrderingDirection.Asc)));
    }

    [Test]
    public void TwoOrderingClause_SecondClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.Last ();
      var ordering = orderBy.Orderings.Last ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      var result = _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);

      FieldDescriptor expectedFieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));
      Assert.That (result, Is.EqualTo (new OrderingField (expectedFieldDescriptor, OrderingDirection.Desc)));
    }

    [Test]
    public void ComplexOrderingClause_FirstOrdering ()
    {
      IQueryable<Student> query =
          MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.Skip (2).First ();
      var ordering = orderBy.Orderings.First ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      var result = _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);

      FieldDescriptor expectedFieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      Assert.That (result, Is.EqualTo (new OrderingField (expectedFieldDescriptor, OrderingDirection.Asc)));
    }

    [Test]
    public void ComplexOrderingClause_SecondOrdering ()
    {
      IQueryable<Student> query =
          MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.Skip (2).First ();
      var ordering = orderBy.Orderings.Last ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      var result = _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);

      FieldDescriptor expectedFieldDescriptor = ExpressionHelper.CreateFieldDescriptor ((FromClauseBase) parsedQuery.BodyClauses[0], typeof (Student).GetProperty ("Last"));
      Assert.That (result, Is.EqualTo (new OrderingField (expectedFieldDescriptor, OrderingDirection.Desc)));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.NonDBProperty' "
        + "does not identify a queryable column.")]
    public void OrderingClause_WithNonDBField ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateOrderByNonDBPropertyQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      var ordering = orderBy.Orderings.First ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);
    }

    [Test]
    public void JoinOrderingClause ()
    {
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      var ordering = orderBy.Orderings.First ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _joinedTableContext);
      var result = _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);

      FromClauseBase fromClause = parsedQuery.MainFromClause;
      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource sourceTable = fromClause.GetColumnSource (StubDatabaseInfo.Instance); // Student_Detail
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);

      PropertyInfo orderingMember = typeof (Student).GetProperty ("First");
      var join = new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));
      var path = new FieldSourcePath (sourceTable, new[] { join });
      var column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, relatedTable, orderingMember);
      var expectedFieldDescriptor = new FieldDescriptor (orderingMember, path, column);
      
      Assert.That (result, Is.EqualTo (new OrderingField (expectedFieldDescriptor, OrderingDirection.Asc)));
    }

    [Test]
    public void ParserUsesContext()
    {
      Assert.That (_joinedTableContext.Count, Is.EqualTo (0));
      JoinOrderingClause();
      Assert.That (_joinedTableContext.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Remotion.Data.UnitTests.Linq.Student_Detail.Student' "
        + "is not supported because it is a relation member.")]
    public void OrderingOnRelationMemberThrows()
    {
      IQueryable<Student_Detail> query = OrderByTestQueryGenerator.CreateRelationMemberOrderByQuery (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      var ordering = orderBy.Orderings.First ();

      var parseContext = new ParseContext (parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      _parser.Parse (ordering.Expression, parseContext, ordering.OrderingDirection);
    }
  }
}
