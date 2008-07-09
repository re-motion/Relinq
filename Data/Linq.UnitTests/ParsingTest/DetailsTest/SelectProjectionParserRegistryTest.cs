/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest
{
  [TestFixture]
  public class SelectProjectionParserRegistryTest
  {
    private IDatabaseInfo _databaseInfo;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private QueryModel _queryModel;
    private JoinedTableContext _context;
    private SelectProjectionParserRegistry _selectProjectionParserRegistry;
    private ParserRegistry _parserRegistry;
    private ParseMode _parseMode;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _context = new JoinedTableContext ();
      _parseMode = new ParseMode();
      _selectProjectionParserRegistry = new SelectProjectionParserRegistry (_databaseInfo, _parseMode);
      _parserRegistry = new ParserRegistry ();
    }

    [Test]
    public void Initialization_AddsDefaultParsers ()
    {
      SelectProjectionParserRegistry selectProjectionParserRegistry =
        new SelectProjectionParserRegistry (_databaseInfo,_parseMode);

      Assert.That (selectProjectionParserRegistry.GetParsers (typeof (BinaryExpression)).ToArray (), Is.Not.Empty);
      Assert.That (selectProjectionParserRegistry.GetParsers (typeof (ConstantExpression)).ToArray (), Is.Not.Empty);
      Assert.That (selectProjectionParserRegistry.GetParsers (typeof (MemberExpression)).ToArray (), Is.Not.Empty);
      Assert.That (selectProjectionParserRegistry.GetParsers (typeof (MethodCallExpression)).ToArray (), Is.Not.Empty);
      Assert.That (selectProjectionParserRegistry.GetParsers (typeof (NewExpression)).ToArray (), Is.Not.Empty);
      Assert.That (selectProjectionParserRegistry.GetParsers (typeof (ParameterExpression)).ToArray (), Is.Not.Empty);     
    }

    [Test]
    public void RegisterNewMethodCallExpressionParser_IsInsertedFirst ()
    {
      Assert.That (_selectProjectionParserRegistry.GetParsers (typeof (MethodCallExpression)).Count (), Is.EqualTo (1));

      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_selectProjectionParserRegistry);
      _selectProjectionParserRegistry.RegisterParser (typeof (MethodCallExpression), methodCallExpressionParser);
      Assert.That (_selectProjectionParserRegistry.GetParsers (typeof (MethodCallExpression)).Count (), Is.EqualTo (2));
      Assert.That (_selectProjectionParserRegistry.GetParsers (typeof (MethodCallExpression)).First(), Is.SameAs (methodCallExpressionParser));
    }

    [Test]
    public void GetParser ()
    {
      ConstantExpression constantExpression = Expression.Constant ("test");
      ISelectProjectionParser expectedParser = _selectProjectionParserRegistry.GetParsers (typeof (ConstantExpression)).First();

      Assert.AreSame (expectedParser, _selectProjectionParserRegistry.GetParser (constantExpression));
    }
  }
}
