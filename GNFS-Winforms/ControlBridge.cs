using System;
using System.Windows.Forms;

namespace GNFS_Winforms
{
	public class ControlBridge
	{
		private Control control;

		public ControlBridge(Control ctrl)
		{
			control = ctrl;
		}

		public void SetControlEnabledState(bool enabled)
		{
			SetControlEnabledState(control, enabled);
		}

		public void SetControlText(string text)
		{
			SetControlText(control, text);
		}

		public static void SetControlEnabledState(Control control, bool enabled)
		{
			if (!control.IsHandleCreated || control.IsDisposed)
			{
				throw new Exception();
			}

			if (control.InvokeRequired /* && !GNFSCore.DirectoryLocations.IsLinuxOS()*/)
			{
				control.Invoke(new Action(() => { SetControlEnabledState(control, enabled); }));
			}
			else
			{
				control.Enabled = enabled;
			}
		}

		public static void SetControlVisibleState(Control control, bool visible)
		{
			if (!control.IsHandleCreated || control.IsDisposed)
			{
				throw new Exception();
			}

			if (control.InvokeRequired /* && !GNFSCore.DirectoryLocations.IsLinuxOS()*/)
			{
				control.Invoke(new Action(() => { SetControlVisibleState(control, visible); }));
			}
			else
			{
				control.Visible = visible;
			}
		}

		public static void SetControlText(Control control, string text)
		{
			if (!control.IsHandleCreated || control.IsDisposed)
			{
				throw new Exception();
			}

			if (control.InvokeRequired /* && !GNFSCore.DirectoryLocations.IsLinuxOS()*/)
			{
				control.Invoke(new Action(() => { SetControlText(control, text); }));
			}
			else
			{
				control.Text = text;
			}
		}
	}
}
