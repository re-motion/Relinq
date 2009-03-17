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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Expressions;
using Rhino.Mocks;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Visitor;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryModelTest
  {
    private Expression _expressionTree;
    private MainFromClause _mainFromClause;
    private SelectClause _selectClause;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _expressionTree = ExpressionHelper.CreateExpression ();
      _mainFromClause = ExpressionHelper.CreateMainFromClause ();
      _selectClause = ExpressionHelper.CreateSelectClause ();
      _queryModel = new QueryModel (typeof (IQueryable<string>), _mainFromClause, (ISelectGroupClause) _selectClause);
      _queryModel.ExpressionTree = _expressionTree;
    }

    [Test]
    public void Initialize_WithFromClauseAndBody ()
    {
      Assert.AreSame (_mainFromClause, _queryModel.MainFromClause);
      Assert.AreSame (_selectClause, _queryModel.SelectOrGroupClause);
      Assert.IsNotNull (_queryModel.GetExpressionTree ());
      Assert.AreEqual (typeof (IQueryable<string>), _queryModel.ResultType);
    }

    [Test]
    public void Initialize_WithExpressionTree ()
    {
      Assert.AreSame (_mainFromClause, _queryModel.MainFromClause);
      Assert.AreSame (_selectClause, _queryModel.SelectOrGroupClause);
      Assert.AreSame (_expressionTree, _queryModel.GetExpressionTree ());
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.Expect (mock => mock.VisitQueryModel (_queryModel));

      repository.ReplayAll ();

      _queryModel.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void Override_ToString()
    {
      var sv = new StringVisitor();
      sv.VisitQueryModel (_queryModel);
      Assert.AreEqual (sv.ToString (), _queryModel.ToString ());
    }

    [Test]
    public void GetExpressionTree_ForSuppliedTree ()
    {
      Expression expressionTree = _queryModel.GetExpressionTree ();
      Assert.That (expressionTree, Is.Not.Null);
      Assert.That (expressionTree, Is.SameAs (_expressionTree));
    }

    [Test]
    public void GetResolveableClause_FromClauseBase ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression identifier3 = Expression.Parameter (typeof (Student), "s3");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);
      AdditionalFromClause clause2 = new AdditionalFromClause (clause1, identifier2, fromExpression, projExpression);
      AdditionalFromClause clause3 = new AdditionalFromClause (clause2, identifier3, fromExpression, projExpression);


      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);
      model.AddBodyClause (clause2);
      model.AddBodyClause (clause3);

      Assert.AreSame (mainFromClause, model.GetResolveableClause ("s0", typeof (Student)));
      Assert.AreSame (clause1, model.GetResolveableClause ("s1", typeof (Student)));
      Assert.AreSame (clause2, model.GetResolveableClause ("s2", typeof (Student)));
      Assert.AreSame (clause3, model.GetResolveableClause ("s3", typeof (Student)));
    }

    [Test]
    public void GetResolveableClause_InvalidIdentifierName ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);

      Assert.IsNull (model.GetResolveableClause ("s273627", typeof (Student)));
    }
   
    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's0' has type "
        + "'Remotion.Data.UnitTests.Linq.Student', but 'System.String' was requested.")]
    public void GetResolveableClause_InvalidIdentifierType_MainFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);

      model.GetResolveableClause ("s0", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's1' has type "
        + "'Remotion.Data.UnitTests.Linq.Student', but 'System.String' was requested.")]
    public void GetResolveableClause_InvalidIdentifierType_AdditionalFromClause ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier0 = Expression.Parameter (typeof (Student), "s0");
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");

      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(identifier0, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause clause1 = new AdditionalFromClause (mainFromClause, identifier1, fromExpression, projExpression);

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (clause1);

      model.GetResolveableClause ("s1", typeof (string));
      Assert.Fail ("Expected exception");
    }

    [Test]
    public void GetResolveableClause_LetClause ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s0");
      
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      LetClause letClause = ExpressionHelper.CreateLetClause ();

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (letClause);

      Assert.AreSame (letClause, model.GetResolveableClause ("i", typeof (int)));
    }
    
    [Test]
    public void ResolveField ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource());
      var additionalFromClause = new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression(), ExpressionHelper.CreateLambdaExpression());

      QueryModel queryModel = ExpressionHelper.CreateQueryModel (mainFromClause);
      queryModel.AddBodyClause (additionalFromClause);

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      var context = new JoinedTableContext ();
      var policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      var descriptor = queryModel.ResolveField (resolver, fieldAccessExpression, context);

      IColumnSource expectedTable = queryModel.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      var expectedPath = new FieldSourcePath (expectedTable, new SingleJoin[0]);

      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedPath, descriptor.SourcePath);
    }

    [Test]
    public void ParentQuery_Null ()
    {
      Assert.IsNull (_queryModel.ParentQuery);
    }

    [Test]
    public void SetParentQuery ()
    {
      QueryModel parentQueryModel = ExpressionHelper.CreateQueryModel ();
      _queryModel.SetParentQuery (parentQueryModel);

      Assert.AreSame (parentQueryModel, _queryModel.ParentQuery);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The query already has a parent query.")]
    public void SetParentQuery_ThrowsOnSecondParent ()
    {
      QueryModel parentQueryModel1 = ExpressionHelper.CreateQueryModel ();
      QueryModel parentQueryModel2 = ExpressionHelper.CreateQueryModel ();
      _queryModel.SetParentQuery (parentQueryModel1);
      _queryModel.SetParentQuery (parentQueryModel2);
    }

    [Test]
    public void SetParentQuery_IgnoresSecondParent_WhenSame ()
    {
      QueryModel parentQueryModel = ExpressionHelper.CreateQueryModel ();
      _queryModel.SetParentQuery (parentQueryModel);
      _queryModel.SetParentQuery (parentQueryModel);
      Assert.That (_queryModel.ParentQuery, Is.SameAs (parentQueryModel));
    }

    [Test]
    public void Clone_ReturnsNewQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var clone = queryModel.Clone ();

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (queryModel));
    }

    [Test]
    public void Clone_HasSameResultTypeAndExpressionTree ()
    {
      var clone = _queryModel.Clone ();

      Assert.That (clone.ResultType, Is.SameAs (_queryModel.ResultType));
      Assert.That (clone.GetExpressionTree(), Is.SameAs (_queryModel.GetExpressionTree()));
    }

    [Test]
    public void Clone_HasCloneForMainFromClause ()
    {
      var clone = _queryModel.Clone ();

      Assert.That (clone.MainFromClause, Is.Not.SameAs (_queryModel.MainFromClause));
      Assert.That (clone.MainFromClause.Identifier, Is.EqualTo (_queryModel.MainFromClause.Identifier));
    }

    [Test]
    public void Clone_HasCloneForSelectClause ()
    {
      var selectClause = (SelectClause) _queryModel.SelectOrGroupClause;
      var clone = _queryModel.Clone ();

      Assert.That (clone.SelectOrGroupClause, Is.Not.SameAs (_queryModel.SelectOrGroupClause));
      var cloneSelectClause = ((SelectClause) clone.SelectOrGroupClause);
      Assert.That (cloneSelectClause.ProjectionExpression, Is.EqualTo (selectClause.ProjectionExpression));
      Assert.That (cloneSelectClause.PreviousClause, Is.SameAs (clone.MainFromClause));
    }

    [Test]
    public void Clone_HasClonesForBodyClauses ()
    {
      var additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      var whereClause = ExpressionHelper.CreateWhereClause();
      _queryModel.AddBodyClause (additionalFromClause);
      _queryModel.AddBodyClause (whereClause);

      var clone = _queryModel.Clone ();
      var clonedAdditionalFromClause = (AdditionalFromClause) clone.BodyClauses[0];
      var clonedWhereClause = (WhereClause) clone.BodyClauses[1];

      Assert.That (clonedAdditionalFromClause, Is.Not.SameAs (additionalFromClause));
      Assert.That (clonedAdditionalFromClause.Identifier, Is.SameAs (additionalFromClause.Identifier));
      Assert.That (clonedAdditionalFromClause.QueryModel, Is.SameAs (clone));
      Assert.That (clonedAdditionalFromClause.PreviousClause, Is.SameAs (clone.MainFromClause));

      Assert.That (clonedWhereClause, Is.Not.SameAs (whereClause));
      Assert.That (clonedWhereClause.BoolExpression, Is.EqualTo (clonedWhereClause.BoolExpression));
      Assert.That (clonedWhereClause.QueryModel, Is.SameAs (clone));
      Assert.That (clonedWhereClause.PreviousClause, Is.SameAs (clonedAdditionalFromClause));

      Assert.That (clone.SelectOrGroupClause.PreviousClause, Is.SameAs (clonedWhereClause));
    }

    //[Test]
    //public void InvalidateExpressionTree ()
    //{
    //  var queryModel = ExpressionHelper.CreateQueryModel ();
    //  queryModel.InvalidateExpressionTree (this, new EventArgs ());
    //  Assert.That (queryModel.GetExpressionTree (), Is.Null);
    //}
  }
}
