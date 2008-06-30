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
  public class NewExpressionParserTest : DetailParserTestBase
  {
    private MemberExpression _memberExpression1;
    private MemberExpression _memberExpression2;
    private MemberExpression _memberExpression3;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private ClauseFieldResolver _resolver;
    private IColumnSource _fromSource;
    private SelectProjectionParserRegistry _parserRegistry;

    public override void SetUp ()
    {
      base.SetUp();

      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      
      _memberExpression1 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      _memberExpression2 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));
      _memberExpression3 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));

      _parserRegistry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode());
      _parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _parserRegistry.RegisterParser (typeof(ParameterExpression), new ParameterExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof(MemberExpression), new MemberExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof(MethodCallExpression), new MethodCallExpressionParser (_parserRegistry));
      _parserRegistry.RegisterParser (typeof(NewExpression), new NewExpressionParser (_parserRegistry));
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
      Column column = new Column (_fromSource, "FirstColumn");
      NewObject newObject = new NewObject(constructorInfo, new IEvaluation[] {column});
     

      NewExpressionParser parser = new NewExpressionParser (_parserRegistry);
      IEvaluation result = parser.Parse (newExpression, ParseContext);

      Assert.That (result, Is.EqualTo(newObject));
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
      NewExpression outerExpression = Expression.New (constructorInfo1, _memberExpression3, innerExpression);

      //expectedResult
      Column column1 = new Column (_fromSource, "FirstColumn");
      Column column2 = new Column (_fromSource, "LastColumn");

      NewObject expectedInnerNewObject = new NewObject (constructorInfo2, new IEvaluation[] {column1, column2});
      NewObject expectedOuterNewObject = new NewObject (constructorInfo1, new IEvaluation[] {column1, expectedInnerNewObject });
      
      
      NewExpressionParser parser = new NewExpressionParser (_parserRegistry);
      IEvaluation result = parser.Parse (outerExpression, ParseContext);

      Assert.That (result, Is.EqualTo (expectedOuterNewObject));
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