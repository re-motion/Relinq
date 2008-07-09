/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
