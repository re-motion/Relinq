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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.UnitTests.Linq.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class CastExpressionNodeTest : ExpressionNodeTestBase
  {
    private CastExpressionNode _node;
    private MethodInfo _castToGoodStudentMethod;
    private MainSourceExpressionNode _studentSource;
    private MainFromClause _studentClause;

    public override void SetUp ()
    {
      base.SetUp ();

      _studentSource = new MainSourceExpressionNode ("s", Expression.Constant (new[] { new Student () }));
      _studentClause = ExpressionHelper.CreateMainFromClause_Student ();
      ClauseGenerationContext.ClauseMapping.AddMapping (_studentSource, _studentClause);

      _castToGoodStudentMethod = ReflectionUtility.GetMethod (() => ((IQueryable<Student[]>)null).Cast<GoodStudent>());
      _node = new CastExpressionNode (CreateParseInfo (_studentSource, "s", _castToGoodStudentMethod));
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (CastExpressionNode.SupportedMethods, q => q.Cast<int> (), e => e.Cast<int> ());
    }

    [Test]
    public void CastItemType ()
    {
      Assert.That (_node.CastItemType, Is.SameAs (typeof (GoodStudent)));
    }

    [Test]
    public void Resolve_PassesConvertedExpressionToSource ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<GoodStudent, string> (s => s.LetterOfRecommendation);
      var result = _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<Student, string> (_studentClause, s => ((GoodStudent) s).LetterOfRecommendation);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));
      Assert.That (QueryModel.ResultOperators.Count, Is.EqualTo (1));

      var castResultOperator = (CastResultOperator) QueryModel.ResultOperators[0];
      Assert.That (castResultOperator.CastItemType, Is.SameAs (typeof (GoodStudent)));
    }
  }
}