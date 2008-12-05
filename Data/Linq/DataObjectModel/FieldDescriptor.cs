// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct FieldDescriptor
  {
    public FieldDescriptor (MemberInfo member, FieldSourcePath sourcePath, Column? column)
        : this()
    {
      ArgumentUtility.CheckNotNull ("sourcePath", sourcePath);

      if (member == null && column == null)
        throw new ArgumentNullException ("member && column", "Either member or column must have a value.");

      Member = member;
      Column = column;
      SourcePath = sourcePath;
    }

    public MemberInfo Member { get; private set; }
    public Column? Column { get; private set; }
    public FieldSourcePath SourcePath { get; private set; }


    public Column GetMandatoryColumn ()
    {
      if (Column != null)
        return Column.Value;
      else
      {
        string message = string.Format ("The member '{0}.{1}' does not identify a queryable column.",
            Member.DeclaringType.FullName, Member.Name);

        throw new FieldAccessResolveException (message);
      }
    }
    
    public override string ToString ()
    {
      return string.Format ("{0} => {1}", SourcePath, Column);
    }
  }
}
