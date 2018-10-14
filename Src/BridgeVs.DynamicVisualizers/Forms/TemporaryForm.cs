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
        private Label lblException;
        private GroupBox groupBox1;

        public TemporaryForm(Exception exception = null)
        {

            InitializeComponent();
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            Location = Cursor.Position;
            StartPosition = FormStartPosition.CenterParent;
            if (exception == null)
            {
                BackColor = Color.FromArgb(1, 0, 0);
                Opacity = 0.01;
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
                ResumeLayout(false);
            }
            else
            {
                Name = "LINQBridgeVs - Error";
                lblException.Text = exception.StackTrace;
            }

            Width = Screen.PrimaryScreen.WorkingArea.Width;
            Height = Screen.PrimaryScreen.WorkingArea.Height;
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

        private void InitializeComponent()
        {
            this.lblException = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblException
            // 
            this.lblException.AutoSize = true;
            this.lblException.Location = new System.Drawing.Point(6, 58);
            this.lblException.Name = "lblException";
            this.lblException.Size = new System.Drawing.Size(0, 25);
            this.lblException.TabIndex = 0;
            lblException.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblException);
            this.groupBox1.Location = new System.Drawing.Point(12, 67);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(2165, 454);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Error";
            // 
            // TemporaryForm
            // 
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(2189, 915);
            this.Controls.Add(this.groupBox1);
            this.Name = "TemporaryForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
