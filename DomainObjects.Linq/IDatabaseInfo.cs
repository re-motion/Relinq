using System;

namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IDatabaseInfo
  {
    string GetTableName (Type querySourceType);
  }
}