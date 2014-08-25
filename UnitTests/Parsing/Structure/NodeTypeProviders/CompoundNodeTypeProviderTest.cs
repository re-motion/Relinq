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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders
{
  [TestFixture]
  public class CompoundNodeTypeProviderTest
  {
    private INodeTypeProvider _nodeTypeProviderMock1;
    private INodeTypeProvider _nodeTypeProviderMock2;
    private CompoundNodeTypeProvider _compoundProvider;
    private MethodInfo _methodInfo;

    [SetUp]
    public void SetUp ()
    {
      _nodeTypeProviderMock1 = MockRepository.GenerateStrictMock<INodeTypeProvider>();
      _nodeTypeProviderMock2 = MockRepository.GenerateStrictMock<INodeTypeProvider> ();
      _compoundProvider = new CompoundNodeTypeProvider (new[] { _nodeTypeProviderMock1, _nodeTypeProviderMock2 });
      _methodInfo = typeof (object).GetMethod ("ToString");
    }

    [Test]
    public void InnerProviders ()
    {
      Assert.That (_compoundProvider.InnerProviders, Is.EqualTo (new[] { _nodeTypeProviderMock1, _nodeTypeProviderMock2 }));

      var fakeProvider = MockRepository.GenerateStub<INodeTypeProvider>();
      _compoundProvider.InnerProviders.Add (fakeProvider);

      Assert.That (_compoundProvider.InnerProviders, Is.EqualTo (new[] { _nodeTypeProviderMock1, _nodeTypeProviderMock2, fakeProvider }));
    }

    [Test]
    public void IsRegistered_FirstReturnsTrue ()
    {
      _nodeTypeProviderMock1
          .Expect (mock => mock.IsRegistered (_methodInfo))
          .Return (true);
      _nodeTypeProviderMock1.Replay();
      _nodeTypeProviderMock2.Replay();

      var result = _compoundProvider.IsRegistered (_methodInfo);

      _nodeTypeProviderMock1.VerifyAllExpectations();
      _nodeTypeProviderMock2.AssertWasNotCalled (mock => mock.IsRegistered (Arg<MethodInfo>.Is.Anything));
      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_SecondReturnsTrue ()
    {
      _nodeTypeProviderMock1
          .Expect (mock => mock.IsRegistered (_methodInfo))
          .Return (false);
      _nodeTypeProviderMock2
          .Expect (mock => mock.IsRegistered (_methodInfo))
          .Return (true);
      _nodeTypeProviderMock1.Replay ();
      _nodeTypeProviderMock2.Replay ();

      var result = _compoundProvider.IsRegistered (_methodInfo);

      _nodeTypeProviderMock1.VerifyAllExpectations ();
      _nodeTypeProviderMock2.VerifyAllExpectations();
      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_NoneReturnsTrue ()
    {
      _nodeTypeProviderMock1
          .Expect (mock => mock.IsRegistered (_methodInfo))
          .Return (false);
      _nodeTypeProviderMock2
          .Expect (mock => mock.IsRegistered (_methodInfo))
          .Return (false);
      _nodeTypeProviderMock1.Replay ();
      _nodeTypeProviderMock2.Replay ();

      var result = _compoundProvider.IsRegistered (_methodInfo);

      _nodeTypeProviderMock1.VerifyAllExpectations ();
      _nodeTypeProviderMock2.VerifyAllExpectations ();
      Assert.That (result, Is.False);
    }

    [Test]
    public void GetNodeType_FirstReturnsType ()
    {
      _nodeTypeProviderMock1
          .Expect (mock => mock.GetNodeType (_methodInfo))
          .Return (typeof (string));
      _nodeTypeProviderMock1.Replay ();
      _nodeTypeProviderMock2.Replay ();

      var result = _compoundProvider.GetNodeType (_methodInfo);

      _nodeTypeProviderMock1.VerifyAllExpectations ();
      _nodeTypeProviderMock2.AssertWasNotCalled (mock => mock.GetNodeType (Arg<MethodInfo>.Is.Anything));
      Assert.That (result, Is.SameAs (typeof (string)));
    }

    [Test]
    public void GetNodetype_SecondReturnsType ()
    {
      _nodeTypeProviderMock1
          .Expect (mock => mock.GetNodeType (_methodInfo))
          .Return (null);
      _nodeTypeProviderMock2
          .Expect (mock => mock.GetNodeType (_methodInfo))
          .Return (typeof(string));
      _nodeTypeProviderMock1.Replay ();
      _nodeTypeProviderMock2.Replay ();

      var result = _compoundProvider.GetNodeType (_methodInfo);

      _nodeTypeProviderMock1.VerifyAllExpectations ();
      _nodeTypeProviderMock2.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs(typeof(string)));
    }

    [Test]
    public void GetNodeType_NoneReturnsType ()
    {
      _nodeTypeProviderMock1
          .Expect (mock => mock.GetNodeType (_methodInfo))
          .Return (null);
      _nodeTypeProviderMock2
          .Expect (mock => mock.GetNodeType (_methodInfo))
          .Return (null);
      _nodeTypeProviderMock1.Replay ();
      _nodeTypeProviderMock2.Replay ();

      var result = _compoundProvider.GetNodeType (_methodInfo);

      _nodeTypeProviderMock1.VerifyAllExpectations();
      _nodeTypeProviderMock2.VerifyAllExpectations();
      Assert.That (result, Is.Null);
    }
  }
}