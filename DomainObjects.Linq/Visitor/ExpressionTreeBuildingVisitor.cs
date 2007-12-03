using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using System.Reflection;
using System.Linq;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Visitor
{
  public class ExpressionTreeBuildingVisitor : IQueryVisitor
  {
    public Expression ExpressionTree { get; private set; }

    public void VisitQueryExpression (QueryExpression queryExpression)
    {
      queryExpression.FromClause.Accept (this);
      queryExpression.QueryBody.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      ExpressionTree = Expression.Constant (fromClause.QuerySource);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      Type lambdaType = fromClause.ProjectionExpression.GetType().GetGenericArguments()[0];
      Type[] genericArguments = lambdaType.GetGenericArguments();

      UnaryExpression fromExpression = Expression.Quote (fromClause.FromExpression);
      UnaryExpression projectionExpression = Expression.Quote (fromClause.ProjectionExpression);

      MethodInfo selectManyMethod = GetQueryMethod ("SelectMany", genericArguments, new Type[] { ExpressionTree.Type, fromExpression.Type,
          projectionExpression.Type});
      Assertion.IsTrue (selectManyMethod.GetParameters().Length == 3, "query model only supports SelectMany with three arguments");
      ExpressionTree = Expression.Call (selectManyMethod, ExpressionTree, fromExpression, projectionExpression);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitLetClause (LetClause letClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      Type[] genericArguments =
          (from parameter in whereClause.BoolExpression.Parameters
           select parameter.Type).ToArray ();

      UnaryExpression boolExpression = Expression.Quote (whereClause.BoolExpression);

      MethodInfo whereMethod = GetQueryMethod ("Where", genericArguments, new Type[] { ExpressionTree.Type, boolExpression.Type });
      Assertion.IsTrue (whereMethod.GetParameters ().Length == 2, "query model only supports Where with two arguments");
      ExpressionTree = Expression.Call (whereMethod, ExpressionTree, boolExpression);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      MethodCallExpression lastMethodCallExpression = ExpressionTree as MethodCallExpression;
      if (lastMethodCallExpression != null)
      {
        if (lastMethodCallExpression.Method.Name == "SelectMany")
          return;
        if (lastMethodCallExpression.Method.Name == "Where" && selectClause.ProjectionExpression == null)
          return;
      }

      Assertion.IsNotNull (selectClause.ProjectionExpression);

      Type lambdaType = selectClause.ProjectionExpression.GetType ().GetGenericArguments ()[0];
      Type[] genericArguments = lambdaType.GetGenericArguments ();

      UnaryExpression projectionExpression = Expression.Quote (selectClause.ProjectionExpression);

      MethodInfo selectMethod = GetQueryMethod ("Select", genericArguments, new Type[] { ExpressionTree.Type, projectionExpression.Type });
      Assertion.IsTrue (selectMethod.GetParameters ().Length == 2, "query model only supports Select with two arguments");
      ExpressionTree = Expression.Call (selectMethod, ExpressionTree, projectionExpression);
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitQueryBody (QueryBody queryBody)
    {
      foreach (IFromLetWhereClause clause in queryBody.FromLetWhereClauses)
        clause.Accept (this);
      queryBody.SelectOrGroupClause.Accept (this);
    }

    private MethodInfo GetQueryMethod (string methodName, Type[] genericArguments, Type[] parameterTypes)
    {
      MemberInfo[] matchingsMethods =
          typeof (Queryable).FindMembers (MemberTypes.Method, BindingFlags.Public | BindingFlags.Static,
          delegate (MemberInfo currentMember, object filterCriteria)
          {
            return currentMember.Name == methodName
                && IsCompatibleMethod ((MethodInfo) currentMember, genericArguments, parameterTypes);
          }, null);

      Assertion.IsTrue (matchingsMethods.Length == 1);
      MethodInfo method = (MethodInfo) matchingsMethods[0];
      return method.MakeGenericMethod (genericArguments);
    }

    private bool IsCompatibleMethod (MethodInfo method, Type[] genericArgumentTypes, Type[] parameterTypes)
    {
      Type[] genericArguments = method.GetGenericArguments ();
      if (genericArguments.Length != genericArgumentTypes.Length)
        return false;

      method = method.MakeGenericMethod (genericArgumentTypes);

      ParameterInfo[] parameters = method.GetParameters();
      if (parameters.Length != parameterTypes.Length)
        return false;

      for (int i = 0; i < parameters.Length; i++)
      {
        ParameterInfo parameter = parameters[i];
        if (!parameter.ParameterType.IsAssignableFrom (parameterTypes[i]))
          return false;
      }

      return true;
    }
  }
}