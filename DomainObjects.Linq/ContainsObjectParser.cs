using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Utilities;
using Remotion.Data.Linq;

namespace Remotion.Data.DomainObjects.Linq
{
  // source: where o.OrderItems.ContainsObject (myOrderItem)
  // transformed: where (from oi in DataContext.Entity<OrderItem> () where oi.Order == o select oi).Contains (myOrderItem)
  // SQL: WHERE @1 IN (SELECT [oi].[ID] FROM [OrderItem] [oi] WHERE (([oi].[OrderID] IS NULL AND [o].[ID] IS NULL) OR [oi].[OrderID] = [o].[ID]))
  public class ContainsObjectParser : IWhereConditionParser
  {
    private static readonly MethodInfo s_genericContainsMethod =
        ParserUtility.GetMethod (() => Queryable.Contains (null, (object) null)).GetGenericMethodDefinition();
    private static readonly MethodInfo s_containsObjectMethod =
        ParserUtility.GetMethod (() => ((DomainObjectCollection) null).ContainsObject (null));
    private static readonly MethodInfo s_genericDataContextEntityMethod = 
        ParserUtility.GetMethod (() => DataContext.Entity<DomainObject>()).GetGenericMethodDefinition();

    private readonly WhereConditionParserRegistry _registry;

    public ContainsObjectParser (WhereConditionParserRegistry registry)
    {
      _registry = registry;
    }

    public bool CanParse (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      MethodCallExpression methodCallExpression = expression as MethodCallExpression;
      return methodCallExpression != null && methodCallExpression.Method == s_containsObjectMethod;
    }

    public ICriterion Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("fieldDescriptors", parseContext);

      MethodCallExpression containsObjectCallExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (
          expression, "ContainsObject parser", expression);
      
      SubQueryExpression subQueryExpression = CreateEqualSubQuery (containsObjectCallExpression);
      MethodCallExpression containsExpression = CreateExpressionForContainsParser (subQueryExpression, containsObjectCallExpression.Arguments[0]);
      return _registry.GetParser (containsExpression).Parse (containsExpression, parseContext);
    }

    public SubQueryExpression CreateEqualSubQuery (MethodCallExpression containsObjectCallExpression)
    {
      ArgumentUtility.CheckNotNull ("containsObjectCallExpression", containsObjectCallExpression);
      QueryModel queryModel = CreateQueryModel (containsObjectCallExpression);
      SubQueryExpression subQuery = new SubQueryExpression (queryModel);
      return subQuery;
    }

    public QueryModel CreateQueryModel (MethodCallExpression methodCallExpression)
    {
      Type containsParameterType = methodCallExpression.Arguments[0].Type;
      MemberExpression collectionExpression = ParserUtility.GetTypedExpression<MemberExpression> (
          methodCallExpression.Object, "object on which ContainsObject is called", methodCallExpression);
      PropertyInfo collectionProperty = 
          ParserUtility.GetTypedExpression<PropertyInfo> (collectionExpression.Member, "member on which ContainsObject is called", methodCallExpression);
      PropertyInfo foreignKeyProperty = GetForeignKeyProperty (collectionProperty);

      MainFromClause mainFromClause = CreateFromClause(containsParameterType);
      WhereClause whereClause = CreateWhereClause (mainFromClause, foreignKeyProperty, collectionExpression.Expression);
      SelectClause selectClause = CreateSelectClause (whereClause, mainFromClause.Identifier);

      QueryModel queryModel = new QueryModel (typeof (IQueryable<>).MakeGenericType (containsParameterType), mainFromClause, selectClause);
      queryModel.AddBodyClause (whereClause);
      return queryModel;
    }

    // from oi in DataContext.Entity<OrderItem>
    public MainFromClause CreateFromClause (Type containsParameterType)
    {
      ArgumentUtility.CheckNotNull ("containsParameterType", containsParameterType);
      string identifierName = "<<generated>>" + Guid.NewGuid().ToString("N");
      ParameterExpression identifier = Expression.Parameter (containsParameterType, identifierName);
    
      MethodInfo entityMethod = s_genericDataContextEntityMethod.MakeGenericMethod (containsParameterType);
      object queryable = entityMethod.Invoke(null, null);
      Expression querySource = Expression.Constant (queryable);

      return new MainFromClause (identifier, querySource);
    }

    public WhereClause CreateWhereClause (MainFromClause fromClause, PropertyInfo foreignKeyProperty, Expression queriedObject)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("foreignKeyProperty", foreignKeyProperty);
      ArgumentUtility.CheckNotNull ("queriedObject", queriedObject);
      return new WhereClause (fromClause, CreateWhereComparison(fromClause.Identifier, foreignKeyProperty, queriedObject));
    }

    public LambdaExpression CreateWhereComparison (ParameterExpression fromIdentifier, PropertyInfo foreignKeyProperty, Expression queriedObject)
    {
      ArgumentUtility.CheckNotNull ("fromIdentifier", fromIdentifier);
      ArgumentUtility.CheckNotNull ("foreignKeyProperty", foreignKeyProperty);
      ArgumentUtility.CheckNotNull ("queriedObject", queriedObject);
      Expression left = Expression.MakeMemberAccess (fromIdentifier, foreignKeyProperty);
      Expression right = queriedObject;
      BinaryExpression binaryExpression = Expression.Equal (left, right);
      return Expression.Lambda (binaryExpression);
    }

    public SelectClause CreateSelectClause (WhereClause whereClause, ParameterExpression fromIdentifier)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("fromIdentifier", fromIdentifier);

      LambdaExpression projection = Expression.Lambda(fromIdentifier);
      return new SelectClause (whereClause, projection, false);
    }


    public PropertyInfo GetForeignKeyProperty (PropertyInfo collectionProperty) // Order.OrderItems
    {
      ArgumentUtility.CheckNotNull ("collectionProperty", collectionProperty);
      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions.GetMandatory (collectionProperty.DeclaringType);
      string propertyName = MappingConfiguration.Current.NameResolver.GetPropertyName (collectionProperty);
      IRelationEndPointDefinition collectionEndPoint = classDefinition.GetMandatoryRelationEndPointDefinition (propertyName); // Order.OrderItems
      IRelationEndPointDefinition foreignKeyEndPoint = collectionEndPoint.RelationDefinition.GetOppositeEndPointDefinition (collectionEndPoint); // OrderItem.Order

      return MappingConfiguration.Current.NameResolver.GetProperty (foreignKeyEndPoint.ClassDefinition.ClassType, foreignKeyEndPoint.PropertyName);
      
    }

    public MethodCallExpression CreateExpressionForContainsParser (SubQueryExpression subQueryExpression, Expression queryParameterExpression)
    {
      MethodInfo concreteContainsObjectMethod = s_genericContainsMethod.MakeGenericMethod (queryParameterExpression.Type);
      return Expression.Call (concreteContainsObjectMethod, subQueryExpression, queryParameterExpression);
    }
  }
}