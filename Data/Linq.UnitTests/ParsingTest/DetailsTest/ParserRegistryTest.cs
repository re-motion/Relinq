/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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


namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
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
