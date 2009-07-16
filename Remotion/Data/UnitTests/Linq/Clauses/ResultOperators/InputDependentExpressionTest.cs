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
using NUnit.Framework;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using System.Linq.Expressions;
using Remotion.Data.UnitTests.Linq.Parsing;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class InputDependentExpressionTest
  {
    private Expression<Func<Student, string>> _dependentExpression;
    private QuerySourceReferenceExpression _expectedInput;
    private InputDependentExpression _expression;

    [SetUp]
    public void SetUp ()
    {
      _dependentExpression = ExpressionHelper.CreateLambdaExpression<Student, string> (s => s.First);
      var mainFromClause = ExpressionHelper.CreateMainFromClause ("s", typeof (Student), ExpressionHelper.CreateQuerySource());
      _expectedInput = new QuerySourceReferenceExpression (mainFromClause);
      _expression = new InputDependentExpression (_dependentExpression, _expectedInput);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_WithInvalidLambda ()
    {
      var dependentExpression = ExpressionHelper.CreateLambdaExpression<Student, Student, string> ((s1, s2) => s1.First);
      new InputDependentExpression (dependentExpression, _expectedInput);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_WithNonMatchingInput ()
    {
      var dependentExpression = ExpressionHelper.CreateLambdaExpression<IndustrialSector, string> (sector => sector.Student_Detail.Student.First);
      new InputDependentExpression (dependentExpression, _expectedInput);
    }

    [Test]
    public void ResolvedExpression ()
    {
      var expectedResolvedExpression = ExpressionHelper.Resolve<Student, string> (_expectedInput.ReferencedClause, s => s.First);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResolvedExpression, _expression.ResolvedExpression);
    }

    [Test]
    public new void ToString ()
    {
      Assert.That (_expression.ToString (), Is.EqualTo ("[s].First"));
    }
  }
}