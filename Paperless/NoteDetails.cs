using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Paperless.Model;

namespace Paperless
{
    [DefaultBindingProperty("Note")]
    public partial class NoteDetails : UserControl
    {
        private NotesContext _context;

        public NoteDetails()
        {
            InitializeComponent();
        }

        public Note Note
        {
            get
            {
                return noteBindingSource.Current as Note;
            }
            set
            {
                DataSource = value;
            }
        }

        [Bindable(true)]
        public object DataSource
        {
            get
            {
                return noteBindingSource.DataSource;
            }
            set
            {
                if (noteBindingSource.DataSource != value && value != null)
                {
                    if (Note != null) Context.SaveChangesAsync();
                    //try
                    //{
                        noteBindingSource.DataSource = value;
                        //noteTagsBindingSource.DataMember = "NoteTags";
                       
                        OnNoteChanged(EventArgs.Empty);
                    //} catch (Exception)
                    //{
                        //something went wrong - null data
                    //}
                }
            }
        }

        public NotesContext Context
        {
            get => _context; set
            {
                _context = value;
                if (Context != null)
                    notebookBindingSource.DataSource = _context.Notebooks.Local.ToBindingList();
                noteTagsList1.NotesContext = value;
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            WireControls(e.Control);
            base.OnControlAdded(e);
        }

        public void WireControls(Control ctrl)
        {
            try
            {
                ctrl.AllowDrop = true;
                ctrl.DragEnter += Control_DragEnter;
                ctrl.DragDrop += Control_DragDrop;
                foreach (Control child in ctrl.Controls)
                {
                    WireControls(child);
                }
                ctrl.ControlAdded += Ctrl_ControlAdded;
                ctrl.ControlRemoved += Ctrl_ControlRemoved;
            }
            catch (NotSupportedException) { }
        }

        private void Ctrl_ControlAdded(object sender, ControlEventArgs e)
        {
            WireControls(e.Control);
        }

        private void Ctrl_ControlRemoved(object sender, ControlEventArgs e)
        {
            UnwireControls(e.Control);
        }

        public void UnwireControls(Control ctrl)
        {
            ctrl.DragEnter -= Control_DragEnter;
            ctrl.DragDrop -= Control_DragDrop;
            foreach (Control child in ctrl.Controls)
            {
                UnwireControls(child);
            }
            ctrl.ControlAdded -= Ctrl_ControlAdded;
            ctrl.ControlRemoved -= Ctrl_ControlRemoved;
        }

        private void Control_DragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }

        private void Control_DragEnter(object sender, DragEventArgs e)
        {
            OnDragEnter(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            noteContents.ObjectForScripting = new ScriptInterface();
        }

        // create an event for the value change
        // this is extra classy, as you can edit the event right
        // from the property window for the control in visual studio
        [Category("Data")]
        [Description("Fires when the note is changed")]
        public event EventHandler NoteChanged;

        protected virtual void OnNoteChanged(EventArgs e)
        {
            // Raise the event
            NoteChanged?.Invoke(this, e);
        }

        [System.Runtime.InteropServices.ComVisibleAttribute(true)]
        public class ScriptInterface
        {
            public void debugPrint(object obj)
            {
                MessageBox.Show("" + obj);
            }
            public void openAttachment(string att)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = Properties.Settings.Default.ProjectLocation + '\\' + att,
                    //Verb = "openas",
                    UseShellExecute = true,
                    ErrorDialog = true
                };

                Process.Start(startInfo);

            }
        }

        private void loadNoteContents()
        {
            noteContents.Stop();
            noteContents.ScriptErrorsSuppressed = true;
            if (String.IsNullOrEmpty(noteContents.DocumentText) || noteContents.StatusText == "Done")
            {
                noteContents.DocumentText = "0";
            }
                noteContents.Document.OpenNew(false);
                if (noteBindingSource.Current != null)
                {
                    var text = Note.NoteData;
                    text = "<html><head>" +

                       "<base href='" + new Uri(Properties.Settings.Default.ProjectLocation + "\\").AbsoluteUri + "'/>" +
                       "<link rel='stylesheet' type='text/css' href='css/paperless.css'/>" +
                       "<meta http-equiv='X-UA-Compatible' content='IE=11'>" +
                       "<script src='js/paperless.js'></script>" +
                       "</head><body>"
                        + text + "</body></html>";
                    noteContents.Document.Write(text);

                }
                noteContents.Refresh();
            noteContents.ScriptErrorsSuppressed = false;
        }

        private void noteBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            loadNoteContents();
        }

       /* private void tags_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var tag = (Tag)tags.Items[e.Index];
            var textSize = TextRenderer.MeasureText(e.Graphics, tag.Name, Font);
            var rect = new Rectangle(e.Bounds.Location, new Size(textSize.Width + 15, e.Bounds.Height));
            e.Graphics.FillRectangle(Brushes.LightBlue, rect);
            TextRenderer.DrawText(e.Graphics, tag.Name, Font, rect, ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }*/
    }

}
