using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ParameterExpressionParserTest
  {
    [Test]
    public void Parse()
    {
      ParameterExpression parameterExpression = Expression.Parameter (typeof (Student), "s1");
      List<FieldDescriptor> fieldDescriptorCollection = new List<FieldDescriptor>();
      Constant criterion = new Constant (5);
      
      ParameterExpressionParser parser = new ParameterExpressionParser (StubDatabaseInfo.Instance, delegate (Expression expression)
      {
        MemberExpression expectedMemberExpression = Expression.MakeMemberAccess (parameterExpression, typeof (Student).GetProperty ("ID"));
        MemberExpression actualMemberExpression = (MemberExpression) expression;
        Assert.AreEqual (expectedMemberExpression.Expression, actualMemberExpression.Expression);
        Assert.AreEqual (expectedMemberExpression.Member, actualMemberExpression.Member);
        return criterion;
      });
      
      ICriterion actualCriterion = parser.Parse (parameterExpression, fieldDescriptorCollection);
      Assert.AreEqual (criterion, actualCriterion);
    }
  }
}