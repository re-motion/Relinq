using System;
using System.Reflection;
using Rubicon.Data.DomainObjects.Mapping;
using Rubicon.Data.Linq;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class DatabaseInfo : IDatabaseInfo
  {
    public static readonly DatabaseInfo Instance = new DatabaseInfo();

    private DatabaseInfo ()
    {
    }

    public string GetTableName (Type querySourceType)
    {
      ArgumentUtility.CheckNotNull ("querySourceType", querySourceType);
      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions[querySourceType];
      if (classDefinition == null)
        return null;
      else
        return classDefinition.GetEntityName();
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
  }
}