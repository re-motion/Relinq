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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest
{
  [TestFixture]
  public class SimpleWhereExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private BodyHelper _bodyWhereHelper;
    private ParseResultCollector _result;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = WhereTestQueryGenerator.CreateSimpleWhereQuery_WhereExpression (_querySource);
      _navigator  = new ExpressionTreeNavigator(_expression);
      _result = new ParseResultCollector (_expression);
      new WhereExpressionParser (true).Parse(_result, _expression);
      _bodyWhereHelper = new BodyHelper (_result.BodyExpressions);
    }

    [Test]
    public void ParsesFromExpressions()
    {
      Assert.IsNotNull (_bodyWhereHelper.FromExpressions);
      Assert.That (_bodyWhereHelper.FromExpressions, Is.EqualTo (new object[] { _expression.Arguments[0] }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _bodyWhereHelper.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _bodyWhereHelper.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_bodyWhereHelper.FromIdentifiers);
      Assert.That (_bodyWhereHelper.FromIdentifiers,
          Is.EqualTo (new object[] { _navigator.Arguments[1].Operand.Parameters[0].Expression }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyWhereHelper.FromIdentifiers[0]);
      Assert.AreEqual ("s", (_bodyWhereHelper.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesBoolExpressions ()
    {
      Assert.IsNotNull (_bodyWhereHelper.WhereExpressions);
      Assert.That (_bodyWhereHelper.WhereExpressions, Is.EqualTo (new object[] { _navigator.Arguments[1].Operand.Expression }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyWhereHelper.WhereExpressions[0]);
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (1, _result.ProjectionExpressions.Count);
      Assert.IsNull (_result.ProjectionExpressions[0]);
    }

    [Test]
    public void ParsesProjectionExpressions_NotTopLevel ()
    {
      _result = new ParseResultCollector (_expression);
      new WhereExpressionParser (false).Parse (_result, _expression);
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (0, _result.ProjectionExpressions.Count);
    }

  }
}
