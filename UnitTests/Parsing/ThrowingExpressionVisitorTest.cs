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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Development.UnitTesting.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.UnitTests.Clauses.Expressions;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing
{
  [TestFixture]
  public class ThrowingExpressionVisitorTest
  {
    private TestThrowingExpressionVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _visitor = new TestThrowingExpressionVisitor ();
    }

    [Test]
    public void VisitExtension_ReducedExpressionIsVisited ()
    {
      RelinqExpressionVisitor visitor = new TestThrowingConstantExpressionVisitor();
      var constantExpression = Expression.Constant (0);
      var expression = new TestExtensionExpression (constantExpression);

      var result = visitor.Visit (expression);

      Assert.That (result, Is.Not.SameAs (constantExpression));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo("ConstantExpression was visited"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitExtension: Test")]
    public void VisitExtension_NonReducibleExpression ()
    {
      RelinqExpressionVisitor visitor = new TestThrowingConstantExpressionVisitor ();

#if !NET_3_5
      var nonReducibleExpression = MockRepository.GenerateStub<Expression>();
#else
      var nonReducibleExpression = MockRepository.GenerateStub<ExtensionExpression> (typeof (int));
#endif
      nonReducibleExpression
          .Stub (stub => ExtensionExpressionTestHelper.CallAccept (stub, Arg<RelinqExpressionVisitor>.Is.Anything))
          .WhenCalled (mi => PrivateInvoke.InvokeNonPublicMethod (mi.Arguments[0], "VisitExtension", nonReducibleExpression))
          .Return (nonReducibleExpression);
      nonReducibleExpression.Stub (stub => stub.CanReduce).Return (false);
      nonReducibleExpression.Stub (stub => stub.ToString ()).Return ("Test");
      
      visitor.Visit (nonReducibleExpression);
    }

    [Test]
#if !NET_3_5
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitExtension: [-1]")]
#else
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitUnknownNonExtension: [-1]")]
#endif
    public void VisitUnknownNonExtension ()
    {
      Visit (_visitor, (ExpressionType) (-1));
    }

    [Test]
    public void Visit_Null ()
    {
      var result =_visitor.Visit (null);
      Assert.That (result, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitUnary", MatchType = MessageMatch.Contains)]
    public void VisitUnary ()
    {
      Visit (_visitor, ExpressionType.UnaryPlus);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitBinary: ", MatchType = MessageMatch.Contains)]
    public void VisitBinary ()
    {
      Visit (_visitor, ExpressionType.And);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitTypeBinary: ", MatchType = MessageMatch.Contains)]
    public void VisitTypeBinary ()
    {
      Visit (_visitor, ExpressionType.TypeIs);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitConstant: ", MatchType = MessageMatch.Contains)]
    public void VisitConstant ()
    {
      Visit (_visitor, ExpressionType.Constant);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitConditional: ", MatchType = MessageMatch.Contains)]
    public void VisitConditional ()
    {
      Visit (_visitor, ExpressionType.Conditional);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitParameter: ", MatchType = MessageMatch.Contains)]
    public void VisitParameter ()
    {
      Visit (_visitor, ExpressionType.Parameter);
    }

    [Test]
    public void VisitLambda ()
    {
      var visitor = new TestThrowingExpressionVisitorForLambda();
      var lambdaExpression = ExpressionInstanceCreator.GetExpressionInstance (ExpressionType.Lambda);
      Assert.That (() => visitor.Visit (lambdaExpression), Throws.TypeOf<NotSupportedException>().With.Message.Contains ("Test of VisitLambda: "));
      var baseResult = visitor.LambdaExpresssionBaseBehaviorResult;
      Assert.That (baseResult, Is.SameAs (lambdaExpression));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMethodCall: ", MatchType = MessageMatch.Contains)]
    public void VisitMethodCall ()
    {
      Visit (_visitor, ExpressionType.Call);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitInvocation: ", MatchType = MessageMatch.Contains)]
    public void VisitInvocation ()
    {
      Visit (_visitor, ExpressionType.Invoke);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMember: ", MatchType = MessageMatch.Contains)]
    public void VisitMember ()
    {
      Visit (_visitor, ExpressionType.MemberAccess);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitNew: ", MatchType = MessageMatch.Contains)]
    public void VisitNew ()
    {
      Visit (_visitor, ExpressionType.New);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitNewArray: ", MatchType = MessageMatch.Contains)]
    public void VisitNewArray ()
    {
      Visit (_visitor, ExpressionType.NewArrayInit);
    }

#if !NET_3_5
    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitBlock: ", MatchType = MessageMatch.Contains)]
    public void VisitBlock ()
    {
      Visit (_visitor, ExpressionType.Block);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitDebugInfo: ", MatchType = MessageMatch.Contains)]
    public void VisitDebugInfo ()
    {
      Visit (_visitor, ExpressionType.DebugInfo);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitDefault: ", MatchType = MessageMatch.Contains)]
    public void VisitDefaultExpression ()
    {
      _visitor.Visit (Expression.Default (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitGoto: ", MatchType = MessageMatch.Contains)]
    public void VisitGoto ()
    {
      Visit (_visitor, ExpressionType.Goto);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitIndex: ", MatchType = MessageMatch.Contains)]
    public void VisitIndex ()
    {
      Visit (_visitor, ExpressionType.Index);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitLabel: ", MatchType = MessageMatch.Contains)]
    public void VisitLabel ()
    {
      Visit (_visitor, ExpressionType.Label);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitLoop: ", MatchType = MessageMatch.Contains)]
    public void VisitLoop ()
    {
      Visit (_visitor, ExpressionType.Loop);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitRuntimeVariables: ", MatchType = MessageMatch.Contains)]
    public void VisitRuntimeVariables ()
    {
      Visit (_visitor, ExpressionType.RuntimeVariables);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitSwitch: ", MatchType = MessageMatch.Contains)]
    public void VisitSwitch ()
    {
      Visit (_visitor, ExpressionType.Switch);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitTry: ", MatchType = MessageMatch.Contains)]
    public void VisitTry ()
    {
      Visit (_visitor, ExpressionType.Try);
    }
#endif

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberInit: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberInit ()
    {
      Visit (_visitor, ExpressionType.MemberInit);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitListInit: ", MatchType = MessageMatch.Contains)]
    public void VisitListInit ()
    {
      Visit (_visitor, ExpressionType.ListInit);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitElementInit: ", MatchType = MessageMatch.Contains)]
    public void VisitElementInit ()
    {
      _visitor.VisitElementInit (ExpressionInstanceCreator.CreateElementInit());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberAssignment: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberAssignment ()
    {
      _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberAssignment ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberMemberBinding: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberMemberBinding ()
    {
      _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberMemberBinding (new MemberBinding[0]));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitMemberListBinding: ", MatchType = MessageMatch.Contains)]
    public void VisitMemberListBinding ()
    {
      _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberListBinding (new ElementInit[0]));
    }

#if !NET_3_5
    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitCatchBlock: ", MatchType = MessageMatch.Contains)]
    public void VisitCatchBlock ()
    {
      _visitor.VisitCatchBlock (ExpressionInstanceCreator.CreateCatchBlock());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitLabelTarget: ", MatchType = MessageMatch.Contains)]
    public void VisitLabelTarget ()
    {
      _visitor.VisitLabelTarget (ExpressionInstanceCreator.CreateLabelTarget());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitSwitchCase: ", MatchType = MessageMatch.Contains)]
    public void VisitSwitchCase ()
    {
      _visitor.VisitSwitchCase (ExpressionInstanceCreator.CreateSwitchCase());
    }
#endif

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitSubQuery: ", MatchType = MessageMatch.Contains)]
    public void VisitSubQuery ()
    {
      _visitor.Visit (new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook> ()));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Test of VisitQuerySourceReference: ", MatchType = MessageMatch.Contains)]
    public void VisitQuerySourceReference ()
    {
      _visitor.Visit (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ()));
    }

    private void Visit (TestThrowingExpressionVisitor visitor, ExpressionType nodeType)
    {
      visitor.Visit (ExpressionInstanceCreator.GetExpressionInstance (nodeType));
    }

  }
}
