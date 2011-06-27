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
using System;
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Caches a resolved expression in the <see cref="IExpressionNode"/> classes.
  /// </summary>
  public class ResolvedExpressionCache<T>
      where T : Expression
  {
    private readonly ExpressionResolver _resolver;
    private T _cachedExpression;

    public ResolvedExpressionCache (IExpressionNode currentNode)
    {
      ArgumentUtility.CheckNotNull ("currentNode", currentNode);

      _resolver = new ExpressionResolver (currentNode);
      _cachedExpression = null;
    }

    public T GetOrCreate (Func<ExpressionResolver, T> generator)
    {
      if (_cachedExpression == null)
        _cachedExpression = generator (_resolver);

      return _cachedExpression;
    }
  }
}
