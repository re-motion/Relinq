// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Contains metadata about a <see cref="MethodCallExpression"/> that is parsed into a <see cref="MethodCallExpressionNodeBase"/>.
  /// </summary>
  public struct MethodCallExpressionParseInfo
  {
    public MethodCallExpressionParseInfo (string associatedIdentifier, IExpressionNode source, MethodCallExpression parsedExpression)
        : this()
    {
      ArgumentUtility.CheckNotNullOrEmpty ("associatedIdentifier", associatedIdentifier);
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("parsedExpression", parsedExpression);

      AssociatedIdentifier = associatedIdentifier;
      Source = source;
      ParsedExpression = parsedExpression;
    }

    /// <summary>
    /// Gets the associated identifier, i.e. the name the user gave the data streaming out of this expression. For example, the 
    /// <see cref="SelectManyExpressionNode"/> corresponding to a <c>from c in C</c> clause should get the identifier "c".
    /// If there is no user-defined identifier (or the identifier is impossible to infer from the expression tree), a generated identifier
    /// is given instead.
    /// </summary>
    public string AssociatedIdentifier { get; private set; }

    /// <summary>
    /// Gets the source expression node, i.e. the node streaming data into the parsed node.
    /// </summary>
    /// <value>The source.</value>
    public IExpressionNode Source { get; private set; }

    /// <summary>
    /// Gets the <see cref="MethodCallExpression"/> being parsed.
    /// </summary>
    public MethodCallExpression ParsedExpression { get; private set; }
  }
}
