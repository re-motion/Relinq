using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class BinaryExpressionParser
  {
    private readonly QueryModel _queryModel;
    private readonly Func<Expression, IEvaluation> _parsingCall;

    public Dictionary<ExpressionType, BinaryEvaluation.EvaluationKind> NodeTypeMap { get; private set; }
        
    public BinaryExpressionParser (QueryModel queryModel, Func<Expression,IEvaluation> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);
      
      _queryModel = queryModel;
      _parsingCall = parsingCall;

      NodeTypeMap = new Dictionary<ExpressionType, BinaryEvaluation.EvaluationKind> { 
                                                                                        { ExpressionType.Add, BinaryEvaluation.EvaluationKind.Add },
                                                                                        { ExpressionType.Divide, BinaryEvaluation.EvaluationKind.Divide },
                                                                                        { ExpressionType.Modulo, BinaryEvaluation.EvaluationKind.Modulo },
                                                                                        { ExpressionType.Multiply, BinaryEvaluation.EvaluationKind.Multiply },
                                                                                        { ExpressionType.Subtract, BinaryEvaluation.EvaluationKind.Subtract }
                                                                                    };
    }

    public virtual IEvaluation Parse(BinaryExpression binaryExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      ArgumentUtility.CheckNotNull ("binaryExpression", binaryExpression);
      ArgumentUtility.CheckNotNull ("fieldDescriptorCollection", fieldDescriptorCollection);

      IEvaluation leftSide = _parsingCall (binaryExpression.Left);
      IEvaluation rightSide = _parsingCall (binaryExpression.Right);

      BinaryEvaluation.EvaluationKind evaluationKind;
      if (!NodeTypeMap.TryGetValue (binaryExpression.NodeType, out evaluationKind))
      {
        throw ParserUtility.CreateParserException (GetSupportedNodeTypeString(), binaryExpression.NodeType,
            "binary expression in select projection", _queryModel.GetExpressionTree());
      }

      return new BinaryEvaluation (leftSide, rightSide, evaluationKind);
    }

    private string GetSupportedNodeTypeString ()
    {
      return SeparatedStringBuilder.Build (", ", NodeTypeMap.Keys);
    }
  }
}