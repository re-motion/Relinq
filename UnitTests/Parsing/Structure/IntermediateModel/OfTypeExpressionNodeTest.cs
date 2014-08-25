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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class OfTypeExpressionNodeTest : ExpressionNodeTestBase
  {
    private OfTypeExpressionNode _node;
    private MethodInfo _ofTypeWithChefMethod;
    private MainSourceExpressionNode _cookSource;
    private MainFromClause _cookClause;

    public override void SetUp ()
    {
      base.SetUp ();

      _cookSource = new MainSourceExpressionNode ("s", Expression.Constant (new[] { new Cook () }));
      _cookClause = ExpressionHelper.CreateMainFromClause<Cook> ();
      ClauseGenerationContext.AddContextInfo (_cookSource, _cookClause);

      _ofTypeWithChefMethod = ReflectionUtility.GetMethod (() => ((IQueryable<Cook[]>) null).OfType<Chef> ());
      _node = new OfTypeExpressionNode (CreateParseInfo (_cookSource, "s", _ofTypeWithChefMethod));
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

      var expectedResult = ExpressionHelper.Resolve<Cook, string> (_cookClause, s => ((Chef) s).LetterOfRecommendation);
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
