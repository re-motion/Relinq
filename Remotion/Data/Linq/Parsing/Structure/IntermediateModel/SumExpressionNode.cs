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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="MethodCallExpression"/> for the different overloads of <see cref="O:Queryable.Sum"/>.
  /// </summary>
  public class SumExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<decimal>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<decimal?>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<double>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<double?>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<int>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<int?>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<long>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<long?>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<float>) null)),
                                                               GetSupportedMethod (() => Queryable.Sum ((IQueryable<float?>) null)),
                                                               
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (decimal)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (decimal?)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (double)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (double?)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (int)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (int?)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (long)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (long?)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (float)0)),
                                                               GetSupportedMethod (() => Queryable.Sum<object> (null, o => (float?)0)),
                                                           };

    public SumExpressionNode (IExpressionNode source, LambdaExpression optionalSelector)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      
      Source = source;
      OptionalSelector = optionalSelector;
    }

    public IExpressionNode Source { get; private set; }
    public LambdaExpression OptionalSelector { get; private set; }

    public Expression GetResolvedSelector ()
    {
      if (OptionalSelector == null)
        throw GetResolvedSelectorException ();
      return Source.Resolve (OptionalSelector.Parameters[0], OptionalSelector.Body);
    }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      throw CreateResolveNotSupportedException ();
    }
  }
}