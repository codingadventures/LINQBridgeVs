#region License
// Copyright (c) 2013 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

namespace BridgeVs.DynamicVisualizers.Forms
{
    internal class TemporaryForm : Form
    {
        private readonly Exception _exception;

        public TemporaryForm(Exception exception = null)
        {

            SuspendLayout();
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
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            ResumeLayout(false);
            Shown += Form1_Shown;
            _exception = exception;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (_exception != null)
            {
                MessageBox.Show(_exception.Message, "Error in LINQBridgeVs", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        protected override void OnClick(EventArgs e)
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
