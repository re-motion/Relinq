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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class NewWithAddedMembersQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void KeyValuePair_And_TupleCtors_GetMemberInfo ()
    {
      var query = from c in QuerySource 
                  select new 
                  { 
                    KVP = new KeyValuePair<string, int>(c.Name, 0),
                    DE = new DictionaryEntry (c.Name, 0),
                    Tuple = new Tuple<string, int> (c.Name, 0) 
                  };

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selector = (NewExpression) queryModel.SelectClause.Selector;
      Assert.That (selector.Arguments.Count, Is.EqualTo (3));
      Assert.That (selector.Members, Is.Not.Null);
      Assert.That (selector.Members.Count, Is.EqualTo (3));
      CheckMemberInNewExpression (selector.Type, "KVP", selector.Members[0]);
      CheckMemberInNewExpression (selector.Type, "DE", selector.Members[1]);
      CheckMemberInNewExpression (selector.Type, "Tuple", selector.Members[2]);

      var kvpArgument = (NewExpression) selector.Arguments[0];
      Assert.That (kvpArgument.Arguments.Count, Is.EqualTo (2));
      Assert.That (kvpArgument.Members, Is.Not.Null);
      Assert.That (kvpArgument.Members.Count, Is.EqualTo (2));
      CheckMemberInNewExpression (typeof(KeyValuePair<string, int>), "Key", kvpArgument.Members[0]);
      CheckMemberInNewExpression (typeof (KeyValuePair<string, int>), "Value", kvpArgument.Members[1]);

      var deArgument = (NewExpression) selector.Arguments[1];
      Assert.That (deArgument.Arguments.Count, Is.EqualTo (2));
      Assert.That (deArgument.Members, Is.Not.Null);
      Assert.That (deArgument.Members.Count, Is.EqualTo (2));
      CheckMemberInNewExpression (typeof (DictionaryEntry), "Key", deArgument.Members[0]);
      CheckMemberInNewExpression (typeof (DictionaryEntry), "Value", deArgument.Members[1]);

      var tupleArgument = (NewExpression) selector.Arguments[2];
      Assert.That (tupleArgument.Arguments.Count, Is.EqualTo (2));
      Assert.That (tupleArgument.Members, Is.Not.Null);
      Assert.That (tupleArgument.Members.Count, Is.EqualTo (2));
      CheckMemberInNewExpression (typeof (Tuple<string, int>), "Item1", tupleArgument.Members[0]);
      CheckMemberInNewExpression (typeof (Tuple<string, int>), "Item2", tupleArgument.Members[1]);
    }

    private void CheckMemberInNewExpression (Type expectedDeclaringType, string expectedPropertyName, MemberInfo actualMemberInfo)
    {
      var expectedProperty = expectedDeclaringType.GetProperty (expectedPropertyName);
      if (Environment.Version.Major < 4)
        Assert.That (actualMemberInfo, Is.EqualTo (expectedProperty.GetGetMethod()));
      else
        Assert.That (actualMemberInfo, Is.EqualTo (expectedProperty));
    }
  }
}
