using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LINQBridge.VSExtension.Forms
{
    public partial class ProjectDependencies : Form
    {
        private readonly Action _onOk;

        public ProjectDependencies(Action onOk)
        {
            _onOk = onOk;
            InitializeComponent();
        }


        internal void ShowDependencies(IEnumerable<Dependency.Project> foundProjects)
        {
            ProjectsDataGridView.DataSource = foundProjects.ToList();
            ShowDialog();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            _onOk();
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
