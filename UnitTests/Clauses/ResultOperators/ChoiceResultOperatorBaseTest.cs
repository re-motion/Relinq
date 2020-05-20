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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Clauses.ResultOperators
{
  [TestFixture]
  public class ChoiceResultOperatorBaseTest
  {
    private TestChoiceResultOperator _resultOperatorWithDefaultWhenEmpty;
    private TestChoiceResultOperator _resultOperatorNoDefaultWhenEmpty;

    [SetUp]
    public void SetUp ()
    {
      _resultOperatorWithDefaultWhenEmpty = new TestChoiceResultOperator (true);
      _resultOperatorNoDefaultWhenEmpty = new TestChoiceResultOperator (false);
    }

    [Test]
    public void GetOutputDataInfo ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (object[]), studentExpression);
      var result = _resultOperatorNoDefaultWhenEmpty.GetOutputDataInfo (input);

      Assert.That (result, Is.InstanceOf (typeof (StreamedSingleValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void GetOutputDataInfo_DefaultWhenEmpty ()
    {
      var studentExpression = Expression.Constant (new Cook ());
      var input = new StreamedSequenceInfo (typeof (Cook[]), studentExpression);

      Assert.That (((StreamedSingleValueInfo) _resultOperatorWithDefaultWhenEmpty.GetOutputDataInfo (input)).ReturnDefaultWhenEmpty, Is.True);
      Assert.That (((StreamedSingleValueInfo) _resultOperatorNoDefaultWhenEmpty.GetOutputDataInfo (input)).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void GetOutputDataInfo_InvalidInput ()
    {
      var input = new StreamedScalarValueInfo (typeof (Cook));
      Assert.That (
          () => _resultOperatorNoDefaultWhenEmpty.GetOutputDataInfo (input),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Parameter 'inputInfo' has type 'Remotion.Linq.Clauses.StreamedData.StreamedScalarValueInfo' "
                  + "when type 'Remotion.Linq.Clauses.StreamedData.StreamedSequenceInfo' was expected."
                  + "\r\nParameter name: inputInfo"));
    }
  }
}
