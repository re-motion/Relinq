using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class BinaryExpressionParser : ISelectProjectionParser<BinaryExpression>, ISelectProjectionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ParserRegistry _parserRegistry;

    public Dictionary<ExpressionType, BinaryEvaluation.EvaluationKind> NodeTypeMap { get; private set; }

    public BinaryExpressionParser(QueryModel queryModel, ParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _queryModel = queryModel;
      _parserRegistry = parserRegistry;

      NodeTypeMap = new Dictionary<ExpressionType, BinaryEvaluation.EvaluationKind>
                      {
                        {ExpressionType.Add, BinaryEvaluation.EvaluationKind.Add},
                        {ExpressionType.Divide, BinaryEvaluation.EvaluationKind.Divide},
                        {ExpressionType.Modulo, BinaryEvaluation.EvaluationKind.Modulo},
                        {ExpressionType.Multiply, BinaryEvaluation.EvaluationKind.Multiply},
                        {ExpressionType.Subtract, BinaryEvaluation.EvaluationKind.Subtract}
                      };
    }

    public virtual List<IEvaluation> Parse (BinaryExpression binaryExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      ArgumentUtility.CheckNotNull ("binaryExpression", binaryExpression);
      ArgumentUtility.CheckNotNull("fieldDescriptorCollection", fieldDescriptorCollection);

      List<IEvaluation> leftSide = GetParser (binaryExpression.Left).Parse (binaryExpression.Left, fieldDescriptorCollection);
      
      List<IEvaluation> rightSide = GetParser (binaryExpression.Right).Parse (binaryExpression.Right, fieldDescriptorCollection);

      BinaryEvaluation.EvaluationKind evaluationKind;
      if (!NodeTypeMap.TryGetValue(binaryExpression.NodeType, out evaluationKind))
      {
        throw ParserUtility.CreateParserException(GetSupportedNodeTypeString(), binaryExpression.NodeType,
                                                  "binary expression in select projection",
                                                  _queryModel.GetExpressionTree());
      }
      return new List<IEvaluation> {new BinaryEvaluation(leftSide[0], rightSide[0], evaluationKind)};
    }

    private string GetSupportedNodeTypeString()
    {
      return SeparatedStringBuilder.Build(", ", NodeTypeMap.Keys);
    }

    public bool CanParse(BinaryExpression binaryExpression)
    {
      return true;
    }

    public List<IEvaluation> Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((BinaryExpression) expression, fieldDescriptors);
    }

    private ISelectProjectionParser GetParser (Expression expression)
    {
      if (expression.GetType () == typeof (ConstantExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((ConstantExpression) expression);
      else if (expression.GetType () == typeof (BinaryExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((BinaryExpression) expression);
      else if (expression.GetType () == typeof (MemberExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((MemberExpression) expression);
      else if (expression.GetType () == typeof (MethodCallExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((MethodCallExpression) expression);
      else if (expression.GetType () == typeof (ParameterExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((ParameterExpression) expression);
      else if (expression.GetType () == typeof (NewExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((NewExpression) expression);
      throw ParserUtility.CreateParserException ("no parser for expression found",expression,"GetParser",
        _queryModel.GetExpressionTree());
    }
  }
}