using System;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Mapping;
using Rubicon.Data.Linq;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class DatabaseInfo : IDatabaseInfo
  {
    public static readonly DatabaseInfo Instance = new DatabaseInfo();

    private DatabaseInfo ()
    {
    }

    public Table? GetTable (FromClauseBase fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      Type querySourceType = fromClause.GetQuerySourceType();
      if (!querySourceType.IsGenericType
          || querySourceType.IsGenericTypeDefinition
          || querySourceType.GetGenericTypeDefinition () != typeof (DomainObjectQueryable<>))
        return null;
      else
      {
        Type domainObjectType = querySourceType.GetGenericArguments()[0];
        ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions[domainObjectType];
        if (classDefinition == null)
          return null;
        else
          return new Table(classDefinition.GetEntityName(), fromClause.Identifier.Name);
      }
    }

    public string GetRelatedTableName (MemberInfo relationMember)
    {
      throw new NotImplementedException();
    }

    public string GetColumnName (MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      PropertyInfo property = member as PropertyInfo;
      if (property == null)
        return null;

      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions[property.DeclaringType];
      if (classDefinition == null)
        return null;

      string propertyIdentifier = ReflectionUtility.GetPropertyName (property);
      PropertyDefinition propertyDefinition = classDefinition.GetPropertyDefinition (propertyIdentifier);

      if (propertyDefinition != null)
        return propertyDefinition.StorageSpecificName;
      else
        return null;
    }

    public Tuple<string, string> GetJoinColumns (MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);
      PropertyInfo property = relationMember as PropertyInfo;
      if (property == null)
        return null;

      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions[property.DeclaringType];
      if (classDefinition == null)
        return null;

      string propertyIdentifier = ReflectionUtility.GetPropertyName (property);
      RelationDefinition relationDefinition = classDefinition.GetRelationDefinition (propertyIdentifier);
      if (relationDefinition != null)
      {
        IRelationEndPointDefinition leftEndPoint = relationDefinition.EndPointDefinitions[0];
        IRelationEndPointDefinition rightEndPoint = relationDefinition.EndPointDefinitions[1];

        string leftColumn = GetJoinColumn (leftEndPoint);
        string rightColumn = GetJoinColumn (rightEndPoint);

        return Tuple.NewTuple (leftColumn, rightColumn);
      }
      else
        return null;
    }

    private string GetJoinColumn (IRelationEndPointDefinition endPoint)
    {
      ClassDefinition classDefinition = endPoint.ClassDefinition;
      return endPoint.IsVirtual ? "ID" : classDefinition.GetMandatoryPropertyDefinition (endPoint.PropertyName).StorageSpecificName;
    }
  }
}