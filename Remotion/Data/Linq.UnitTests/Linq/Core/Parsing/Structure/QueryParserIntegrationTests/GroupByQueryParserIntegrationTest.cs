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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class GroupByQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void GroupBy ()
    {
      var query = (from s in QuerySource group s.ID by s.IsStarredCook);
      
      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, mainFromClause, s => s);

      var groupResultOperator = (GroupResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, bool> (groupResultOperator.KeySelector, mainFromClause, s => s.IsStarredCook);
      CheckResolvedExpression<Cook, int> (groupResultOperator.ElementSelector, mainFromClause, s => s.ID);
    }

    [Test]
    public void GroupByWithoutElementSelector ()
    {
      var query = QuerySource.GroupBy (s => s.IsStarredCook);
      
      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Cook>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, mainFromClause, s => s);

      var groupResultOperator = (GroupResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, bool> (groupResultOperator.KeySelector, mainFromClause, s => s.IsStarredCook);
      CheckResolvedExpression<Cook, Cook> (groupResultOperator.ElementSelector, mainFromClause, s => s);
    }

    [Test]
    public void GroupIntoWithAggregate ()
    {
      var query = from s in QuerySource 
                  group s.ID by s.IsStarredCook 
                  into x 
                      where x.Count() > 0
                      select x;

      // equivalent to:
      //var query2 = from x in
      //               (from s in _querySource
      //                group s.ID by s.IsStarredCook)
      //             where x.Count () > 0
      //             select x;

      // parsed as:
      //var query2 = from x in
      //               (from s in _querySource
      //                group s.ID by s.IsStarredCook)
      //             where (from generated in x select generated).Count () > 0
      //             select x;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, int>)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      var subQuerySelectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (subQuerySelectClause.Selector, mainFromClause, s => s);
      
      var subQueryGroupResultOperator = (GroupResultOperator) subQueryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, bool> (subQueryGroupResultOperator.KeySelector, subQueryModel.MainFromClause, s => s.IsStarredCook);
      CheckResolvedExpression<Cook, int> (subQueryGroupResultOperator.ElementSelector, subQueryModel.MainFromClause, s => s.ID);
      
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      Assert.That (whereClause.Predicate, Is.InstanceOfType (typeof (BinaryExpression)));
      var predicateLeftSide = ((BinaryExpression) whereClause.Predicate).Left;
      Assert.That (predicateLeftSide, Is.InstanceOfType (typeof (SubQueryExpression)));
      var predicateSubQueryModel = ((SubQueryExpression) predicateLeftSide).QueryModel;
      Assert.That (predicateSubQueryModel.MainFromClause.ItemType, Is.SameAs (typeof (int)));
      Assert.That (predicateSubQueryModel.MainFromClause.ItemName, NUnit.Framework.SyntaxHelpers.Text.StartsWith ("<generated>"));
      Assert.That (((QuerySourceReferenceExpression) predicateSubQueryModel.MainFromClause.FromExpression).ReferencedQuerySource, Is.SameAs (mainFromClause));
      Assert.That (((QuerySourceReferenceExpression) predicateSubQueryModel.SelectClause.Selector).ReferencedQuerySource, 
                   Is.SameAs (predicateSubQueryModel.MainFromClause));
      Assert.That (predicateSubQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (CountResultOperator)));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void GroupByFollowedByWhere ()
    {
      var query = (from s in ExpressionHelper.CreateCookQueryable ()
                   group s by s.IsStarredCook).Where (g => g.Key);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Cook>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, Cook>)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("g"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      var subQuerySelectClause = subQueryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (subQuerySelectClause.Selector, subQueryModel.MainFromClause, s => s);

      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOfType (typeof (GroupResultOperator)));

      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Cook>>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<IGrouping<bool, Cook>, bool> (whereClause.Predicate, mainFromClause, g => g.Key);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }
  }
}
