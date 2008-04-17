using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
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
    }

    [Test]
    public void ParseWithAdd ()
    {
      MethodInfo methodInfo = typeof(string).GetMethod("Concat",new Type[] { typeof(string), typeof(string) });
      BinaryExpression binaryExpression = Expression.Add (_memberExpression1, _memberExpression2, methodInfo);
      
      //expectedResult
      Column column1 = new Column (_fromSource, "FirstColumn");
      Column column2 = new Column (_fromSource, "LastColumn");
      BinaryEvaluation expectedResult = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);

      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel, e => e == _memberExpression1 ? column1 : column2 );

      //result
     IEvaluation result = binaryExpressionParser.Parse (binaryExpression, _fieldDescriptors);

     Assert.AreEqual (expectedResult, result);
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
      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel, e => e == _memberExpression1 ? column : column);

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
      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_queryModel, e => e == memberExpression ? column : column);
      binaryExpressionParser.Parse (binaryExpression, _fieldDescriptors);
    }
  }
}