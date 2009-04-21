// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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

    public NewObject (ConstructorInfo constructorInfo, params IEvaluation[] constructorArguments)
    {
      ArgumentUtility.CheckNotNull ("constructorInfo", constructorInfo);
      ArgumentUtility.CheckNotNull ("constructorArguments", constructorArguments);

      ConstructorInfo = constructorInfo;
      ConstructorArguments = constructorArguments;
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
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
