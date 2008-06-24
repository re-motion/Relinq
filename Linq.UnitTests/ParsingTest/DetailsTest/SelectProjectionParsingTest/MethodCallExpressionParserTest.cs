using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

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
    private SelectProjectionParserRegistry _parserRegistry;
    private ClauseFieldResolver _resolver;

    [SetUp]
    public void SetUp()
    {
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _fieldDescriptors = new List<FieldDescriptor> ();
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new SelectFieldAccessPolicy());
      _parserRegistry = 
        new SelectProjectionParserRegistry (_queryModel, StubDatabaseInfo.Instance, new JoinedTableContext(), new ParseContext());
      _parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _parserRegistry.RegisterParser (typeof(ParameterExpression), new ParameterExpressionParser (_queryModel,_resolver));
      _parserRegistry.RegisterParser (typeof(MemberExpression), new MemberExpressionParser (_queryModel, _resolver));
      _parserRegistry.RegisterParser (typeof(MethodCallExpression), new MethodCallExpressionParser (_parserRegistry));
    }


    [Test]
    public void CreateMethodCallEvaluation ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] {  });      
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo);

      //expected Result
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };
      MethodCallEvaluation expected = new MethodCallEvaluation(methodInfo,column,null);
      
      //MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_queryModel, e => c1);
      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_parserRegistry);

      //result
      //MethodCallEvaluation result = methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);
      List<IEvaluation> result = methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);

      Assert.IsEmpty (((MethodCallEvaluation)result[0]).EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, ((MethodCallEvaluation) result[0]).EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, ((MethodCallEvaluation) result[0]).EvaluationParameter);
    }

    [Test]
    public void CreateMethodCall_WithOneArgument ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));

      MethodInfo methodInfo = typeof (string).GetMethod ("Remove", new Type[] { typeof(int) });
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo, Expression.Constant (5));

      //expected Result
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };
      Constant item = new Constant(5);
      List<IEvaluation> item1 = new List<IEvaluation> { item };
      List<IEvaluation> arguments = new List<IEvaluation> { item};
      MethodCallEvaluation expected = new MethodCallEvaluation (methodInfo, column, arguments);

      //MethodCallExpressionParser methodCallExpressionParser = 
        //new MethodCallExpressionParser (_queryModel, e => e == memberExpression ? c1 : item1);
      MethodCallExpressionParser methodCallExpressionParser =
        new MethodCallExpressionParser ( _parserRegistry);

      //result
      List<IEvaluation> result = methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);

      
      Assert.AreEqual (((MethodCallEvaluation)result[0]).EvaluationArguments, expected.EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, ((MethodCallEvaluation) result[0]).EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, ((MethodCallEvaluation) result[0]).EvaluationParameter);
    }

    [Test]
    public void CreateMethodCall_WithStaticMethod ()
    {
      MethodInfo methodInfo = typeof (DateTime).GetMethod ("get_Now");
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo);

      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_parserRegistry);
      List<IEvaluation> result = methodCallExpressionParser.Parse (methodCallExpression, _fieldDescriptors);

      Assert.That (((MethodCallEvaluation) result[0]).EvaluationArguments, Is.Empty);
      Assert.That (((MethodCallEvaluation) result[0]).EvaluationMethodInfo, Is.EqualTo (methodInfo));
      Assert.That (((MethodCallEvaluation) result[0]).EvaluationParameter, Is.Null);
    }
  }
}