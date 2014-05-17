/*
    Copyright (C) 2012-2014 de4dot@gmail.com

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

﻿using System;
using System.Diagnostics;
using System.Threading;
using dnlib.Utils;
using dnlib.DotNet.MD;

namespace dnlib.DotNet {
	/// <summary>
	/// A high-level representation of a row in the ImplMap table
	/// </summary>
	[DebuggerDisplay("{Module} {Name}")]
	public abstract class ImplMap : IMDTokenProvider {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

		/// <inheritdoc/>
		public MDToken MDToken {
			get { return new MDToken(Table.ImplMap, rid); }
		}

		/// <inheritdoc/>
		public uint Rid {
			get { return rid; }
			set { rid = value; }
		}

		/// <summary>
		/// From column ImplMap.MappingFlags
		/// </summary>
		public PInvokeAttributes Attributes {
			get { return (PInvokeAttributes)attributes; }
			set { attributes = (int)value; }
		}
		/// <summary>Attributes</summary>
		protected int attributes;

		/// <summary>
		/// From column ImplMap.ImportName
		/// </summary>
		public UTF8String Name {
			get { return name; }
			set { name = value; }
		}
		/// <summary>Name</summary>
		protected UTF8String name;

		/// <summary>
		/// From column ImplMap.ImportScope
		/// </summary>
		public ModuleRef Module {
			get { return module; }
			set { module = value; }
		}
		/// <summary/>
		protected ModuleRef module;

		/// <summary>
		/// Modify <see cref="attributes"/> property: <see cref="attributes"/> =
		/// (<see cref="attributes"/> &amp; <paramref name="andMask"/>) | <paramref name="orMask"/>.
		/// </summary>
		/// <param name="andMask">Value to <c>AND</c></param>
		/// <param name="orMask">Value to OR</param>
		void ModifyAttributes(PInvokeAttributes andMask, PInvokeAttributes orMask) {
#if THREAD_SAFE
			int origVal, newVal;
			do {
				origVal = attributes;
				newVal = (origVal & (int)andMask) | (int)orMask;
			} while (Interlocked.CompareExchange(ref attributes, newVal, origVal) != origVal);
#else
			attributes = (attributes & (int)andMask) | (int)orMask;
#endif
		}

		/// <summary>
		/// Set or clear flags in <see cref="attributes"/>
		/// </summary>
		/// <param name="set"><c>true</c> if flags should be set, <c>false</c> if flags should
		/// be cleared</param>
		/// <param name="flags">Flags to set or clear</param>
		void ModifyAttributes(bool set, PInvokeAttributes flags) {
#if THREAD_SAFE
			int origVal, newVal;
			do {
				origVal = attributes;
				if (set)
					newVal = origVal | (int)flags;
				else
					newVal = origVal & ~(int)flags;
			} while (Interlocked.CompareExchange(ref attributes, newVal, origVal) != origVal);
#else
			if (set)
				attributes |= (int)flags;
			else
				attributes &= ~(int)flags;
#endif
		}

		/// <summary>
		/// Gets/sets the <see cref="PInvokeAttributes.NoMangle"/> bit
		/// </summary>
		public bool IsNoMangle {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.NoMangle) != 0; }
			set { ModifyAttributes(value, PInvokeAttributes.NoMangle); }
		}

		/// <summary>
		/// Gets/sets the char set
		/// </summary>
		public PInvokeAttributes CharSet {
			get { return (PInvokeAttributes)attributes & PInvokeAttributes.CharSetMask; }
			set { ModifyAttributes(~PInvokeAttributes.CharSetMask, value & PInvokeAttributes.CharSetMask); }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CharSetNotSpec"/> is set
		/// </summary>
		public bool IsCharSetNotSpec {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CharSetMask) == PInvokeAttributes.CharSetNotSpec; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CharSetAnsi"/> is set
		/// </summary>
		public bool IsCharSetAnsi {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CharSetMask) == PInvokeAttributes.CharSetAnsi; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CharSetUnicode"/> is set
		/// </summary>
		public bool IsCharSetUnicode {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CharSetMask) == PInvokeAttributes.CharSetUnicode; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CharSetAuto"/> is set
		/// </summary>
		public bool IsCharSetAuto {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CharSetMask) == PInvokeAttributes.CharSetAuto; }
		}

		/// <summary>
		/// Gets/sets best fit
		/// </summary>
		public PInvokeAttributes BestFit {
			get { return (PInvokeAttributes)attributes & PInvokeAttributes.BestFitMask; }
			set { ModifyAttributes(~PInvokeAttributes.BestFitMask, value & PInvokeAttributes.BestFitMask); }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.BestFitUseAssem"/> is set
		/// </summary>
		public bool IsBestFitUseAssem {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.BestFitMask) == PInvokeAttributes.BestFitUseAssem; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.BestFitEnabled"/> is set
		/// </summary>
		public bool IsBestFitEnabled {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.BestFitMask) == PInvokeAttributes.BestFitEnabled; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.BestFitDisabled"/> is set
		/// </summary>
		public bool IsBestFitDisabled {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.BestFitMask) == PInvokeAttributes.BestFitDisabled; }
		}

		/// <summary>
		/// Gets/sets throw on unmappable char
		/// </summary>
		public PInvokeAttributes ThrowOnUnmappableChar {
			get { return (PInvokeAttributes)attributes & PInvokeAttributes.ThrowOnUnmappableCharMask; }
			set { ModifyAttributes(~PInvokeAttributes.ThrowOnUnmappableCharMask, value & PInvokeAttributes.ThrowOnUnmappableCharMask); }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.ThrowOnUnmappableCharUseAssem"/> is set
		/// </summary>
		public bool IsThrowOnUnmappableCharUseAssem {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.ThrowOnUnmappableCharMask) == PInvokeAttributes.ThrowOnUnmappableCharUseAssem; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.ThrowOnUnmappableCharEnabled"/> is set
		/// </summary>
		public bool IsThrowOnUnmappableCharEnabled {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.ThrowOnUnmappableCharMask) == PInvokeAttributes.ThrowOnUnmappableCharEnabled; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.ThrowOnUnmappableCharDisabled"/> is set
		/// </summary>
		public bool IsThrowOnUnmappableCharDisabled {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.ThrowOnUnmappableCharMask) == PInvokeAttributes.ThrowOnUnmappableCharDisabled; }
		}

		/// <summary>
		/// Gets/sets the <see cref="PInvokeAttributes.SupportsLastError"/> bit
		/// </summary>
		public bool SupportsLastError {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.SupportsLastError) != 0; }
			set { ModifyAttributes(value, PInvokeAttributes.SupportsLastError); }
		}

		/// <summary>
		/// Gets/sets calling convention
		/// </summary>
		public PInvokeAttributes CallConv {
			get { return (PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask; }
			set { ModifyAttributes(~PInvokeAttributes.CallConvMask, value & PInvokeAttributes.CallConvMask); }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CallConvWinapi"/> is set
		/// </summary>
		public bool IsCallConvWinapi {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask) == PInvokeAttributes.CallConvWinapi; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CallConvCdecl"/> is set
		/// </summary>
		public bool IsCallConvCdecl {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask) == PInvokeAttributes.CallConvCdecl; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CallConvStdcall"/> is set
		/// </summary>
		public bool IsCallConvStdcall {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask) == PInvokeAttributes.CallConvStdcall; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CallConvThiscall"/> is set
		/// </summary>
		public bool IsCallConvThiscall {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask) == PInvokeAttributes.CallConvThiscall; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="PInvokeAttributes.CallConvFastcall"/> is set
		/// </summary>
		public bool IsCallConvFastcall {
			get { return ((PInvokeAttributes)attributes & PInvokeAttributes.CallConvMask) == PInvokeAttributes.CallConvFastcall; }
		}

		/// <summary>
		/// Checks whether this <see cref="ImplMap"/> is a certain P/Invoke method
		/// </summary>
		/// <param name="dllName">Name of the DLL</param>
		/// <param name="funcName">Name of the function within the DLL</param>
		/// <returns><c>true</c> if it's the specified P/Invoke method, else <c>false</c></returns>
		public bool IsPinvokeMethod(string dllName, string funcName) {
			if (name != funcName)
				return false;
			var mod = module;
			if (mod == null)
				return false;
			return GetDllName(dllName).Equals(GetDllName(mod.Name), StringComparison.OrdinalIgnoreCase);
		}

		static string GetDllName(string dllName) {
			if (dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				return dllName.Substring(0, dllName.Length - 4);
			return dllName;
		}
	}

	/// <summary>
	/// An ImplMap row created by the user and not present in the original .NET file
	/// </summary>
	public class ImplMapUser : ImplMap {
		/// <summary>
		/// Default constructor
		/// </summary>
		public ImplMapUser() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="scope">Scope</param>
		/// <param name="name">Name</param>
		/// <param name="flags">Flags</param>
		public ImplMapUser(ModuleRef scope, UTF8String name, PInvokeAttributes flags) {
			this.module = scope;
			this.name = name;
			this.attributes = (int)flags;
		}
	}

	/// <summary>
	/// Created from a row in the ImplMap table
	/// </summary>
	sealed class ImplMapMD : ImplMap, IMDTokenProviderMD {
		/// <summary>The module where this instance is located</summary>
		readonly ModuleDefMD readerModule;

		readonly uint origRid;

		/// <inheritdoc/>
		public uint OrigRid {
			get { return origRid; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>ImplMap</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is invalid</exception>
		public ImplMapMD(ModuleDefMD readerModule, uint rid) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (readerModule.TablesStream.ImplMapTable.IsInvalidRID(rid))
				throw new BadImageFormatException(string.Format("ImplMap rid {0} does not exist", rid));
#endif
			this.origRid = rid;
			this.rid = rid;
			this.readerModule = readerModule;
			var rawRow = readerModule.TablesStream.ReadImplMapRow(origRid);
			attributes = (int)rawRow.MappingFlags;
			name = readerModule.StringsStream.ReadNoNull(rawRow.ImportName);
			module = readerModule.ResolveModuleRef(rawRow.ImportScope);
		}
	}
}
