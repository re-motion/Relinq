// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using System.Collections.Generic;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.SelectProjectionParsing
{
  [TestFixture]
  public class ResultModifierParserTest : DetailParserTestBase
  {
    private SelectProjectionParserRegistry _selectRegistry;
    private WhereConditionParserRegistry _whereRegistry;
    private IQueryable<Student> _source;

    public override void SetUp ()
    {
      base.SetUp ();
      _selectRegistry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode ());
      _whereRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      _source = null;
    }

    [Test]
    public void MethodCallEvaluation_Count ()
    {
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (_source));
      MethodCallExpression resultModifierExpression = Expression.Call (methodInfo, Expression.Constant (null, typeof (IQueryable<Student>)));

      ResultModifierParser parser = new ResultModifierParser (_selectRegistry,_whereRegistry);
      Tuple<MethodCall, ICriterion> result = parser.Parse (resultModifierExpression, ParseContext);

      MethodCall expected = new MethodCall (methodInfo, null, new List<IEvaluation>());
      Assert.That (result.A, Is.EqualTo (expected));
    }

    [Test]
    public void MethodCallEvaluation_Take ()
    {
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Take (_source, 2));
      MethodCallExpression resultModifierExpression = Expression.Call (methodInfo, Expression.Constant (null, typeof (IQueryable<Student>)), Expression.Constant(1));

      ResultModifierParser parser = new ResultModifierParser (_selectRegistry, _whereRegistry);
      Tuple<MethodCall, ICriterion> result = parser.Parse (resultModifierExpression, ParseContext);

      MethodCall expected = new MethodCall (methodInfo, null, new List<IEvaluation> {new Constant(1)});
      Assert.That (result.A, Is.EqualTo (expected));
    }

    [Test]
    public void MethodCallEvaluation_SingleWithPredicate ()
    {
      var lambdaExpression = ExpressionHelper.CreateLambdaExpression<Student, bool> (s => s.ID == 5);
      var methodInfo = ParserUtility.GetMethod (() => _source.Single (lambdaExpression));
      UnaryExpression unaryExpression = Expression.Quote (lambdaExpression);

      MethodCallExpression resultModifierExpression = Expression.Call (methodInfo, Expression.Constant (null, typeof (IQueryable<Student>)), unaryExpression);
      
      ResultModifierParser parser = new ResultModifierParser (_selectRegistry, _whereRegistry);
      Tuple<MethodCall, ICriterion> result = parser.Parse (resultModifierExpression, ParseContext);

      MethodCall expectedMethodCall = new MethodCall (methodInfo, null, new List<IEvaluation>());
      Assert.That (result.A, Is.EqualTo (expectedMethodCall));

      IValue left = new Column (new Table ("studentTable", "s"), "IDColumn");
      IValue right = new Constant (5);
      ICriterion expectedCriterion = new BinaryCondition (left, right,BinaryCondition.ConditionKind.Equal);
      Assert.That (result.B, Is.EqualTo (expectedCriterion));
    }

    [Test]
    public void MethodCallEvaluation_SingleWithoutQuote ()
    {
      var methodInfo = ParserUtility.GetMethod (() => _source.Single ());
      MethodCallExpression resultModifierExpression = Expression.Call (methodInfo, Expression.Constant (null, typeof (IQueryable<Student>)));

      ResultModifierParser parser = new ResultModifierParser (_selectRegistry, _whereRegistry);
      Tuple<MethodCall, ICriterion> result = parser.Parse (resultModifierExpression, ParseContext);

      MethodCall expected = new MethodCall (methodInfo, null, new List<IEvaluation> ());
      Assert.That (result.A, Is.EqualTo (expected));
      Assert.That (result.B, Is.Null);
    }


    
  }
}
