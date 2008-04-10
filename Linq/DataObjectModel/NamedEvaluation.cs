using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct NamedEvaluation : IColumnSource
  {
    public NamedEvaluation (string alias,IEvaluation evaluation) : this()
    {
      ArgumentUtility.CheckNotNull ("alias", alias);
      ArgumentUtility.CheckNotNull ("evaluation", evaluation);
      Alias = alias;
      Evaluation = evaluation;
    }
    public IEvaluation Evaluation { get; private set; }

    public string Alias {get; private set; }
    public string AliasString { get { return Alias; }
    }
  }
}