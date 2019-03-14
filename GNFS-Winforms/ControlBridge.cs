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
			if (!GNFSCore.DirectoryLocations.IsLinuxOS() && control.InvokeRequired)
			{
				control.Invoke(new MethodInvoker(() =>
					SetControlEnabledState(control, enabled)
				));
			}
			else
			{
				control.Enabled = enabled;
			}
		}

		public static void SetControlVisibleState(Control control, bool visible)
		{
			if (!GNFSCore.DirectoryLocations.IsLinuxOS() && control.InvokeRequired)
			{
				control.Invoke(new MethodInvoker(() =>
					SetControlVisibleState(control, visible)
				));
			}
			else
			{
				control.Visible = visible;
			}
		}

		public static void SetControlText(Control control, string text)
		{
			if (!GNFSCore.DirectoryLocations.IsLinuxOS() && control.InvokeRequired)
			{
				control.Invoke(new MethodInvoker(() =>
					SetControlText(control, text)
				));
			}
			else
			{
				control.Text = text;
			}
		}
	}
}
