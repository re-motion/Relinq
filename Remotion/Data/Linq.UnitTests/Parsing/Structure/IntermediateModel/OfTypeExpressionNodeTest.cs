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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class OfTypeExpressionNodeTest : ExpressionNodeTestBase
  {
    private OfTypeExpressionNode _node;
    private MethodInfo _ofTypeWithGoodStudentMethod;
    private MainSourceExpressionNode _studentSource;
    private MainFromClause _studentClause;

    public override void SetUp ()
    {
      base.SetUp ();

      _studentSource = new MainSourceExpressionNode ("s", Expression.Constant (new[] { new Cook () }));
      _studentClause = ExpressionHelper.CreateMainFromClause_Student ();
      ClauseGenerationContext.AddContextInfo (_studentSource, _studentClause);

      _ofTypeWithGoodStudentMethod = ReflectionUtility.GetMethod (() => ((IQueryable<Cook[]>) null).OfType<Chef> ());
      _node = new OfTypeExpressionNode (CreateParseInfo (_studentSource, "s", _ofTypeWithGoodStudentMethod));
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (OfTypeExpressionNode.SupportedMethods, q => q.OfType<int> (), e => e.OfType<int> ());
    }

    [Test]
    public void OfTypeItemType ()
    {
      Assert.That (_node.SearchedItemType, Is.SameAs (typeof (Chef)));
    }

    [Test]
    public void Resolve_PassesConvertedExpressionToSource ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<Chef, string> (s => s.LetterOfRecommendation);
      var result = _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<Cook, string> (_studentClause, s => ((Chef) s).LetterOfRecommendation);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));
      Assert.That (QueryModel.ResultOperators.Count, Is.EqualTo (1));

      var OfTypeResultOperator = (OfTypeResultOperator) QueryModel.ResultOperators[0];
      Assert.That (OfTypeResultOperator.SearchedItemType, Is.SameAs (typeof (Chef)));
    }
  }
}
