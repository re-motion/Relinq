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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;
using ConstantExpressionParser=Remotion.Data.Linq.Parsing.Details.WhereConditionParsing.ConstantExpressionParser;
using NUnit.Framework.SyntaxHelpers;
using System.Linq;


namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest
{
  [TestFixture]
  public class ParserRegistryTest
  {
    [Test]
    public void GetParsers_NoParserRegistered ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();
      IEnumerable resultList = parserRegistry.GetParsers (typeof (Expression));
      Assert.IsFalse (resultList.GetEnumerator ().MoveNext ());
    }

    [Test]
    public void GetParsers_ParsersRegistered ()
    {
      ConstantExpressionParser parser1 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser2 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser3 = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser1);
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser2);
      parserRegistry.RegisterParser (typeof (BinaryExpression), parser3);

      Assert.That (parserRegistry.GetParsers (typeof (ConstantExpression)).ToArray (), Is.EqualTo (new[] { parser2, parser1 }));
      Assert.That (parserRegistry.GetParsers (typeof (BinaryExpression)).ToArray (), Is.EqualTo (new[] { parser3 }));
   }

    [Test]
    public void GetParser_LastRegisteredParser ()
    {
      ConstantExpressionParser parser1 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser2 = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      ParserRegistry parserRegistry = new ParserRegistry();

      parserRegistry.RegisterParser (typeof (ConstantExpression), parser1);
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser2);
      
      Assert.That (parserRegistry.GetParser (Expression.Constant (0)), Is.SameAs (parser2));
    }

    [Test]
    [ExpectedException (typeof (ParseException), ExpectedMessage = "Cannot parse 5, no appropriate parser found")]
    public void GetParser_NoParserFound ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();

      ConstantExpression constantExpression = Expression.Constant (5);
      parserRegistry.GetParser (constantExpression);
    }
  }
}
