/*
 * ResizeEditor.cs
 *
 * Author(s)
 * 	Stephane Delcroix  (stephane@delcroix.org)
 *
 * This is free software. See COPYING for details.
 */

using System;
using FSpot;
using FSpot.Editors;
using Gtk;
using Gdk;
using Mono.Unix;

namespace FSpot.Addins.Editors {
	class ResizeEditor : Editor {
		double ratio;
		SpinButton size;

		public ResizeEditor () : base (Catalog.GetString ("Resize"), null) {
			CanHandleMultiple = false;
			HasSettings = true;
		}

		protected override Pixbuf Process (Pixbuf input, Cms.Profile input_profile)
		{
			Pixbuf output = (Pixbuf) input.Clone ();
			double ratio = (double)size.Value / Math.Max (output.Width, output.Height);
			return output.ScaleSimple ((int)(output.Width * ratio), (int)(output.Height * ratio), InterpType.Bilinear);
		}

		public override Widget ConfigurationWidget ()
		{
//			int max = Math.Max (input.Width, input.Height);
			int max = 2000;
			size = new SpinButton (128, max, 10);
			size.Value = max;
			return size;
		}
	}
}