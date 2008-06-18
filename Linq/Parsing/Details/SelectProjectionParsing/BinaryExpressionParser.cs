using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class BinaryExpressionParser : ISelectProjectionParser
  {
    private readonly Expression _expressionTreeRoot;
    private readonly SelectProjectionParserRegistry _parserRegistry;

    public Dictionary<ExpressionType, BinaryEvaluation.EvaluationKind> NodeTypeMap { get; private set; }

    public BinaryExpressionParser (Expression expressionTreeRoot, SelectProjectionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
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

      List<IEvaluation> leftSide = _parserRegistry.GetParser (binaryExpression.Left).Parse (binaryExpression.Left, fieldDescriptorCollection);
      List<IEvaluation> rightSide = _parserRegistry.GetParser (binaryExpression.Right).Parse (binaryExpression.Right, fieldDescriptorCollection);

      BinaryEvaluation.EvaluationKind evaluationKind;
      if (!NodeTypeMap.TryGetValue(binaryExpression.NodeType, out evaluationKind))
      {
        throw ParserUtility.CreateParserException(GetSupportedNodeTypeString(), binaryExpression.NodeType,
                                                  "binary expression in select projection",
                                                  _expressionTreeRoot);
      }
      return new List<IEvaluation> {new BinaryEvaluation(leftSide[0], rightSide[0], evaluationKind)};
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((BinaryExpression) expression, fieldDescriptors);
    }

    private string GetSupportedNodeTypeString()
    {
      return SeparatedStringBuilder.Build(", ", NodeTypeMap.Keys);
    }

    public bool CanParse(Expression expression)
    {
      return expression is BinaryExpression;
    }
  }
}