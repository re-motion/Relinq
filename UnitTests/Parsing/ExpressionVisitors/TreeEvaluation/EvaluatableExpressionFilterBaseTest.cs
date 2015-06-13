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
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.TreeEvaluation
{
  [TestFixture]
  public class EvaluatableExpressionFilterBaseTest
  {
    private class TestableEvaluatableExpressionFilterBase : EvaluatableExpressionFilterBase
    {
    }

    [Test]
    public void IsEvaluatableExpression ()
    {
      var supportedExpressionTypeValues =
          Enum.GetValues (typeof (ExpressionType))
              .Cast<ExpressionType>()
              .Except (
                  new[]
                  {
                      ExpressionType.Parameter,
#if !NET_3_5
                      ExpressionType.Extension,
                      ExpressionType.RuntimeVariables,
#endif
                  })
              .ToArray();

      foreach (var expressionType in supportedExpressionTypeValues)
      {
        var expressionInstance = ExpressionInstanceCreator.GetExpressionInstance (expressionType);
        if (expressionInstance == null)
          continue;

        var methods = typeof (EvaluatableExpressionFilterBase).FindMembers (
            MemberTypes.Method,
            BindingFlags.Instance | BindingFlags.Public,
            (memberInfo, criteria) => memberInfo.Name.StartsWith ("IsEvaluatable")
                                      && ((MethodInfo) memberInfo).GetParameters().All (pi => pi.ParameterType.IsInstanceOfType (criteria)),
            expressionInstance)
            .Cast<MethodInfo>().ToArray();
        Assert.That (methods.Count(), Is.LessThan (2), "Found more than one method for ExpressionType '{0}'.", expressionType);
        Assert.That (methods, Is.Not.Empty, "Method for ExpressionType '{0}' was not found.", expressionType);
        var method = methods.Single();

        var filter = new TestableEvaluatableExpressionFilterBase();
        var result = (bool) method.Invoke (filter, new object[] { expressionInstance });

        Assert.That (result, Is.True);
      }
    }

    [Test]
    public void IsEvaluatableElementInit ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableElementInit (ExpressionInstanceCreator.CreateElementInit());

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsEvaluatableMemberAssignment ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableMemberAssignment (ExpressionInstanceCreator.CreateMemberAssignment());

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsEvaluatableMemberMemberBinding ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableMemberMemberBinding (ExpressionInstanceCreator.CreateMemberMemberBinding (new MemberBinding[0]));

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsEvaluatableMemberListBinding ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableMemberListBinding (ExpressionInstanceCreator.CreateMemberListBinding (new ElementInit[0]));

      Assert.That (result, Is.True);
    }

#if !NET_3_5
    [Test]
    public void IsEvaluatableCatchBlock ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableCatchBlock (ExpressionInstanceCreator.CreateCatchBlock());

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsEvaluatableLabelTarget ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableLabelTarget (ExpressionInstanceCreator.CreateLabelTarget());

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsEvaluatableSwitchCase ()
    {
      var filter = new TestableEvaluatableExpressionFilterBase();
      var result = filter.IsEvaluatableSwitchCase (ExpressionInstanceCreator.CreateSwitchCase ());

      Assert.That (result, Is.True);
    }
#endif
  }
}