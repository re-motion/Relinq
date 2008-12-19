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
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
{
  [TestFixture]
  public class SimpleFromSourceExpressionParserTest
  {
    private SimpleFromSourceExpressionParser _parser;
    private ParameterExpression _potentialFromIdentifier;

    [SetUp]
    public void SetUp ()
    {
      _parser = new SimpleFromSourceExpressionParser ();
      _potentialFromIdentifier = ExpressionHelper.CreateParameterExpression ();
    }

    [Test]
    public void SimpleSource_Constant ()
    {
      Expression constantExpression = Expression.Constant (ExpressionHelper.CreateQuerySource (), typeof (IQueryable<Student>));
      ParseResultCollector result = new ParseResultCollector (constantExpression);
      _parser.Parse (result, constantExpression, _potentialFromIdentifier, "bla");

      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (_potentialFromIdentifier, ((FromExpressionData) result.BodyExpressions[0]).Identifier);
      Assert.AreEqual (constantExpression, ((FromExpressionData) result.BodyExpressions[0]).TypedExpression);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Query sources cannot be null.")]
    public void SimpleSource_ConstantNull ()
    {
      Expression constantExpression = Expression.Constant (null, typeof (IQueryable<Student>));
      ParseResultCollector result = new ParseResultCollector (constantExpression);
      _parser.Parse (result, constantExpression, _potentialFromIdentifier, "bla");
    }

    [Test]
    public void SimpleSource_MemberExpression ()
    {
      Expression memberExpression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Student)), typeof (Student).GetProperty ("Scores"));
      ParseResultCollector result = new ParseResultCollector (memberExpression);
      _parser.Parse (result, memberExpression, _potentialFromIdentifier, "bla");

      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (_potentialFromIdentifier, ((FromExpressionData) result.BodyExpressions[0]).Identifier);
      Assert.AreEqual (memberExpression, ((FromExpressionData) result.BodyExpressions[0]).TypedExpression);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    public void SimpleSource_MethodCall ()
    {
      Expression callExpression = Expression.Call (typeof (ExpressionHelper).GetMethod ("CreateQuerySource", new Type[0]));
      ParseResultCollector result = new ParseResultCollector (callExpression);
      _parser.Parse (result, callExpression, _potentialFromIdentifier, "bla");

      Assert.AreEqual (1, result.BodyExpressions.Count);
      Assert.AreEqual (_potentialFromIdentifier, ((FromExpressionData) result.BodyExpressions[0]).Identifier);
      Assert.IsInstanceOfType (typeof (TestQueryable<Student>), ((ConstantExpression) ((FromExpressionData) result.BodyExpressions[0]).TypedExpression).Value);
      Assert.That (result.ProjectionExpressions, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Parsing of expression 'TestQueryable<Student>()' is not supported. The " 
        + "expression was interpreted as a from source, but there is no from identifier matching it in expression tree 'TestQueryable<Student>()'.")]
    public void NoPotentialFromIdentifier ()
    {
      Expression constantExpression = Expression.Constant (ExpressionHelper.CreateQuerySource (), typeof (IQueryable<Student>));
      ParseResultCollector result = new ParseResultCollector (constantExpression);
      _parser.Parse (result, constantExpression, null, "bla");

    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Constant, MemberAccess, or Call expression for xy, found i (ParameterExpression).")]
    public void InvalidSource ()
    {
      Expression constantExpression = Expression.Parameter (typeof (int), "i");
      _parser.Parse (new ParseResultCollector (constantExpression), _potentialFromIdentifier, _potentialFromIdentifier, "xy");
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The expression 'WriteLine()' could not be evaluated as a query source because it "
        + "cannot be compiled: Argument types do not match")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = Expression.Call (typeof (Console), "WriteLine", Type.EmptyTypes);
      _parser.Parse (new ParseResultCollector (expression), expression, _potentialFromIdentifier, "whatever");
    }
  }
}
