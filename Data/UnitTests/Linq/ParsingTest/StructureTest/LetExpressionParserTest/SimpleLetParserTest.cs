/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.LetExpressionParserTest
{
  [TestFixture]
  public class SimpleLetParserTest
  {
    private MethodCallExpression _letExpression;
    private ParseResultCollector _result;
    private BodyHelper _bodyHelper;

    [SetUp]
    public void SetUp ()
    {
      _letExpression = (MethodCallExpression) LetTestQueryGenerator.CreateSimpleSelect_LetExpression (ExpressionHelper.CreateQuerySource ()).Arguments[0];
      _result = new ParseResultCollector (_letExpression);
      new LetExpressionParser ().Parse (_result, _letExpression);
      _bodyHelper = new BodyHelper (_result.BodyExpressions);
    }

    [Test]
    public void ParsesLetExpression ()
    {
      Assert.IsNotNull (_bodyHelper.LetExpressions);
    }

    [Test]
    public void ParsesLetIdentifiers ()
    {
      Assert.IsNotNull (_bodyHelper.LetIdentifiers);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyHelper.LetIdentifiers[0]);
      Assert.AreEqual ("x", _bodyHelper.LetIdentifiers[0].Name);
      Assert.AreEqual (typeof(string), _bodyHelper.LetIdentifiers[0].Type);
    }
  }
}
