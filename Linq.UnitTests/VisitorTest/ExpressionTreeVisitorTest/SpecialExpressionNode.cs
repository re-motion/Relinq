using System;
using System.Linq.Expressions;
namespace Remotion.Data.Linq.UnitTests.VisitorTest.ExpressionTreeVisitorTest
{
  public class SpecialExpressionNode : Expression
  {
    public SpecialExpressionNode (ExpressionType nodeType, Type type)
        : base (nodeType, type)
    {
    }
  }
}