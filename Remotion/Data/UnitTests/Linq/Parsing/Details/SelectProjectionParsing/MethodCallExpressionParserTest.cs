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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Backend.Details;
using Remotion.Data.Linq.Backend.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.SelectProjectionParsing
{
  [TestFixture]
  public class MethodCallExpressionParserTest : DetailParserTestBase
  {
    private IColumnSource _fromSource;
    private SelectProjectionParserRegistry _parserRegistry;
    private FieldResolver _resolver;

    public override void SetUp ()
    {
      base.SetUp();

      _fromSource = ParseContext.JoinedTableContext.GetColumnSource (StudentClause);
      QueryModel = ExpressionHelper.CreateQueryModel (StudentClause);
      _resolver = new FieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy());
      _parserRegistry =
          new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode());
      _parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof (MethodCallExpression), new MethodCallExpressionParser (_parserRegistry));
    }


    [Test]
    public void CreateMethodCallEvaluation ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (StudentReference, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] { });
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo);

      //expected Result
      var column = new Column (_fromSource, "FirstColumn");
      var c1 = new List<IEvaluation> { column };
      var expected = new MethodCall (methodInfo, column, new List<IEvaluation>());

      var methodCallExpressionParser = new MethodCallExpressionParser (_parserRegistry);

      //result
      IEvaluation result = methodCallExpressionParser.Parse (methodCallExpression, ParseContext);

      Assert.IsEmpty (((MethodCall) result).Arguments);
      Assert.That (((MethodCall) result).EvaluationMethodInfo, Is.EqualTo (expected.EvaluationMethodInfo));
      Assert.That (((MethodCall) result).TargetObject, Is.EqualTo (expected.TargetObject));
    }

    [Test]
    public void CreateMethodCall_WithOneArgument ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (StudentReference, typeof (Student).GetProperty ("First"));

      MethodInfo methodInfo = typeof (string).GetMethod ("Remove", new[] { typeof (int) });
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo, Expression.Constant (5));

      //expected Result
      var column = new Column (_fromSource, "FirstColumn");
      var c1 = new List<IEvaluation> { column };
      var item = new Constant (5);
      var item1 = new List<IEvaluation> { item };
      var arguments = new List<IEvaluation> { item };
      var expected = new MethodCall (methodInfo, column, arguments);

      var methodCallExpressionParser = new MethodCallExpressionParser (_parserRegistry);

      //result
      IEvaluation result = methodCallExpressionParser.Parse (methodCallExpression, ParseContext);


      Assert.That (expected.Arguments, Is.EqualTo (((MethodCall) result).Arguments));
      Assert.That (((MethodCall) result).EvaluationMethodInfo, Is.EqualTo (expected.EvaluationMethodInfo));
      Assert.That (((MethodCall) result).TargetObject, Is.EqualTo (expected.TargetObject));
    }

    [Test]
    public void CreateMethodCall_WithStaticMethod ()
    {
      MethodInfo methodInfo = typeof (DateTime).GetMethod ("get_Now");
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo);

      var methodCallExpressionParser = new MethodCallExpressionParser (_parserRegistry);
      IEvaluation result = methodCallExpressionParser.Parse (methodCallExpression, ParseContext);

      Assert.That (((MethodCall) result).Arguments, Is.Empty);
      Assert.That (((MethodCall) result).EvaluationMethodInfo, Is.EqualTo (methodInfo));
      Assert.That (((MethodCall) result).TargetObject, Is.Null);
    }
  }
}
