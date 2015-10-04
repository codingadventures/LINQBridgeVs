#region License
// Copyright (c) 2013 Giovanni Campo
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LINQBridgeVs.Helper.Dependency;
using LINQBridgeVs.Helper.Extension;

namespace LINQBridgeVs.Helper.Forms
{
    internal partial class ProjectDependencies : Form
    {
        #region [ Fields ]
        private Action<string, string, string> _onOk;
        private readonly IEnumerable<Project> _foundProjects;
        #endregion

        private IEnumerable<Project> SelectedProjects
        {
            get
            {
                return from DataGridViewRow row in ProjectsDataGridView.Rows
                       where Convert.ToBoolean(row.Cells[0].Value)
                       select new Project
                       {
                           AssemblyName = row.Cells[2].Value.ToString(),
                           AssemblyPath = row.Cells[3].Value.ToString(),
                           DependencyType = DependencyType.ProjectReference
                       };
            }
        }

        #region [ Constructor ]
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDependencies"/> class.
        /// </summary>
        /// <param name="foundProjects">The dependant found projects.</param>
        /// <param name="message">The message to show </param>
        internal ProjectDependencies(IEnumerable<Project> foundProjects, string message)
        {
            _foundProjects = foundProjects;
            InitializeComponent();
            ProjectsDataGridView.CellClick += ProjectsDataGridView_CellClick;
            ProjectsDataGridView.DataBindingComplete += ProjectsDataGridView_DataBindingComplete;
            MessageLabel.Text = message;
        }
        #endregion

        #region [ ProjectsDataGridView Events ]

        void ProjectsDataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ProjectsDataGridView.Columns[0].Width = 20;

            foreach (DataGridViewRow row in ProjectsDataGridView.Rows)
            {
                row.Cells[0].Value = true;

            }
        }

        void ProjectsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var ch1 = new DataGridViewCheckBoxCell();
            if (e.ColumnIndex != 0) return;

            if (ProjectsDataGridView.CurrentRow != null)
                ch1 = (DataGridViewCheckBoxCell)ProjectsDataGridView.Rows[ProjectsDataGridView.CurrentRow.Index].Cells[0];

            if (ch1.Value == null)
                ch1.Value = false;
            switch (ch1.Value.ToString())
            {
                case "True":
                    ch1.Value = false;
                    break;
                case "False":
                    ch1.Value = true;
                    break;
            }
        }
        #endregion


        internal IEnumerable<Project> ShowDependencies(Action<string, string, string> onOk)
        {
            _onOk = onOk;
            ProjectsDataGridView.DataSource = _foundProjects.ToList();
            var res = ShowDialog();
            return res == DialogResult.OK ? SelectedProjects : Enumerable.Empty<Project>();
        }

        #region [ Button Events ]

        private void OkButton_Click(object sender, EventArgs e)
        {

            SelectedProjects.ForEach(p
                => _onOk(
                    Path.GetDirectoryName(p.AssemblyPath)
                    , p.AssemblyName
                    , p.SolutionName));

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }

        #endregion
    }
}
