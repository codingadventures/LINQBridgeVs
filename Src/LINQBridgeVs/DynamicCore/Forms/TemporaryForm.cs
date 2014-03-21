using System.Drawing;
using System.Windows.Forms;

namespace LINQBridge.DynamicCore.Forms
{
    internal class TemporaryForm : Form
    {
        public TemporaryForm()
        {
            SuspendLayout();
            Name = "Waiting LINQBridge";
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            Name = "TemporaryForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            Location = Cursor.Position;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(1, 0, 0);
            WindowState = FormWindowState.Maximized;
            Opacity = 0.01;
            TopMost = true;
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            ResumeLayout(false);
        }

        protected override void OnClick(System.EventArgs e)
        {
            base.OnClick(e);
            Invalidate();
            Close();
        }

       

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Invalidate();
            Close();
        }


    }
}
