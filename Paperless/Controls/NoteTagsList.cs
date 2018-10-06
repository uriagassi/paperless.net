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
    public partial class NoteTagsList : UserControl
    {
        public NoteTagsList()
        {
            InitializeComponent();
        }

        [Bindable(true)]
        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get
            {
                return noteTags.DataSource;
            }
            set
            {
                noteTags.DataSource = value;
            }
        }

        [Bindable(true)]
        public string DataMember
        {
            get
            {
                return noteTags.DataMember;
            }
            set
            {
                noteTags.DataMember = value;
            }
        }

        public NotesContext NotesContext
        {
            get => _notesContext;
            internal set
            {
                _notesContext = value;
                if (_notesContext != null)
                {
                    newTagCombo.AutoCompleteCustomSource.Clear();
                   // newTagCombo.Items.Clear();
                        newTagCombo.AutoCompleteCustomSource.AddRange((from t in _notesContext.Tags.Local
                                                                      select t.Name).ToArray());
//newTagCombo.Items.Add(tag.Name);
                    _notesContext.Tags.Local.CollectionChanged += AvailableTagsChanged;
                }
            }
        }

        private void AvailableTagsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var tag in e.NewItems)
                {
                    newTagCombo.AutoCompleteCustomSource.Add(((Tag)tag).Name);
  //                  newTagCombo.Items.Add(((Tag)tag).Name);
                }
            } else
            {
                newTagCombo.AutoCompleteCustomSource.Clear();
                //newTagCombo.Items.Clear();
                foreach (var tag in _notesContext.Tags.Local)
                {
                    newTagCombo.AutoCompleteCustomSource.Add(tag.Name);
                   // newTagCombo.Items.Add(tag.Name);
                }
            }
        }

        private void noteTags_DataSourceChanged(object sender, EventArgs e)
        {
            if (CheckChanges())
            {
                DrawList();
            }
        }

        private bool CheckChanges()
        {
            int i = 0;
            //var en = noteTags.GetEnumerator();
            //while (en.MoveNext())
            //{
            //  if ((flowLayoutPanel1.Controls[i] as NoteTagView)?.NoteTag != en.Current) return true;
            // i++;
            // }
            return true;
        }

        private Stack<NoteTagView> controlCache = new Stack<NoteTagView>();
        private NotesContext _notesContext;

        private void DrawList()
        {
            SuspendLayout();
            int i = 0;
            //flowLayoutPanel1.Controls.Clear();
            try
            {
                var en = noteTags.GetEnumerator();
                while (en.MoveNext())
                {
                    if (flowLayoutPanel1.Controls.Count > i)
                    {
                        ((NoteTagView)flowLayoutPanel1.Controls[i]).DataSource = en.Current;
                    }
                    else
                    {
                        NoteTagView item = null;
                        if (controlCache.Count == 0)
                        {
                            item = new NoteTagView { NotesContext = NotesContext };
                            item.TagDeleted += Item_TagDeleted;
                        }
                        else
                        {
                            item = controlCache.Pop();
                        }

                        item.DataSource = en.Current;
                        flowLayoutPanel1.Controls.Add(item);
                    }
                    i++;
                }
                while (flowLayoutPanel1.Controls.Count > i)
                {
                    controlCache.Push((NoteTagView)flowLayoutPanel1.Controls[i]);
                    flowLayoutPanel1.Controls.RemoveAt(i);
                }
                CheckShowMore();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
            finally
            {
                ResumeLayout();
            }
        }

        private void Item_TagDeleted(object sender, EventArgs e)
        {
            DrawList();
        }

        private void noteTags_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (CheckChanges())
            {
                DrawList();
            }
        }

        private void showMore_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillPolygon(Brushes.Black, new[] { new Point(6, 8), new Point(14, 8), new Point(10, 13) });
        }

        private void flowLayoutPanel1_ClientSizeChanged(object sender, EventArgs e)
        {
            CheckShowMore();
        }

        private void CheckShowMore()
        {
            if (flowLayoutPanel1.Controls.Count > 0)
            {
                newTagCombo.Visible = flowLayoutPanel1.Controls[flowLayoutPanel1.Controls.Count - 1].Bounds.Y == 0;
                showMore.Visible = (!newTagCombo.Visible || newTagCombo.Bounds.Y >= 10);
            }
        }

        private void newTagCombo_TextUpdate(object sender, EventArgs e)
        {
            try
            {
                if (newTagCombo.Bounds.Y < 10 && newTagCombo.Text != "Add tag..." && newTagCombo.Text != "")
                {
                    var en = noteTags.GetEnumerator();
                    while (en.MoveNext())
                    {
                        if ((en.Current as NoteTag)?.Tag?.Name == newTagCombo.Text) return;
                    }
                    AddTag?.Invoke(this, new TextEventArgs { Text = newTagCombo.Text });
                }
            }
            finally
            {
                newTagCombo.Text = "Add tag...";
                newTagCombo.SelectAll();
            }
        }
        public class TextEventArgs : EventArgs { public string Text { get; set; } }
        [Category("Data Change")]
        public event EventHandler<TextEventArgs> AddTag;

        private void newTagCombo_Enter(object sender, EventArgs e)
        {
            newTagCombo.Text = "";
            newTagCombo.BackColor = SystemColors.Window;
        }

        private void newTagCombo_Leave(object sender, EventArgs e)
        {
            // newTagCombo.Text = "Add tag...";
            newTagCombo.BackColor = SystemColors.Control;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Tab)
            {
                TextBoxBase box = this.ActiveControl as TextBoxBase;
                if (box == null || !box.Multiline)
                {
                    // Not a dialog, not a multi-line textbox; we can use Enter for tabbing
                    this.SelectNextControl(this.ActiveControl, true, true, true, true);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
