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
      var expression = new ReducibleExtensionExpression (constantExpression);

      var result = visitor.Visit (expression);

      Assert.That (result, Is.Not.SameAs (constantExpression));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo("ConstantExpression was visited"));
    }

    [Test]
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
      Assert.That (
          () => visitor.Visit (nonReducibleExpression),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("Test of VisitExtension: Test"));
    }

    [Test]
    public void VisitUnknownNonExtension () //REVIEW removed a part of the test
    {
        Assert.That (
                () => Visit (_visitor, (ExpressionType) (-1)),
                Throws.InstanceOf<NotSupportedException>()
                      .With.Message.EqualTo ("Test of VisitExtension: [-1]"));
    }

    [Test]
    public void Visit_Null ()
    {
      var result =_visitor.Visit (null);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void VisitUnary ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.UnaryPlus),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitUnary"));
    }

    [Test]
    public void VisitBinary ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.And),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitBinary: "));
    }

    [Test]
    public void VisitTypeBinary ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.TypeIs),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitTypeBinary: "));
    }

    [Test]
    public void VisitConstant ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Constant),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitConstant: "));
    }

    [Test]
    public void VisitConditional ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Conditional),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitConditional: "));
    }

    [Test]
    public void VisitParameter ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Parameter),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitParameter: "));
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
    public void VisitMethodCall ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Call),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitMethodCall: "));
    }

    [Test]
    public void VisitInvocation ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Invoke),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitInvocation: "));
    }

    [Test]
    public void VisitMember ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.MemberAccess),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitMember: "));
    }

    [Test]
    public void VisitNew ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.New),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitNew: "));
    }

    [Test]
    public void VisitNewArray ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.NewArrayInit),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitNewArray: "));
    }

#if !NET_3_5
    [Test]
    public void VisitBlock ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Block),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitBlock: "));
    }

    [Test]
    public void VisitDebugInfo ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.DebugInfo),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitDebugInfo: "));
    }

    [Test]
    public void VisitDefaultExpression ()
    {
      Assert.That (
          () => _visitor.Visit (Expression.Default (typeof (int))),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitDefault: "));
    }

    [Test]
    public void VisitGoto ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Goto),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitGoto: "));
    }

    [Test]
    public void VisitIndex ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Index),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitIndex: "));
    }

    [Test]
    public void VisitLabel ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Label),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitLabel: "));
    }

    [Test]
    public void VisitLoop ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Loop),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitLoop: "));
    }

    [Test]
    public void VisitRuntimeVariables ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.RuntimeVariables),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitRuntimeVariables: "));
    }

    [Test]
    public void VisitSwitch ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Switch),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitSwitch: "));
    }

    [Test]
    public void VisitTry ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.Try),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitTry: "));
    }
#endif

    [Test]
    public void VisitMemberInit ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.MemberInit),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitMemberInit: "));
    }

    [Test]
    public void VisitListInit ()
    {
      Assert.That (
          () => Visit (_visitor, ExpressionType.ListInit),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitListInit: "));
    }

    [Test]
    public void VisitElementInit ()
    {
      Assert.That (
          () => _visitor.VisitElementInit (ExpressionInstanceCreator.CreateElementInit()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitElementInit: "));
    }

    [Test]
    public void VisitMemberAssignment ()
    {
      Assert.That (
          () => _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberAssignment ()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitMemberAssignment: "));
    }

    [Test]
    public void VisitMemberMemberBinding ()
    {
      Assert.That (
          () => _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberMemberBinding (new MemberBinding[0])),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitMemberMemberBinding: "));
    }

    [Test]
    public void VisitMemberListBinding ()
    {
      Assert.That (
          () => _visitor.VisitMemberBinding (ExpressionInstanceCreator.CreateMemberListBinding (new ElementInit[0])),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitMemberListBinding: "));
    }

#if !NET_3_5
    [Test]
    public void VisitCatchBlock ()
    {
      Assert.That (
          () => _visitor.VisitCatchBlock (ExpressionInstanceCreator.CreateCatchBlock()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitCatchBlock: "));
    }

    [Test]
    public void VisitLabelTarget ()
    {
      Assert.That (
          () => _visitor.VisitLabelTarget (ExpressionInstanceCreator.CreateLabelTarget()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitLabelTarget: "));
    }

    [Test]
    public void VisitSwitchCase ()
    {
      Assert.That (
          () => _visitor.VisitSwitchCase (ExpressionInstanceCreator.CreateSwitchCase()),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitSwitchCase: "));
    }
#endif

    [Test]
    public void VisitSubQuery ()
    {
      Assert.That (
          () => _visitor.Visit (new SubQueryExpression (ExpressionHelper.CreateQueryModel<Cook> ())),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitSubQuery: "));
    }

    [Test]
    public void VisitQuerySourceReference ()
    {
      Assert.That (
          () => _visitor.Visit (new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ())),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.Contains ("Test of VisitQuerySourceReference: "));
    }

    private void Visit (TestThrowingExpressionVisitor visitor, ExpressionType nodeType)
    {
      visitor.Visit (ExpressionInstanceCreator.GetExpressionInstance (nodeType));
    }

  }
}
