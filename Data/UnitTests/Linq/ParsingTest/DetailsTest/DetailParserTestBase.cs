/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest
{
  public class DetailParserTestBase
  {
    protected QueryModel QueryModel;
    protected ParseContext ParseContext;

    [SetUp]
    public virtual void SetUp ()
    {
      QueryModel = ExpressionHelper.CreateQueryModel ();
      ParseContext = new ParseContext(QueryModel, QueryModel.GetExpressionTree(), new List<FieldDescriptor>(), new JoinedTableContext());
    }
  }
}
