namespace LINQBridge.VSExtension.Forms
{
    partial class ProjectDependencies
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProjectsDataGridView = new System.Windows.Forms.DataGridView();
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ProjectsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // ProjectsDataGridView
            // 
            this.ProjectsDataGridView.AllowUserToAddRows = false;
            this.ProjectsDataGridView.AllowUserToDeleteRows = false;
            this.ProjectsDataGridView.AllowUserToResizeColumns = false;
            this.ProjectsDataGridView.AllowUserToResizeRows = false;
            this.ProjectsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ProjectsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.ProjectsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ProjectsDataGridView.CausesValidation = false;
            this.ProjectsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ProjectsDataGridView.Location = new System.Drawing.Point(12, 57);
            this.ProjectsDataGridView.MultiSelect = false;
            this.ProjectsDataGridView.Name = "ProjectsDataGridView";
            this.ProjectsDataGridView.ReadOnly = true;
            this.ProjectsDataGridView.Size = new System.Drawing.Size(715, 321);
            this.ProjectsDataGridView.TabIndex = 0;
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(286, 396);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 1;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(380, 396);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(417, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Following project dependencies have been found...LINQBridge them? (Recommended)";
            // 
            // ProjectDependencies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(739, 428);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.ProjectsDataGridView);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProjectDependencies";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Project Dependencies Found...";
            ((System.ComponentModel.ISupportInitialize)(this.ProjectsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView ProjectsDataGridView;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label label1;
    }
}