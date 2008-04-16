using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.SelectEProjectionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;


namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class NewExpressionParserTest
  {
    private MemberExpression _memberExpression1;
    private MemberExpression _memberExpression2;
    private MemberExpression _memberExpression3;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private ClauseFieldResolver _resolver;
    private QueryModel _queryModel;
    private IColumnSource _fromSource;
    private List<FieldDescriptor> _fieldDescriptors;

    [SetUp]
    public void SetUp ()
    {
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _queryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new SelectFieldAccessPolicy ());
      _fieldDescriptors = new List<FieldDescriptor> ();
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      
      _memberExpression1 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      _memberExpression2 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));
      _memberExpression3 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));

    }

    [Test]
    public void ParseMemberExpressionInNewExpression ()
    {
      Type constructorType = typeof (DummyClass);
      Type[] types = new Type[1];
      types[0] = typeof (string);
      ConstructorInfo constructorInfo = constructorType.GetConstructor (BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types, null);
      NewExpression newExpression = Expression.New (constructorInfo, new[] { _memberExpression1 });

      //expectedResult
      Column column1 = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> expected1 = new List<IEvaluation> { column1};
      Column column2 = new Column (_fromSource, "LastColumn");
      List<IEvaluation> expected2 = new List<IEvaluation> { column2 };

      NewExpressionParser parser = new NewExpressionParser (_queryModel, _resolver, e => e == _memberExpression1 ? expected1 : expected2);
      List<IEvaluation> result = parser.Parse (newExpression, _fieldDescriptors);

      Assert.That (result, Is.EqualTo (expected1));
    }

    [Test]
    public void ParseNewExpressionInNewExpression ()
    {
      Type constructorType1 = typeof (DummyClass);
      Type[] types1 = new[] { typeof (string), typeof (DoubleString) };
      
      ConstructorInfo constructorInfo1 = constructorType1.GetConstructor (BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types1, null);

      Type[] types2 = new[] { typeof (string), typeof (string)};
      ConstructorInfo constructorInfo2 = typeof(DoubleString).GetConstructor (BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types2, null);
      NewExpression innerExpression = Expression.New (constructorInfo2, _memberExpression1, _memberExpression2);

      //expectedResult
      Column column1 = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> expected1 = new List<IEvaluation> { column1};
      Column column2 = new Column (_fromSource, "LastColumn");
      List<IEvaluation> expected2 = new List<IEvaluation> { column1, column2 };

      List<IEvaluation> expectedResult = new List<IEvaluation> { column1, column1, column2 };
      
      List<IEvaluation> test = new List<IEvaluation> { column1,column2};

      NewExpression outerExpression = Expression.New (constructorInfo1, _memberExpression3, innerExpression);
      NewExpressionParser parser = new NewExpressionParser (_queryModel, _resolver,
          e => e == _memberExpression3 ? expected1 : expected2);
      List<IEvaluation> result = parser.Parse (outerExpression, _fieldDescriptors);
      
      Assert.That (result, Is.EqualTo (expectedResult));
    }
  }

  public class DummyClass
  {
    public DummyClass (string arg1) { }
    public DummyClass (string arg1, DoubleString arg2) { }
  }

  public class DoubleString
  {
    public DoubleString (string arg1, string arg2) {}
  }

  
}