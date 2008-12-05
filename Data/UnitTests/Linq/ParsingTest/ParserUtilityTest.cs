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
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using System.Reflection;

namespace Remotion.Data.UnitTests.Linq.ParsingTest
{
  [TestFixture]
  public class ParserUtilityTest
  {
    [Test]
    public void GetTypedExpression()
    {
      Expression sourceExpression = ExpressionHelper.CreateNewIntArrayExpression();
      NewArrayExpression expression = ParserUtility.GetTypedExpression<NewArrayExpression> (sourceExpression,
          "...", ExpressionHelper.CreateExpression());
      Assert.AreSame (sourceExpression, expression);
    }

    [Test]
    [ExpectedException (typeof (ParserException),
        ExpectedMessage = "Expected NewArrayExpression for source expression, found i (ParameterExpression).")]
    public void GetTypedExpression_InvalidType ()
    {
      Expression sourceExpression = ExpressionHelper.CreateParameterExpression();
      ParserUtility.GetTypedExpression<NewArrayExpression> (sourceExpression, "source expression", ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void CheckMethodCallExpression()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression(ExpressionHelper.CreateQuerySource());
      string result = ParserUtility.CheckMethodCallExpression (selectExpression, selectExpression, "SelectMany", "Select", "Where");
      Assert.AreEqual ("Select", result);
      result = ParserUtility.CheckMethodCallExpression (selectExpression, selectExpression, "Select");
      Assert.AreEqual ("Select", result);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected one of 'SelectMany, Where', but found 'Select' at "
        + "TestQueryable<Student>().Select(s => s) in tree TestQueryable<Student>().Select(s => s).")]
    public void CheckMethodCallExpression_InvalidName ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckMethodCallExpression (selectExpression, selectExpression, "SelectMany", "Where");
    }

    [Test]
    public void CheckNumberOfArguments_Succeed ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckNumberOfArguments (selectExpression, "Select", 2, ExpressionHelper.CreateExpression ());
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for Select method call, found 2 arguments.")]
    public void CheckNumberOfArguments_Fail ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckNumberOfArguments (selectExpression, "Select", 1, ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void CheckParameterType_Succeed ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckParameterType<ConstantExpression> (selectExpression, "Select", 0, ExpressionHelper.CreateExpression ());
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression for argument 0 of Select method call, found " 
        + "ConstantExpression (TestQueryable<Student>()).")]
    public void CheckParameterType_Fail ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckParameterType<ParameterExpression> (selectExpression, "Select", 0, ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void GetMethod ()
    {
      MethodInfo method = ParserUtility.GetMethod (() => "x".ToUpper());
      Assert.That (method, Is.EqualTo (typeof (string).GetMethod ("ToUpper", new Type[0])));
    }
  }
}
