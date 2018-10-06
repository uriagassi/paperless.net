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

        public NotesContext NotesContext { get; internal set; }

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
                        } else
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
                showMore.Visible = (flowLayoutPanel1.Controls[i - 1].Bounds.Y != 0) ;
            } catch (Exception e)
            {
                Console.Out.WriteLine(e);
            } finally
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
            if (flowLayoutPanel1.Controls.Count > 0)
            {
                showMore.Visible = (flowLayoutPanel1.Controls[flowLayoutPanel1.Controls.Count - 1].Bounds.Y != 0);
            }
        }
    }
}
