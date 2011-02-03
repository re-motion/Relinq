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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure
{
  [TestFixture]
  public class CompoundProcessingStepTest
  {
    private IExpressionTreeProcessingStep _stepMock2;
    private IExpressionTreeProcessingStep _stepMock1;
    private CompoundProcessingStep _compoundStep;

    [SetUp]
    public void SetUp ()
    {
      _stepMock1 = MockRepository.GenerateStrictMock<IExpressionTreeProcessingStep> ();
      _stepMock2 = MockRepository.GenerateStrictMock<IExpressionTreeProcessingStep> ();
      _compoundStep = new CompoundProcessingStep (new[] { _stepMock1, _stepMock2 });
    }

    [Test]
    public void InnerSteps ()
    {
      Assert.That (_compoundStep.InnerSteps, Is.EqualTo (new[] { _stepMock1, _stepMock2 }));

      var fakeStep = MockRepository.GenerateStub<IExpressionTreeProcessingStep> ();
      _compoundStep.InnerSteps.Add (fakeStep);

      Assert.That (_compoundStep.InnerSteps, Is.EqualTo (new[] { _stepMock1, _stepMock2, fakeStep }));
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

      var result = _compoundStep.Process (inputTree);

      _stepMock1.VerifyAllExpectations();
      _stepMock2.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult2));
    }
  }
}