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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Backend.Details;
using Remotion.Data.Linq.Backend.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Backend.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.SelectProjectionParsing
{
  [TestFixture]
  public class BinaryExpressionParserTest : DetailParserTestBase
  {
    private FieldResolver _resolver;
    private IColumnSource _fromSource;
    private SelectProjectionParserRegistry _parserRegistry;

    public override void SetUp ()
    {
      base.SetUp();

      QueryModel = ExpressionHelper.CreateQueryModel (StudentClause);
      _resolver = new FieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy());
      _fromSource = ParseContext.JoinedTableContext.GetColumnSource (StudentClause);
      _parserRegistry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode());
      _parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof (MethodCallExpression), new MethodCallExpressionParser (_parserRegistry));
    }

    [Test]
    public void ParseWithAdd ()
    {
      MethodInfo methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      BinaryExpression binaryExpression = Expression.Add (Student_First_Expression, Student_Last_Expression, methodInfo);

      //expectedResult
      var column1 = new Column (_fromSource, "FirstColumn");
      var column2 = new Column (_fromSource, "LastColumn");
      var expectedResult = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);

      var binaryExpressionParser = new BinaryExpressionParser (_parserRegistry);

      //result
      IEvaluation result = binaryExpressionParser.Parse (binaryExpression, ParseContext);

      Assert.That (result, Is.EqualTo (expectedResult));
    }

    [Test]
    public void CheckNodeTypeMap ()
    {
      BinaryExpression binaryExpression1 = Expression.Add (Student_ID_Expression, Student_ID_Expression);
      BinaryExpression binaryExpression2 = Expression.Divide (Student_ID_Expression, Student_ID_Expression);
      BinaryExpression binaryExpression3 = Expression.Modulo (Student_ID_Expression, Student_ID_Expression);
      BinaryExpression binaryExpression4 = Expression.Multiply (Student_ID_Expression, Student_ID_Expression);
      BinaryExpression binaryExpression5 = Expression.Subtract (Student_ID_Expression, Student_ID_Expression);

      var binaryExpressionParser = new BinaryExpressionParser (_parserRegistry);

      BinaryEvaluation.EvaluationKind evaluationKind;
      Assert.That (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression1.NodeType, out evaluationKind), Is.True);
      Assert.That (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression2.NodeType, out evaluationKind), Is.True);
      Assert.That (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression3.NodeType, out evaluationKind), Is.True);
      Assert.That (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression4.NodeType, out evaluationKind), Is.True);
      Assert.That (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression5.NodeType, out evaluationKind), Is.True);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Add, Divide, Modulo, Multiply, Subtract for binary expression "
                                                                    + "in select projection, found 'AddChecked'.")]
    public void CheckExceptionHandling ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (StudentReference, typeof (Student).GetProperty ("ID"));
      BinaryExpression binaryExpression = Expression.AddChecked (memberExpression, memberExpression);
      var binaryExpressionParser = new BinaryExpressionParser (_parserRegistry);
      binaryExpressionParser.Parse (binaryExpression, ParseContext);
    }
  }
}
