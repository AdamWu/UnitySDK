using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using UnityEngine;

public partial class NativePlugin
{
	[DllImport("NativePluginCPP", CallingConvention = CallingConvention.StdCall)]
	private static extern float TestMultiply(float a, float b);

	[DllImport("NativePluginCPP", CallingConvention = CallingConvention.StdCall)]
	private static extern int Encrypt(byte[] data, int len, out IntPtr result);

	[DllImport("NativePluginCPP", CallingConvention = CallingConvention.StdCall)]
	private static extern int Decrypt(byte[] data, int len, out IntPtr result);

	[DllImport("NativePluginCPP", CallingConvention = CallingConvention.StdCall)]
	private static extern int FreeMem([In] ref IntPtr p);


	public static byte[] Encrypt(byte[] data) {
		IntPtr result;
		int len = Encrypt(data, data.Length, out result);
		byte[] encrypt_data = new byte[len];
		Marshal.Copy (result, encrypt_data, 0, len);
		int size = FreeMem (ref result);
		return encrypt_data;
	}

	public static byte[] Decrypt(byte[] data) {
		IntPtr result;
		int len = Decrypt(data, data.Length, out result);
		byte[] decrypt_data = new byte[len];
		Marshal.Copy (result, decrypt_data, 0, len);
		int size = FreeMem (ref result);
		return decrypt_data;
	}
}