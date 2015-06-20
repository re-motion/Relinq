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
using Remotion.Linq.Development.UnitTesting.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.ExpressionTreeProcessors
{
  [TestFixture]
  public class PartialEvaluatingExpressionTreeProcessorTest
  {
    [Test]
    public void Process ()
    {
      var expression = Expression.Add (Expression.Constant (1), Expression.Constant (1));
      var processor = new PartialEvaluatingExpressionTreeProcessor (new TestEvaluatableExpressionFilter());

      var result = processor.Process (expression);

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo (2));
    }

    [Test]
    public void Process_WithFilter_PassesIntoVisitor ()
    {
      var expression = Expression.Constant (1);

      var filterMock = MockRepository.GenerateStrictMock<IEvaluatableExpressionFilter>();
      filterMock.Expect (_ => _.IsEvaluatableConstant (expression)).Return (true);
      filterMock.Replay();

      var processor = new PartialEvaluatingExpressionTreeProcessor (filterMock);

      var result = processor.Process (expression);

      Assert.That (result, Is.SameAs (expression));
      filterMock.VerifyAllExpectations();
    }
  }
}