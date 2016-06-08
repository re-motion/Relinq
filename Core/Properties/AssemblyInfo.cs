// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Reflection;
using System.Security;

[assembly: AssemblyTitle ("re-linq Frontend")]
[assembly: AssemblyDescription (@"re-linq Frontend: A foundation for parsing LINQ expression trees and generating queries in SQL or other languages.
Key features:
- Transforms expression trees into abstract syntax trees resemblying LINQ query expressions (from ... select syntax)
- Simplifies many aspects of this tree (sub queries, transparent identifieres, pre-evaluation ...)
- Provides basic infrastructure for backend development (e.g. SQL generation) 
- Provides a framework for user-defined query extensions and transformations")]
[assembly: AssemblyCulture ("")]
[assembly: CLSCompliant (true)]
#if !NET_3_5

[assembly: SecurityTransparent] // required to allow assembly to be linked from assemblies having the AllowPartiallyTrustedCallersAttribute applied
#else
[assembly: AllowPartiallyTrustedCallers] // required to allow assembly to be linked from assemblies having the AllowPartiallyTrustedCallersAttribute applied
#endif