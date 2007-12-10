using System;
using System.Reflection;

namespace Rubicon.Data.Linq
{
  public interface IDatabaseInfo
  {
    string GetTableName (Type querySourceType);
    string GetColumnName (MemberInfo member);
  }
}