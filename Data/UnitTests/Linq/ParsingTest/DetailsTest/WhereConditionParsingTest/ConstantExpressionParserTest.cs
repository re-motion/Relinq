// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ConstantExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse()
    {
      object expected = new Constant (5);
      ConstantExpressionParser parser = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      object result = parser.Parse (Expression.Constant(5, typeof (int)), ParseContext);
      Assert.AreEqual (expected, result);
    }
  }
}
