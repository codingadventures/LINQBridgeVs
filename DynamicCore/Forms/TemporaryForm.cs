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
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(1, 0, 0);
            WindowState = FormWindowState.Maximized;
            Opacity = 0.01;
            ResumeLayout(false);
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Invalidate();
            Close();
        }


    }
}
