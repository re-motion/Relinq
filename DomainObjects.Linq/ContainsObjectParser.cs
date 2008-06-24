using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Utilities;

namespace Remotion.Data.DomainObjects.Linq
{
  public class ContainsObjectParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;
    
    public ContainsObjectParser (Expression expressionTreeRoot, WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, List<FieldDescriptor> fieldDescriptors)
    {
      return CreateContainsObject ((SubQueryExpression) methodCallExpression.Object, methodCallExpression.Arguments[0], fieldDescriptors);
    }

    public ICriterion Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MethodCallExpression) expression, fieldDescriptors);
    }

    public bool CanParse (Expression expression)
    {
      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
      {
        if (methodCallExpression.Method.Name == "ContainsObject")
          return true;
      }
      return false;
    }

    private BinaryCondition CreateContainsObject (SubQueryExpression subQueryExpression, Expression itemExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {

      
      
      return new BinaryCondition (new SubQuery (subQueryExpression.QueryModel, null), _parserRegistry.GetParser (itemExpression).Parse (itemExpression, fieldDescriptorCollection),
          BinaryCondition.ConditionKind.Contains);
    }
  }
}