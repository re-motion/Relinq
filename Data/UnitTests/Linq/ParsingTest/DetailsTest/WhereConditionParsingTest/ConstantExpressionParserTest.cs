/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
