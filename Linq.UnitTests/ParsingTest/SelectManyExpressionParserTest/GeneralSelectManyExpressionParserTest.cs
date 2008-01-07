using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.SelectManyExpressionParserTest
{
  [TestFixture]
  public class GeneralSelectManyExpressionParserTest
  {

    [Test]
    public void Initialize ()
    {
      IQueryable<Student> querySource1 = ExpressionHelper.CreateQuerySource();
      IQueryable<Student> querySource2 = ExpressionHelper.CreateQuerySource ();
      MethodCallExpression expression = TestQueryGenerator.CreateMultiFromQuery_SelectManyExpression(querySource1,querySource2);

      SelectManyExpressionParser parser = new SelectManyExpressionParser (expression, expression);
      Assert.AreSame (expression, parser.SourceExpression);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected one of 'SelectMany', but found 'Where' at position value"
        + "(Rubicon.Data.Linq.UnitTests.TestQueryable`1[Rubicon.Data.Linq.UnitTests.Student])"
        +".Where(s => (s.Last = \"Garcia\")) in tree value(Rubicon.Data.Linq.UnitTests.TestQueryable`1"
        +"[Rubicon.Data.Linq.UnitTests.Student]).Where(s => (s.Last = \"Garcia\")).")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = TestQueryGenerator.CreateSimpleWhereQuery_WhereExpression (ExpressionHelper.CreateQuerySource ());
      new SelectManyExpressionParser (expression, expression);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected SelectMany call with three arguments for SelectMany expressions, "
        + "found MethodCallExpression (Convert(null).SelectMany(student => null)).")]
    public void Initialize_FromWrongExpressionInWhereExpression ()
    {
      Expression nonCallExpression = Expression.Convert (Expression.Constant (null), typeof (IQueryable<Student>));
      // Get method Queryable.SelectMany with two arguments whose second argument is of type Expression<Func<TSource, IEnumerable<TResult>>>
      // (rather than one of the many other overloads)
      MethodInfo method = (from m in typeof (Queryable).GetMethods () where m.Name == "SelectMany" && m.GetParameters().Length == 2 
                                   && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2 select m).First ();
      method = method.MakeGenericMethod (typeof (Student), typeof (Student));
      MethodCallExpression selectExpression = Expression.Call (method, nonCallExpression, Expression.Lambda (Expression.Constant (null, typeof (IEnumerable<Student>)), Expression.Parameter (typeof (Student), "student")));
      new SelectManyExpressionParser (selectExpression, selectExpression);
    }
  }

  
}