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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class TransparentIdentifierRemovingExpressionTreeVisitorTest
  {
    private NewExpression _anonymousTypeNewExpression;
    private NewExpression _anonymousTypeNewExpressionWithAssignments;
    private NewExpression _anonymousTypeNewExpressionWithMethodAssignments;
    private PropertyInfo _anonymousTypeA;
    private PropertyInfo _anonymousTypeB;
    private PropertyInfo _anonymousTypeList;
    private MethodInfo _listAddMethod;
    private Expression _assignedExpressionA;
    private Expression _assignedExpressionB;

    [SetUp]
    public void SetUp ()
    {
      _anonymousTypeA = typeof (AnonymousType).GetProperty ("a");
      _anonymousTypeB = typeof (AnonymousType).GetProperty ("b");
      _anonymousTypeList = typeof (AnonymousType).GetProperty ("List");

      _assignedExpressionA = Expression.Constant (5);
      _assignedExpressionB = Expression.Constant (6);

      _anonymousTypeNewExpression = Expression.New (typeof (AnonymousType).GetConstructor (Type.EmptyTypes));
      _anonymousTypeNewExpressionWithAssignments = Expression.New (
          typeof (AnonymousType).GetConstructor (new[] {typeof (int), typeof (int)}),
          new[] { _assignedExpressionA, _assignedExpressionB },
          new MemberInfo[] { _anonymousTypeA, _anonymousTypeB });
      _anonymousTypeNewExpressionWithMethodAssignments = Expression.New (
          typeof (AnonymousType).GetConstructor (new[] { typeof (int), typeof (int) }),
          new[] { _assignedExpressionA, _assignedExpressionB },
          new MemberInfo[] { _anonymousTypeA.GetGetMethod(), _anonymousTypeB.GetGetMethod() });

      _listAddMethod = _anonymousTypeList.PropertyType.GetMethod ("Add");
    }

    [Test]
    public void MemberExpression_WithMemberInitExpression_ReplacedByAssignedExpression ()
    {
      // new AnonymousType { a = 5 }.a => 5
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (_assignedExpressionA));
    }

    [Test]
    public void MemberExpression_WithNewExpression_WithMembers_ReplacedByAssignedExpression ()
    {
      // new AnonymousType ( a = 5 ).a => 5
      var memberExpression = Expression.MakeMemberAccess (
          _anonymousTypeNewExpressionWithAssignments,
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (_assignedExpressionA));
    }

    [Test]
    public void MemberExpression_WithNewExpression_WithMethod_ReplacedByAssignedExpression ()
    {
      // This strange expression, where a MethodInfo is specified in a NewExpression - instead of a PropertyInfo - is created by the 
      // C# compiler for transparent identifiers...

      // new AnonymousType ( get_a() = 5 ).a => 5
      var memberExpression = Expression.MakeMemberAccess (
          _anonymousTypeNewExpressionWithMethodAssignments,
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (_assignedExpressionA));
    }

    [Test]
    public void MemberExpression_WithOneMatchingAssignment_ReplacedByAssignedExpression ()
    {
      // new AnonymousType { a = 5, a = 6 }.b => 6
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA),
            Expression.Bind (_anonymousTypeB, _assignedExpressionB)),
          _anonymousTypeB);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (_assignedExpressionB));
    }

    [Test]
    public void MemberExpression_WithTwoMatchingAssignments_ReplacedByLastAssignedExpression ()
    {
      Expression assignedExpression1 = Expression.Constant (1);
      Expression assignedExpression2 = Expression.Constant (2);

      // new AnonymousType { a = 1, a = 2 }.a => 2
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, assignedExpression1),
            Expression.Bind (_anonymousTypeA, assignedExpression2)),
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (assignedExpression2));
    }

    [Test]
    public void MemberExpression_WithNoMatchingAssignment_ReturnsOriginalExpression ()
    {
      // new AnonymousType { a = 5 }.b => same
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeB);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void MemberExpression_WithCollectionAssignment_ReturnsOriginalExpression ()
    {
      // new AnonymousType { List = { 7 }, a = 5 }.List => same
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.ListBind (_anonymousTypeList, Expression.ElementInit (_listAddMethod, Expression.Constant (7))),
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeList);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void MemberExpression_WithNewExpression_WithoutMembers_ReturnsOriginalExpression ()
    {
      // new AnonymousType ().a => same
      var memberExpression = Expression.MakeMemberAccess (
          _anonymousTypeNewExpression,
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void MemberExpression_WithoutMemberInit_ReturnsOriginalExpression ()
    {
      var anon = new AnonymousType ();

      // Constant(anon).a => same
      var memberExpression = Expression.MakeMemberAccess (
          Expression.Constant (anon),
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (memberExpression);
      Assert.That (result, Is.SameAs (memberExpression));
    }

    [Test]
    public void TransparentIdentifier_WithinAnotherExpression ()
    {
      // () => new AnonymousType {a = 5}.a => () => 5
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (Expression.Lambda (memberExpression));
      Assert.That (result, Is.InstanceOf (typeof (LambdaExpression)));
      Assert.That (((LambdaExpression) result).Body, Is.SameAs (_assignedExpressionA));
    }

    [Test]
    public void TransparentIdentifier_WithinAnotherMemberExpression ()
    {
      Expression assignedExpression = Expression.Constant (null, typeof (List<int>));

      // new AnonymousType {List = null}.List.Count => () => null.Count
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeList, assignedExpression)),
          _anonymousTypeList);
      var outerMemberExpression = Expression.MakeMemberAccess (
          memberExpression,
          typeof (List<int>).GetProperty ("Count"));

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (outerMemberExpression);
      Assert.That (result, Is.InstanceOf (typeof (MemberExpression)));
      Assert.That (((MemberExpression) result).Expression, Is.SameAs (assignedExpression));
    }

    [Test]
    public void TransparentIdentifier_WithinAnotherMemberExpression_WithUnrelatedAssignments ()
    {
      // new AnonymousType {a = new AnonymousType {a = 5}.a )}.b => new AnonymousType { a = 5 }.b
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeA);
      var outerMemberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, memberExpression)),
          _anonymousTypeB);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (outerMemberExpression);

      Assert.That (result, Is.InstanceOf (typeof (MemberExpression)));
      var innerExpression = ((MemberExpression) result).Expression;
      Assert.That (innerExpression, Is.InstanceOf (typeof (MemberInitExpression)));
      Assert.That (((MemberAssignment) ((MemberInitExpression) innerExpression).Bindings[0]).Expression, Is.SameAs (_assignedExpressionA));
    }

    [Test]
    public void TransparentIdentifier_WithinSubQueries ()
    {
      // new AnonymousType { a = 5 }.a => 5
      var memberExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeA);

      var subQuery = ExpressionHelper.CreateQueryModel_Int();
      subQuery.SelectClause.Selector = memberExpression;

      var subQueryExpression = new SubQueryExpression (subQuery);

      var result = (SubQueryExpression) TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (subQueryExpression);

      Assert.That (result.QueryModel.SelectClause.Selector, Is.SameAs (_assignedExpressionA));
    }

    [Test]
    public void ReplaceableExpression_GivingAnotherReplaceableExpression ()
    {
      // new AnonymousType { a = (new AnonymousType { a = 5 }.a) }.a => 5
      var replaceableExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, _assignedExpressionA)),
          _anonymousTypeA);

      var outerExpression = Expression.MakeMemberAccess (
          Expression.MemberInit (
            _anonymousTypeNewExpression,
            Expression.Bind (_anonymousTypeA, replaceableExpression)),
          _anonymousTypeA);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (outerExpression);
      Assert.That (result, Is.SameAs (_assignedExpressionA));
    }


    [Test]
    public void IntegrationTest_WithSimpleExpressions ()
    {
      var expression1 = ExpressionHelper.MakeExpression<int, int> (i => new AnonymousType { a = i, b = 1 }.a);
      var expression2 = ExpressionHelper.MakeExpression<int, int> (i => new AnonymousType { a = i, b = 1 }.b);
      var expectedResult1 = ExpressionHelper.MakeExpression<int, int> (i => i);
      var expectedResult2 = ExpressionHelper.MakeExpression<int, int> (i => 1);

      var result1 = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (expression1);
      var result2 = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (expression2);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult1, result1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult2, result2);
    }

    [Test]
    public void IntegrationTest_WithExpressionNodes ()
    {
      var query = from a in ExpressionHelper.CreateQueryable<Cook> ()
                  from b in ExpressionHelper.CreateQueryable<Cook>()
                  where a.ID > 5
                  select a.ID;

      var nodeTypeRegistry = new MethodInfoBasedNodeTypeRegistry ();
      nodeTypeRegistry.Register (SelectExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));
      nodeTypeRegistry.Register (SelectManyExpressionNode.GetSupportedMethods(), typeof (SelectManyExpressionNode));
      nodeTypeRegistry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));

      var selectNode = (SelectExpressionNode) new ExpressionTreeParser (nodeTypeRegistry, new NullExpressionTreeProcessor()).ParseTree (query.Expression);
      var clauseGenerationContext = new ClauseGenerationContext (new MethodInfoBasedNodeTypeRegistry());

      var selectManyNode = (SelectManyExpressionNode) selectNode.Source.Source;
      var mainSourceExpressionNode = (MainSourceExpressionNode) selectManyNode.Source;

      var queryModel = mainSourceExpressionNode.Apply (null, clauseGenerationContext);
      var mainFromClause = queryModel.MainFromClause;
      
      selectManyNode.Apply (queryModel, clauseGenerationContext); // only to add the clause to the mapping

      var selectProjection = selectNode.GetResolvedSelector (clauseGenerationContext); // new ( a = IR (a), b = IR (b) ).a.ID

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (selectProjection);

      // IR(a).ID
      Assert.That (result, Is.InstanceOf (typeof (MemberExpression)));
      Expression innerExpression = ((MemberExpression)result).Expression;
      Assert.That (innerExpression, Is.InstanceOf (typeof (QuerySourceReferenceExpression)));
      Assert.That (((QuerySourceReferenceExpression) innerExpression).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void VisitUnknownNonExtensionExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers(expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitExtensionExpression_ChildrenAreProcessed ()
    {
      // new TestExtensionExpression (new AnonymousType ( a = 5 ).a) => new TestExtensionExpression (5)
      var memberExpression = Expression.MakeMemberAccess (
          _anonymousTypeNewExpressionWithAssignments,
          _anonymousTypeA);
      var extensionExpression = new TestExtensionExpression (memberExpression);

      var result = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (extensionExpression);

      var expectedExpression = new TestExtensionExpression (_assignedExpressionA);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

  }
}
