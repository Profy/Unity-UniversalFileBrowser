#if PROPERTIES

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShellFileDialogs
{
	/// <summary>
	/// A property store
	/// </summary>
	[ComImport]
	[Guid(ShellIIDGuid.IPropertyStore)]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IPropertyStore
	{
		/// <summary>
		/// Gets the number of properties contained in the property store.
		/// </summary>
		/// <param name="propertyCount"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetCount([Out] out uint propertyCount);

		/// <summary>
		/// Get a property key located at a specific index.
		/// </summary>
		/// <param name="propertyIndex"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetAt([In] uint propertyIndex, out PropertyKey key);

		/// <summary>
		/// Gets the value of a property from the store
		/// </summary>
		/// <param name="key"></param>
		/// <param name="pv"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetValue([In] ref PropertyKey key, [Out] PropVariant pv);

		/// <summary>
		/// Sets the value of a property in the store
		/// </summary>
		/// <param name="key"></param>
		/// <param name="pv"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
		HResult SetValue([In] ref PropertyKey key, [In] PropVariant pv);

		/// <summary>
		/// Commits the changes.
		/// </summary>
		/// <returns></returns>
		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult Commit();
	}
}
#endif
