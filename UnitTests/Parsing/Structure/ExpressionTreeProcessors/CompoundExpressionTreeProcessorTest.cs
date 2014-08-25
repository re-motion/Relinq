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
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.ExpressionTreeProcessors
{
  [TestFixture]
  public class CompoundExpressionTreeProcessorTest
  {
    private IExpressionTreeProcessor _stepMock2;
    private IExpressionTreeProcessor _stepMock1;
    private CompoundExpressionTreeProcessor _compoundExpressionTreeProcessor;

    [SetUp]
    public void SetUp ()
    {
      _stepMock1 = MockRepository.GenerateStrictMock<IExpressionTreeProcessor> ();
      _stepMock2 = MockRepository.GenerateStrictMock<IExpressionTreeProcessor> ();
      _compoundExpressionTreeProcessor = new CompoundExpressionTreeProcessor (new[] { _stepMock1, _stepMock2 });
    }

    [Test]
    public void InnerSteps ()
    {
      Assert.That (_compoundExpressionTreeProcessor.InnerProcessors, Is.EqualTo (new[] { _stepMock1, _stepMock2 }));

      var fakeStep = MockRepository.GenerateStub<IExpressionTreeProcessor> ();
      _compoundExpressionTreeProcessor.InnerProcessors.Add (fakeStep);

      Assert.That (_compoundExpressionTreeProcessor.InnerProcessors, Is.EqualTo (new[] { _stepMock1, _stepMock2, fakeStep }));
    }

    [Test]
    public void Process ()
    {
      var inputTree = ExpressionHelper.CreateExpression();
      var fakeResult1 = ExpressionHelper.CreateExpression();
      var fakeResult2 = ExpressionHelper.CreateExpression();

      _stepMock1
          .Expect (mock => mock.Process (inputTree))
          .Return (fakeResult1);
      _stepMock2
          .Expect (mock => mock.Process (fakeResult1))
          .Return (fakeResult2);
      _stepMock1.Replay();
      _stepMock2.Replay();

      var result = _compoundExpressionTreeProcessor.Process (inputTree);

      _stepMock1.VerifyAllExpectations();
      _stepMock2.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult2));
    }
  }
}