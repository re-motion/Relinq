using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    private ParameterExpression _parameter;
    private IColumnSource _fromSource;
    private MainFromClause _fromClause;
    private QueryModel _queryModel;
    private List<FieldDescriptor> _fieldDescriptors;

    [SetUp]
    public void SetUp()
    {
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _fieldDescriptors = new List<FieldDescriptor> ();
    }


    [Test]
    public void CreateMethodCallEvaluation ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] {  });      
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo);

      //expected Result
      Column column = new Column (_fromSource, "FirstColumn");
      MethodCallEvaluation expected = new MethodCallEvaluation(methodInfo,column,null);
      
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, e => column);

      //result
      MethodCallEvaluation result = methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);

      Assert.IsEmpty (result.EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, result.EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, result.EvaluationParameter);
    }

    [Test]
    public void CreateEMethodCall_WithoutExpression ()
    {
      MethodInfo methodInfo = typeof (MethodCallExpressionParserTest).GetMethod ("Test");
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo);

      Column column = new Column (_fromSource, "FirstColumn");
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, e => null);
      methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);
    }

    [Test]
    public void CreateMethodCall_WithOneArgument ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("Remove", new Type[] { typeof(int) });
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo, Expression.Constant (5));

      //expected Result
      Column column = new Column (_fromSource, "FirstColumn");
      Constant item = new Constant(5);
      List<IEvaluation> arguments = new List<IEvaluation> { item};
      MethodCallEvaluation expected = new MethodCallEvaluation (methodInfo, column, arguments);

      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, e => e == memberExpression ? (IEvaluation) column : item);

      //result
      MethodCallEvaluation result = methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);

      Assert.AreEqual (result.EvaluationArguments, expected.EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, result.EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, result.EvaluationParameter);
    }

    public static void Test ()
    {
    }

  }
}