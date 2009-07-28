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
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.UnitTests.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Clauses.ExpressionTreeVisitors
{
  [TestFixture]
  public class ReverseResolvingExpressionTreeVisitorTest
  {
    private ConstructorInfo _anonymousTypeCtor;
    
    private MainFromClause _fromClause1;
    private QuerySourceReferenceExpression _querySource1;
    private MainFromClause _fromClause2;
    private QuerySourceReferenceExpression _querySource2;

    private NewExpression _itemExpression;

    [SetUp]
    public void SetUp ()
    {
      _anonymousTypeCtor = typeof (AnonymousType<Student, Student>).GetConstructor (new[] { typeof (Student), typeof (Student) });

      _fromClause1 = ExpressionHelper.CreateMainFromClause ("s1", typeof (Student), ExpressionHelper.CreateStudentQueryable ());
      _querySource1 = new QuerySourceReferenceExpression (_fromClause1);

      _fromClause2 = ExpressionHelper.CreateMainFromClause ("s2", typeof (Student), ExpressionHelper.CreateStudentQueryable ());
      _querySource2 = new QuerySourceReferenceExpression (_fromClause2);

      _itemExpression = Expression.New (
          _anonymousTypeCtor,
          new Expression[] { _querySource1, _querySource2 },
          new MemberInfo[] { typeof (AnonymousType<Student, Student>).GetProperty ("a"), typeof (AnonymousType<Student, Student>).GetProperty ("b") });
    }

    [Test]
    public void NoReferenceExpression ()
    {
      // itemExpression: new AnonymousType<Student, Student> ( a = [s1], b = [s2] )
      // resolvedExpression: 0
      // expected result: input => 0

      var resolvedExpression = Expression.Constant (0);

      LambdaExpression lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<AnonymousType<Student, Student>, int> (input => 0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }

    [Test]
    public void TopLevelReferenceExpression ()
    {
      // itemExpression: new AnonymousType<Student, Student> ( a = [s1], b = [s2] )
      // resolvedExpression: [s1]
      // expected result: input => input.a

      var resolvedExpression = _querySource1;

      LambdaExpression lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<AnonymousType<Student, Student>, Student> (input => input.a);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot create a LambdaExpression that retrieves the value of '[s3]' "
        + "from items with a structure of 'new AnonymousType`2(a = [s1], b = [s2])'. The item expression does not contain the value or it is too "
        + "complex.")]
    public void NonAccessibleReferenceExpression_Throws ()
    {
      // itemExpression: new AnonymousType<Student, Student> ( a = [s1], b = [s2] )
      // resolvedExpression: [s3]
      // expected result: exception

      var fromClause3 = ExpressionHelper.CreateMainFromClause ("s3", typeof (Student), ExpressionHelper.CreateStudentQueryable());
      var resolvedExpression = new QuerySourceReferenceExpression (fromClause3);

      ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);
    }

    [Test]
    public void MultipleNestedReferenceExpressions ()
    {
      // itemExpression: new AnonymousType<Student, Student> ( a = [s1], b = [s2] )
      // resolvedExpression: [s1].ID + [s2].ID
      // expected result: input => input.a.ID + input.b.ID

      var resolvedExpression = ExpressionHelper.Resolve<Student, Student, int> (_fromClause1, _fromClause2, (s1, s2) => s1.ID + s2.ID);

      LambdaExpression lambdaExpression = ReverseResolvingExpressionTreeVisitor.ReverseResolve (_itemExpression, resolvedExpression);

      var expectedExpression = ExpressionHelper.CreateLambdaExpression<AnonymousType<Student, Student>, int> (input => input.a.ID + input.b.ID);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, lambdaExpression);
    }
  }
}