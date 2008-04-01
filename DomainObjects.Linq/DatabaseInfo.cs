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

    public string GetTableName (FromClauseBase fromClause)
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
          return classDefinition.GetEntityName();
      }
    }

    public string GetRelatedTableName (MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<RelationDefinition, ClassDefinition, string> relationData = GetRelationData (relationMember);
      if (relationData == null)
        return null;

      RelationDefinition relationDefinition = relationData.A;
      ClassDefinition classDefinition = relationData.B;
      string propertyIdentifier = relationData.C;

      return relationDefinition.GetOppositeClassDefinition (classDefinition.ID, propertyIdentifier).GetEntityName();
    }

    public string GetColumnName (MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      PropertyInfo property = member as PropertyInfo;
      if (property == null)
        return null;

      if (property.Name == "ID" && property.DeclaringType == typeof (DomainObject))
        return "ID";

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

    public Tuple<string, string> GetJoinColumnNames (MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<RelationDefinition, ClassDefinition, string> relationData = GetRelationData (relationMember);
      if (relationData == null)
        return null;

      RelationDefinition relationDefinition = relationData.A;
      ClassDefinition classDefinition = relationData.B;
      string propertyIdentifier = relationData.C;

      IRelationEndPointDefinition leftEndPoint = relationDefinition.GetEndPointDefinition (classDefinition.ID, propertyIdentifier);
      IRelationEndPointDefinition rightEndPoint = relationDefinition.GetOppositeEndPointDefinition (leftEndPoint);
     
      string leftColumn = GetJoinColumn (leftEndPoint);
      string rightColumn = GetJoinColumn (rightEndPoint);

      return Tuple.NewTuple (leftColumn, rightColumn);
    }

    public object ProcessWhereParameter (object parameter)
    {
      DomainObject domainObject = parameter as DomainObject;
      if (domainObject != null)
        return domainObject.ID;
      return parameter;
    }

    public MemberInfo GetPrimaryKeyMember (Type entityType)
    {
      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions[entityType];
      if (classDefinition == null)
        return null;
      else
        return typeof (DomainObject).GetProperty ("ID");
    }

    private Tuple<RelationDefinition, ClassDefinition, string> GetRelationData (MemberInfo relationMember)
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
      if (relationDefinition == null)
        return null;
      else
        return Tuple.NewTuple (relationDefinition, classDefinition, propertyIdentifier);
    }

    private string GetJoinColumn (IRelationEndPointDefinition endPoint)
    {
      ClassDefinition classDefinition = endPoint.ClassDefinition;
      return endPoint.IsVirtual ? "ID" : classDefinition.GetMandatoryPropertyDefinition (endPoint.PropertyName).StorageSpecificName;
    }
  }
}