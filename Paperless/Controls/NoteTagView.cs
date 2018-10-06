using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Paperless.Model;

namespace Paperless.Controls
{
    public partial class NoteTagView : UserControl
    {
        public NoteTagView()
        {
            InitializeComponent();
        }

        [Bindable(true)]
        public object DataSource
        {
            get
            {
                return noteTag.DataSource;
            }
            set
            {
                if (value != DataSource)
                {
                    noteTag.DataSource = value;
                }
            }
        }

        public NotesContext NotesContext { get; set; }

        private void deletePanel_Paint(object sender, PaintEventArgs e)
        {
            if (drawDelete)
            {
                using (var pen = new Pen(Color.Black, 2))
                {
                    e.Graphics.DrawLine(pen, 6, 6, 14, 14);
                    e.Graphics.DrawLine(pen, 6, 14, 14, 6);
                }
            }
        }

        public NoteTag NoteTag
        {
            get
            {
                return noteTag.Current as NoteTag;
            }
        }

        private void deletePanel_Click(object sender, EventArgs e)
        {
            NotesContext.NoteTags.Remove(NoteTag);
            NoteTag.Tag.NoteTags.Remove(NoteTag);
            NoteTag.Note.NoteTags.Remove(NoteTag);
            //noteTag.RemoveCurrent();
            OnTagDeleted();
        }

        private bool drawDelete = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            CheckMousePosition();
            drawDelete = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            CheckMousePosition();
            base.OnMouseLeave(e);
        }

        protected void CheckMousePosition()
        {
            if (ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                drawDelete = true;
                BackColor = SystemColors.GradientActiveCaption;
            } else
            {
                drawDelete = false;
                BackColor = SystemColors.GradientInactiveCaption;
            }
        }

        public event EventHandler<EventArgs> TagDeleted;

        protected virtual void OnTagDeleted()
        {
            var handler = TagDeleted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void tagName_MouseEnter(object sender, EventArgs e)
        {
            CheckMousePosition();
        }

        private void tagName_DragEnter(object sender, DragEventArgs e)
        {
            int i = 0;
        }
    }
}
