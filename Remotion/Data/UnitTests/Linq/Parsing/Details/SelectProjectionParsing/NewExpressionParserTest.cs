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
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.Details;
using Remotion.Data.Linq.Backend.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Backend.FieldResolving;


namespace Remotion.Data.UnitTests.Linq.Parsing.Details.SelectProjectionParsing
{
  [TestFixture]
  public class NewExpressionParserTest : DetailParserTestBase
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
      _parserRegistry.RegisterParser (typeof (NewExpression), new NewExpressionParser (_parserRegistry));
    }

    [Test]
    public void ParseMemberExpressionInNewExpression ()
    {
      Type constructorType = typeof (DummyClass);
      var types = new Type[1];
      types[0] = typeof (string);
      ConstructorInfo constructorInfo = constructorType.GetConstructor (
          BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types, null);
      NewExpression newExpression = Expression.New (constructorInfo, new[] { Student_First_Expression });

      //expectedResult
      var column = new Column (_fromSource, "FirstColumn");
      var newObject = new NewObject (constructorInfo, new IEvaluation[] { column });


      var parser = new NewExpressionParser (_parserRegistry);
      IEvaluation result = parser.Parse (newExpression, ParseContext);

      Assert.That (result, Is.EqualTo (newObject));
    }

    [Test]
    public void ParseNewExpressionInNewExpression ()
    {
      Type constructorType1 = typeof (DummyClass);
      var types1 = new[] { typeof (string), typeof (DoubleString) };

      ConstructorInfo constructorInfo1 = constructorType1.GetConstructor (
          BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types1, null);

      var types2 = new[] { typeof (string), typeof (string) };
      ConstructorInfo constructorInfo2 = typeof (DoubleString).GetConstructor (
          BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types2, null);
      NewExpression innerExpression = Expression.New (constructorInfo2, Student_First_Expression, Student_Last_Expression);
      NewExpression outerExpression = Expression.New (constructorInfo1, Student_First_Expression, innerExpression);

      //expectedResult
      var column1 = new Column (_fromSource, "FirstColumn");
      var column2 = new Column (_fromSource, "LastColumn");

      var expectedInnerNewObject = new NewObject (constructorInfo2, new IEvaluation[] { column1, column2 });
      var expectedOuterNewObject = new NewObject (constructorInfo1, new IEvaluation[] { column1, expectedInnerNewObject });


      var parser = new NewExpressionParser (_parserRegistry);
      IEvaluation result = parser.Parse (outerExpression, ParseContext);

      Assert.That (result, Is.EqualTo (expectedOuterNewObject));
    }
  }

  public class DummyClass
  {
    public DummyClass (string arg1)
    {
    }

    public DummyClass (string arg1, DoubleString arg2)
    {
    }
  }

  public class DoubleString
  {
    public DoubleString (string arg1, string arg2)
    {
    }
  }
}
