using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class SubQueryExpressionTest
  {
    [Test]
    public void Initialize_DontOverwriteExpressionType ()
    {
      Assert.IsFalse(Enum.IsDefined (typeof (ExpressionType), new SubQueryExpression (ExpressionHelper.CreateQueryModel()).NodeType));
    }

    [Test]
    public void Initialize_CorrectType ()
    {
      QueryModel model = new QueryModel (typeof (int), ExpressionHelper.CreateMainFromClause(), ExpressionHelper.CreateSelectClause());
      SubQueryExpression sqe = new SubQueryExpression (model);

      Assert.AreEqual (typeof (int), sqe.Type);
    }


  }
}