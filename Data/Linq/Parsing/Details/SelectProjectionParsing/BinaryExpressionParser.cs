/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class BinaryExpressionParser : ISelectProjectionParser
  {
    private readonly SelectProjectionParserRegistry _parserRegistry;

    public Dictionary<ExpressionType, BinaryEvaluation.EvaluationKind> NodeTypeMap { get; private set; }

    public BinaryExpressionParser (SelectProjectionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

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

    public virtual IEvaluation Parse (BinaryExpression binaryExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("binaryExpression", binaryExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      IEvaluation leftSide = _parserRegistry.GetParser (binaryExpression.Left).Parse (binaryExpression.Left, parseContext);
      IEvaluation rightSide = _parserRegistry.GetParser (binaryExpression.Right).Parse (binaryExpression.Right, parseContext);

      BinaryEvaluation.EvaluationKind evaluationKind;
      if (!NodeTypeMap.TryGetValue(binaryExpression.NodeType, out evaluationKind))
      {
        throw ParserUtility.CreateParserException(GetSupportedNodeTypeString(), binaryExpression.NodeType,
                                                  "binary expression in select projection",
                                                  parseContext.ExpressionTreeRoot);
      }
      return new BinaryEvaluation(leftSide, rightSide, evaluationKind);
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((BinaryExpression) expression, parseContext);
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
