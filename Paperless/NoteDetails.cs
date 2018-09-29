﻿using System;
using System.ComponentModel;
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
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
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

        private void loadNoteContents()
        {
            if (String.IsNullOrEmpty(noteContents.DocumentText) || noteContents.StatusText == "Done")
            {
                noteContents.DocumentText = "0";
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
            }
        }

        private void noteBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            loadNoteContents();
        }
    }

}
