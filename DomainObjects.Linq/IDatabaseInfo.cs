using System;
using System.Reflection;

namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IDatabaseInfo
  {
    string GetTableName (Type querySourceType);
    string GetColumnName (MemberInfo member);
    bool IsDbColumn (MemberInfo member);
  }
}