/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class ResultModifierParserTest : DetailParserTestBase
  {
    private SelectProjectionParserRegistry _registry;
    private IQueryable<Student> _source;

    public override void SetUp ()
    {
      base.SetUp ();
      _registry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode ());
      _source = null;
    }

    [Test]
    public void MethodCallEvaluation_Count ()
    {
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (_source));
      MethodCallExpression resultModifierExpression = Expression.Call (methodInfo, Expression.Constant (null, typeof (IQueryable<Student>)));

      ResultModifierParser parser = new ResultModifierParser (_registry);
      MethodCall result = parser.Parse (resultModifierExpression, ParseContext);

      MethodCall expected = new MethodCall (methodInfo, null, new List<IEvaluation> { SourceMarkerEvaluation.Instance });
      Assert.That (result, Is.EqualTo (expected));
    }
  }
}