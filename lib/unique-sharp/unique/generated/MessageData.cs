// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Unique {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class MessageData : GLib.Opaque, ICloneable {

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_get_filename(IntPtr raw);

		[DllImport("libunique-1.0-0.dll")]
		static extern void unique_message_data_set_filename(IntPtr raw, IntPtr filename);

		public string Filename { 
			get {
				IntPtr raw_ret = unique_message_data_get_filename(Handle);
				string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
				return ret;
			}
			set {
				IntPtr native_value = GLib.Marshaller.StringToPtrGStrdup (value);
				unique_message_data_set_filename(Handle, native_value);
				GLib.Marshaller.Free (native_value);
			}
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_get_startup_id(IntPtr raw);

		public string StartupId { 
			get {
				IntPtr raw_ret = unique_message_data_get_startup_id(Handle);
				string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
				return ret;
			}
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern uint unique_message_data_get_workspace(IntPtr raw);

		public uint Workspace { 
			get {
				uint raw_ret = unique_message_data_get_workspace(Handle);
				uint ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_get_uris(IntPtr raw);

		[DllImport("libunique-1.0-0.dll")]
		static extern void unique_message_data_set_uris(IntPtr raw, IntPtr[] uris);

		public string[] Uris { 
			get {
				IntPtr raw_ret = unique_message_data_get_uris(Handle);
				string[] ret = GLib.Marshaller.NullTermPtrToStringArray (raw_ret, false);
				return ret;
			}
			set {
				int cnt_value = value == null ? 0 : value.Length;
				IntPtr[] native_value = new IntPtr [cnt_value + 1];
				for (int i = 0; i < cnt_value; i++)
					native_value [i] = GLib.Marshaller.StringToPtrGStrdup(value[i]);
				native_value [cnt_value] = IntPtr.Zero;
				unique_message_data_set_uris(Handle, native_value);
			}
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_get_type();

		public static GLib.GType GType { 
			get {
				IntPtr raw_ret = unique_message_data_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_copy(IntPtr raw);

		private Unique.MessageData Copy() {
			IntPtr raw_ret = unique_message_data_copy(Handle);
			Unique.MessageData ret = raw_ret == IntPtr.Zero ? null : (Unique.MessageData) GLib.Opaque.GetOpaque (raw_ret, typeof (Unique.MessageData), true);
			return ret;
		}

		public MessageData(IntPtr raw) : base(raw) {}

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_new();

		public MessageData () 
		{
			Raw = unique_message_data_new();
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern void unique_message_data_free(IntPtr raw);

		protected override void Free (IntPtr raw)
		{
			unique_message_data_free (raw);
		}

		class FinalizerInfo {
			IntPtr handle;

			public FinalizerInfo (IntPtr handle)
			{
				this.handle = handle;
			}

			public bool Handler ()
			{
				unique_message_data_free (handle);
				return false;
			}
		}

		~MessageData ()
		{
			if (!Owned)
				return;
			FinalizerInfo info = new FinalizerInfo (Handle);
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (info.Handler));
		}

#endregion
#region Customized extensions
#line 1 "MessageData.custom"
//
// MessageData.custom
//
// Author(s):
//	Stephane Delcroix  <stephane@delcroix.org>
//
// Copyright (c) 2009 Stephane Delcroix
//
// This is open source software.
//

		public object Clone ()
		{
			return (object)Copy ();
		}
		
		[DllImport("libunique-1.0-0.dll")]
		static extern bool unique_message_data_set_text(IntPtr raw, IntPtr str, IntPtr length);

		[DllImport("libunique-1.0-0.dll")]
		static extern IntPtr unique_message_data_get_text(IntPtr raw);

		public string Text {
			set {
				IntPtr native_str = GLib.Marshaller.StringToPtrGStrdup (value);
				bool raw_ret = unique_message_data_set_text(Handle, native_str, new IntPtr ((long) System.Text.Encoding.UTF8.GetByteCount (value)));
				bool ret = raw_ret;
				GLib.Marshaller.Free (native_str);
				if (!ret)
					throw new Exception ("Failed to convert the text to UTF-8");
			}
			get {
				IntPtr raw_ret = unique_message_data_get_text(Handle);
				string ret = GLib.Marshaller.PtrToStringGFree(raw_ret);
				return ret;
			}
		}

		[DllImport("libunique-1.0-0.dll")]
		static extern byte[] unique_message_data_get(IntPtr raw, out UIntPtr length);

		[DllImport("libunique-1.0-0.dll")]
		static extern void unique_message_data_set(IntPtr raw, byte[] data, UIntPtr n_data);

		public byte[] Data { 
			set { unique_message_data_set(Handle, value, new UIntPtr ((ulong) (value == null ? -1 : value.Length))); }
			get {
				UIntPtr native_length;
				return unique_message_data_get(Handle, out native_length);
			}
		}




#endregion
	}
}