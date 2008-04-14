using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.SelectEProjectionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;
using Rubicon.Data.Linq.UnitTests.VisitorTest.ExpressionTreeVisitorTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class NewExpressionParserTest
  {
    [Test]
    public void ParseMemberExpressionInNewExpression ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new SelectFieldAccessPolicy ());

      List<FieldDescriptor> fieldDescriptors = new List<FieldDescriptor> ();
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

     
      //create new expression
      Type constructorType = typeof(DummyClass);
      Type[] types = new Type[1];
      types[0] = typeof (string);
      ConstructorInfo constructorInfo =  
        constructorType.GetConstructor(BindingFlags.Instance | BindingFlags.Public,null,CallingConventions.Any,types,null);

      MemberExpression memberExpression1 = Expression.MakeMemberAccess(parameter, typeof (Student).GetProperty ("First"));
      MemberExpression memberExpression2 = Expression.MakeMemberAccess(parameter, typeof (Student).GetProperty ("Last"));
      NewExpression newExpression = Expression.New (constructorInfo, new[] {memberExpression1});

      //expectedResult
      Column column1 = new Column (fromSource, "FirstColumn");
      List<IEvaluation> expected1 = new List<IEvaluation> { column1};
      Column column2 = new Column (fromSource, "LastColumn");
      List<IEvaluation> expected2 = new List<IEvaluation> { column2 };

      //Func<Expression,List<IEvaluation>>
      NewExpressionParser parser = new NewExpressionParser (queryModel, resolver, e => e == memberExpression1 ? expected1 : expected2);
      List<IEvaluation> result = parser.Parse (newExpression, fieldDescriptors);

      Assert.That (result, Is.EqualTo (expected1));
    }

    [Test]
    [Ignore]
    public void ParseNewExpressionInNewExpression ()
    {

    }
  }

  public class DummyClass
  {
    public DummyClass (string arg1) { }
  }
}