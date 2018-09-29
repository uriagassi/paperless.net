using Paperless.Model;
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
    public partial class NoteDetailsForm : Form
    {
        public NoteDetailsForm()
        {
            InitializeComponent();
        }

        public NoteDetailsForm(object dataSource, NotesContext context)
        {
            InitializeComponent();
            noteBindingSource.DataSource = dataSource;
            noteDetails1.Context = context;
            noteDetails1.DataSource = dataSource;
        }
    }
}
