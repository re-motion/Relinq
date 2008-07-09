/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class FieldAccessResolveException : Exception
  {
    public FieldAccessResolveException (string message)
        : base (message)
    {
    }

    public FieldAccessResolveException (string message, Exception inner)
        : base (message, inner)
    {
    }
  }
}
