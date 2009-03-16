// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using System.Reflection;
using System.Linq;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Visitor
{
  public class ExpressionTreeBuildingVisitor : IQueryVisitor
  {
    public Expression ExpressionTree { get; private set; }

    public void VisitQueryModel (QueryModel queryModel)
    {
      queryModel.MainFromClause.Accept (this);
      foreach (IBodyClause bodyClause in queryModel.BodyClauses)
        bodyClause.Accept (this);

      queryModel.SelectOrGroupClause.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      ExpressionTree = fromClause.QuerySource;
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

    public void VisitMemberFromClause (MemberFromClause fromClause)
    {
      throw new System.NotImplementedException();
    }

    public void VisitSubQueryFromClause (SubQueryFromClause clause)
    {
      throw new NotImplementedException();
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
      //ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);

      ////first element of orderBy
      //CreateExpressionOrderBy (orderByClause.OrderingList[0], true);

      ////loop through other elements
      //if (orderByClause.OrderingList.Count > 1)
      //{
      //    for (int i = 1;i <= orderByClause.OrderingList.Count;i++)
      //    {
      //      CreateExpressionOrderBy (orderByClause.OrderingList[i], false);
      //    }
      //}
        //idea: first element of orderByclause.OrderingList is first OrderBy in ExpressionTree
        //Build OrderBy
        //then simple for to iterate through collection (possible wrong way up)
        //make ThenBy or OrderBy (Descending - depending of direction)
        //generic Arguments: SourcePath,Key
        //parameters: ConstantExpression / MemberExpression
        throw new System.NotImplementedException();
    }

    //public void CreateExpressionOrderBy(Ordering ordering,bool first)
    //{
    //  ArgumentUtility.CheckNotNull ("first", first);
    //  ArgumentUtility.CheckNotNull ("ordering", ordering);

    //  Type[] genericArguments =
    //    (from parameter in orderingClause.Expression.Parameters
    //     select parameter.Type).ToArray ();
    //  UnaryExpression orderByExpression = Expression.Quote (orderingClause.Expression);
    //  MethodInfo orderByMethod = null;

    //  if (orderingClause.OrderingDirection == OrderingDirection.Asc)
    //  {
    //    if (first)
    //      orderByMethod = GetQueryMethod ("ThenBy", genericArguments, new Type[] { ExpressionTree.Type, orderByExpression.Type });
    //    else 
    //      orderByMethod = GetQueryMethod ("ThenBy", genericArguments, new Type[] { ExpressionTree.Type, orderByExpression.Type });
    //  }
    //  else
    //  {
    //    if (first)
    //      orderByMethod = GetQueryMethod ("OrderByDescending", genericArguments, new Type[] { ExpressionTree.Type, orderByExpression.Type });
    //    else 
    //      orderByMethod = GetQueryMethod ("ThenByDescending", genericArguments, new Type[] { ExpressionTree.Type, orderByExpression.Type });
    //  }

    //  if (orderByMethod != null)
    //    ExpressionTree = Expression.Call (orderByMethod, ExpressionTree, orderByExpression);
    //}

    public void VisitOrdering (Ordering ordering)
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
      throw new NotImplementedException();
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

      if (matchingsMethods.Length == 0)
      {
        string message = string.Format("The query contains an invalid query method: {0}<{1}>({2})",
            methodName,
            SeparatedStringBuilder.Build (", ", genericArguments, t => t.ToString()),
            SeparatedStringBuilder.Build (", ", parameterTypes, t => t.ToString ()));
        throw new NotSupportedException (message);
      }

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
