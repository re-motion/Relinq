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

#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace JetBrains.Annotations
{
  /// <summary>
  /// Describes dependency between method input and output
  /// </summary>
  /// <syntax>
  /// <p>Function definition table syntax:</p>
  /// <list>
  /// <item>FDT      ::= FDTRow [;FDTRow]*</item>
  /// <item>FDTRow   ::= Input =&gt; Output | Output &lt;= Input</item>
  /// <item>Input    ::= ParameterName: Value [, Input]*</item>
  /// <item>Output   ::= [ParameterName: Value]* {halt|stop|void|nothing|Value}</item>
  /// <item>Value    ::= true | false | null | notnull | canbenull</item>
  /// </list>
  /// If method has single input parameter, it's name could be omitted. <br/>
  /// Using "halt" (or "void"/"nothing", which is the same) for method output means that methos doesn't return normally. <br/>
  /// "canbenull" annotation is only applicable for output parameters. <br/>
  /// You can use multiple [ContractAnnotation] for each FDT row, or use single attribute with rows separated by semicolon. <br/>
  /// </syntax>
  /// <examples>
  /// <list>
  /// <item>[ContractAnnotation("=> halt")] public void TerminationMethod()</item>
  /// <item>[ContractAnnotation("halt &lt;= condition: false")] public void Assert(bool condition, string text) // Regular Assertion method</item>
  /// <item>[ContractAnnotation("s:null => true")] public bool IsNullOrEmpty(string s) // String.IsNullOrEmpty</item>
  /// <item>[ContractAnnotation("null => null; notnull => notnull")] public object Transform(object data) // Method which returns null if parameter is null, and not null if parameter is not null</item>
  /// <item>[ContractAnnotation("s:null=>false; =>true,result:notnull; =>false, result:null")] public bool TryParse(string s, out Person result)</item>
  /// </list>
  /// </examples>
  [AttributeUsage (AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
  sealed partial class ContractAnnotationAttribute : Attribute
  {
    public ContractAnnotationAttribute ([NotNull] string fdt)
        : this (fdt, false)
    {
    }

    public ContractAnnotationAttribute ([NotNull] string fdt, bool forceFullStates)
    {
      FDT = fdt;
      ForceFullStates = forceFullStates;
    }

    public string FDT { get; private set; }
    public bool ForceFullStates { get; private set; }
  }
}