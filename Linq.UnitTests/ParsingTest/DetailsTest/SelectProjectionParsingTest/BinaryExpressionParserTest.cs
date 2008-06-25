using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class BinaryExpressionParserTest
  {
    private QueryModel _queryModel;
    private ClauseFieldResolver _resolver;
    private IColumnSource _fromSource;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private MemberExpression _memberExpression1;
    private MemberExpression _memberExpression2;
    private MemberExpression _memberExpression3;
    private List<FieldDescriptor> _fieldDescriptors;
    private SelectProjectionParserRegistry _parserRegistry;

    [SetUp]
    public void SetUp ()
    {
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new SelectFieldAccessPolicy ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      _memberExpression1 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      _memberExpression2 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));
      _memberExpression3 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));
      _fieldDescriptors = new List<FieldDescriptor> ();
      _parserRegistry = new SelectProjectionParserRegistry (_queryModel,StubDatabaseInfo.Instance, new JoinedTableContext(), new ParseMode());
      _parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _parserRegistry.RegisterParser (typeof(ParameterExpression), new ParameterExpressionParser (_queryModel, _resolver));
      _parserRegistry.RegisterParser (typeof(MemberExpression), new MemberExpressionParser (_queryModel, _resolver));
      _parserRegistry.RegisterParser (typeof(MethodCallExpression), new MethodCallExpressionParser (_parserRegistry));
    }

    [Test]
    public void ParseWithAdd ()
    {
      MethodInfo methodInfo = typeof(string).GetMethod("Concat",new Type[] { typeof(string), typeof(string) });
      BinaryExpression binaryExpression = Expression.Add (_memberExpression1, _memberExpression2, methodInfo);
      
      //expectedResult
      Column column1 = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column1 };
      Column column2 = new Column (_fromSource, "LastColumn");
      List<IEvaluation> c2 = new List<IEvaluation> { column2 };
      BinaryEvaluation expectedResult = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);

      //BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel, e => e == _memberExpression1 ? c1 : c2 );
      BinaryExpressionParser binaryExpressionParser = 
        new BinaryExpressionParser (_queryModel.GetExpressionTree(), _parserRegistry);

      //result
     List<IEvaluation> result = binaryExpressionParser.Parse (binaryExpression, _fieldDescriptors);

     Assert.AreEqual (expectedResult, result[0]);
    }

    [Test]
    public void CheckNodeTypeMap ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("ID"));
      BinaryExpression binaryExpression1 = Expression.Add (memberExpression, memberExpression);
      BinaryExpression binaryExpression2 = Expression.Divide (memberExpression, memberExpression);
      BinaryExpression binaryExpression3 = Expression.Modulo (memberExpression, memberExpression);
      BinaryExpression binaryExpression4 = Expression.Multiply (memberExpression, memberExpression);
      BinaryExpression binaryExpression5 = Expression.Subtract (memberExpression, memberExpression);

      Column column = new Column (_fromSource, "IDColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };
      //BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel, e => e == _memberExpression1 ? c1 : c1);
      BinaryExpressionParser binaryExpressionParser = 
        new BinaryExpressionParser (_queryModel.GetExpressionTree(), _parserRegistry);

      BinaryEvaluation.EvaluationKind evaluationKind;
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression1.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression2.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression3.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression4.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression5.NodeType, out evaluationKind));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Add, Divide, Modulo, Multiply, Subtract for binary expression "
      +"in select projection, found AddChecked.")]
    public void CheckExceptionHandling ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("ID"));
      BinaryExpression binaryExpression = Expression.AddChecked (memberExpression, memberExpression);
      Column column = new Column (_fromSource, "IDColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };
      //BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel, e => e == memberExpression ? c1 : c1);
      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel.GetExpressionTree(), _parserRegistry);
      binaryExpressionParser.Parse (binaryExpression, _fieldDescriptors);
    }
  }
}