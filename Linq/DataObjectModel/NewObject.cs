using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.DataObjectModel
{
  public class NewObject : IEvaluation
  {
    public ConstructorInfo ConstructorInfo { get; private set; }
    public IEvaluation[] ConstructorArguments { get; private set; }

    public NewObject (ConstructorInfo constructorInfo, IEvaluation[] constructorArguments)
    {
      ArgumentUtility.CheckNotNull ("constructorInfo", constructorInfo);
      ArgumentUtility.CheckNotNull ("constructorArguments", constructorArguments);

      ConstructorInfo = constructorInfo;
      ConstructorArguments = constructorArguments;
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      visitor.VisitNewObjectEvaluation (this);
    }

    public override bool Equals (object obj)
    {
      NewObject other = obj as NewObject;
      return other != null 
          && ConstructorInfo.Equals (other.ConstructorInfo) 
          && ConstructorArguments.SequenceEqual (other.ConstructorArguments);
    }

    public override int GetHashCode ()
    {
      return ConstructorInfo.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (ConstructorArguments);
    }

    public override string ToString ()
    {
      return string.Format ("new {0} ({1})", ConstructorInfo.DeclaringType.Name, SeparatedStringBuilder.Build (", ", ConstructorArguments));
    }
  }
}