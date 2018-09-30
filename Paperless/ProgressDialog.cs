using Paperless.Import;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paperless
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        private bool cancelled = false;

        public bool Start(Func<Progress<ImportProgress>, CancelEventArgs, bool> action, IWin32Window owner)
        {
            var progress = new Progress<ImportProgress>();
            var cancelToken = new CancelEventArgs();

            progress.ProgressChanged += (e, p) =>
            {
                if (cancelled)
                {
                    cancelled = false;
                    cancelToken.Cancel = true;
                }
                if (p.Title != null) { Text = p.Title; }
                if (p.MaxValue > 0)
                {
                    progressBar1.Maximum = p.MaxValue;
                    progressBar1.Style = ProgressBarStyle.Blocks;
                }
                if (p.Progress > 0) { progressBar1.Value = p.Progress;  }
                if (p.Text != null)
                {
                    progressLabel.Text = p.Text;
                }
            };
            Shown += async (s,e)=> {
                await Task.Run(() => action(progress, cancelToken));
                Close();
                };
            return ShowDialog(owner) == DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            cancelled = true;
        }
    }

}
