// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using System.Linq;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
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
