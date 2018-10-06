using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Specialized;
using Paperless.Model;
using Microsoft.EntityFrameworkCore;
using Paperless.Utils;
using System.Collections;

namespace Paperless
{
    public partial class PaperlessMain : Form
    {
        public PaperlessMain()
        {
            WebBrowserHelper.FixBrowserVersion();
            InitializeComponent();
            LoadProject();
        }

        private void tags_DataSourceChanged(object sender, EventArgs e)
        {
            UpdateTagList();

        }

        private void UpdateTagList()
        {
            context.CreateIfNeeded();
            tagView.BeginUpdate();
            tagView.SuspendLayout();
            try
            {
                UpdateTag(tagView.Nodes["Tags"].Nodes, null);
            } finally
            {
                tagView.EndUpdate();
                tagView.ResumeLayout();
            }
        }

        private void CreateNotebookList()
        {
            tagView.Nodes["Notebooks"].Nodes.Clear();
            UpdateNotebookList();
            tagView.SelectedNode = tagView.Nodes["Notebooks"].Nodes[0];
        }

        private void UpdateNotebookList()
        {
            var nodes = tagView.Nodes["Notebooks"].Nodes;
            foreach (var notebook in context.Notebooks)
            {
                var count = (from note in context.Notes
                             where note.Notebook == notebook
                             select note).Count();
                var text = notebook.Name + " (" + count + ")";
                var node = nodes[notebook.NotebookId.ToString()] ?? nodes.Add(notebook.NotebookId.ToString(), text);

                    node.Text = text;
                node.Tag = notebook;
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;
            }
            if (nodes.Count > context.Notebooks.Count())
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    if (context.Notebooks.Find(nodes[i].Name) == null)
                    {
                        nodes.RemoveAt(i);
                    }
                }
            }
        }

        private void UpdateTag(TreeNodeCollection nodes, Model.Tag parentTag)
        {
            var children = parentTag == null ? (from tag1 in context.Tags
                            where tag1.ParentTag == null
                            orderby tag1.Name
                            select new { Tag = tag1, NoteCount = tag1.NoteTags.Count() }).ToList() : (from tag1 in context.Tags
                                                     where tag1.ParentTag == parentTag
                                                     orderby tag1.Name
                                                     select new { Tag = tag1, NoteCount = tag1.NoteTags.Count() }).ToList();
            if (children.Count() == 0)
            {
                nodes.Clear();
            }
            else
            {
                if (parentTag != null && !parentTag.IsExpanded)
                {
                    if (nodes.Count == 0)
                    {

                        nodes.Add("");
                    }
                }
                else
                {

                    for (int i = 0; i <children.Count(); i++)
                    {
                        var childNode = (from TreeNode cNode in nodes
                                         where cNode.Tag == children[i].Tag
                                         select cNode).FirstOrDefault();
                        if (childNode == null)
                        {
                            childNode = new TreeNode(children[i].Tag.Name) { Name = children[i].Tag.Name };
                                          
                        }
                        childNode.Tag = children[i].Tag;
                        if (children[i].NoteCount > 0)
                        {
                            childNode.Text = $"{childNode.Name} ({children[i].NoteCount})";
                        }

                        UpdateTag(childNode.Nodes, children[i].Tag);
                        if (nodes.Count < i+1 || nodes[i] != childNode)
                        {
                            nodes.Remove(childNode);
                            nodes.Insert(i, childNode);
                        }
                        if (children[i].Tag.IsExpanded)
                        {
                            childNode.Expand();
                        }

                    }
                    for (int i = nodes.Count - 1; i >= children.Count; i--)
                    {
                        nodes.RemoveAt(i);
                    }

                }


            }
        }
        protected override void OnClosed(EventArgs e)
        {
            Properties.Settings.Default.MainWindowState = WindowState;
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.MainWindowSize = Size;
            }
            Properties.Settings.Default.Save();
            base.OnClosed(e);
        }
        private void PaperlessMain_Load(object sender, EventArgs e)
        {
            //LoadProject();
            WindowState = Properties.Settings.Default.MainWindowState;
            if (WindowState != FormWindowState.Maximized)
            {
                Size = Properties.Settings.Default.MainWindowSize;
            }
        }

        private void LoadProject()
        {
            try { 
            context = new Model.NotesContext(Properties.Settings.Default.ProjectLocation);
                noteDetails1.Context = context;
            //context.CreateIfNeeded();
            tags.DataSource = context.Tags.Local.ToBindingList();
                tagView.Nodes["Tags"].Expand();
                context.Tags.Local.CollectionChanged += tags_DataSourceChanged();
                UpdateTagList();
                CreateNotebookList();
                tagView.Nodes[0].EnsureVisible();
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not load database\n" + e.Message, "Error");
                System.Console.Out.Write(e);
            }

        }

        private NotifyCollectionChangedEventHandler tags_DataSourceChanged()
        {
            UpdateTagList();
            return null;
        }

        private NotesContext context;

        private void tagView_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode NewNode;

            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
                NewNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                using (context.Database.BeginTransaction())
                {
                    if (DestinationNode == null)
                    {

                        ((Model.Tag)NewNode.Tag).ParentTag = null;
                    }
                    else if (CheckNoParent(NewNode, DestinationNode))
                    {
                        //Remove Original Node
                        ((Model.Tag)NewNode.Tag).ParentTag = ((Model.Tag)DestinationNode.Tag);

                    }
                }
                context.SaveChangesAsync();
                UpdateTagList();
            }
        }

        private bool CheckNoParent(TreeNode newNode, TreeNode destinationNode)
        {
            if (newNode == destinationNode) return false;
            if (destinationNode == null) return true;
            return CheckNoParent(newNode, destinationNode.Parent);
        }

        private void tagView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move | DragDropEffects.Copy);
        }

        private void tagView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tagView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag is Model.Tag)
            {
                ((Model.Tag)e.Node.Tag).IsExpanded = true;
                context.SaveChangesAsync();
                UpdateTag(e.Node.Nodes, (Model.Tag)e.Node.Tag);
            }
        }

        private void tagView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Model.Tag)
            {
                ((Model.Tag)e.Node.Tag).IsExpanded = false;
                context.SaveChangesAsync();
            }
        }

        private void tagView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag is Model.Tag && e.Label != null)
            {
                try
                {
                    ((Model.Tag)e.Node.Tag).Name = e.Label;
                    context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    e.CancelEdit = true;
                    ((Model.Tag)e.Node.Tag).Name = e.Node.Name;
                    MessageBox.Show(this, "There is already a tag named '" + e.Label + "'", "Rename Aborted");
                }
            }
        }

        private void importFromEvernoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (evernoteImportDialog.ShowDialog() == DialogResult.OK)
            {
                new ProgressDialog().Start((p, c) =>
                new Import.FromEvernote(evernoteImportDialog.FileName, context, 
                Properties.Settings.Default.ProjectLocation + @"/attachments/", p, c).Import(), this);
                UpdateNotebookList();
                UpdateTagList();
                tagView_AfterSelect(this, new TreeViewEventArgs(tagView.SelectedNode));
                tagView.SelectedNode.EnsureVisible();
            }
        }

        private void changeProjectFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectFolderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = Properties.Settings.Default.ProjectLocation 
                    = projectFolderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
                LoadProject();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadProject();
        }

        private void tagView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (!(e.Node.Tag is Model.Tag))
            {
                e.CancelEdit = true;
            }
        }

        private void tagView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateNoteView(e.Node);

        }

        private void UpdateNoteView(TreeNode node)
        {
            if (node == null) return;
            noteBindingSource.SuspendBinding();
            IQueryable<Note> query = null;
            if (node.Tag is Tag)
            {
                query = (from noteTag1 in context.NoteTags
                         where noteTag1.Tag == node.Tag
                         && noteTag1.Note.Notebook != context.Deleted
                         select noteTag1.Note);
            }
            else if (node.Tag is Notebook)
            {
                query = context.Notes.Where(n =>
                                                n.Notebook == node.Tag);

            }
            if (query != null)
            {
                noteBindingSource.DataSource = query.OrderByDescending(n => n.CreateTime).Include("NoteTags.Tag").Include("Attachments").ToList();
                noteBindingSource.Position = 0;
            }
            noteBindingSource.ResetBindings(false);
        }

        private void noteListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            noteDetails1.DataSource = noteBindingSource.Current;
        }

        private void noteListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = noteListView.IndexFromPoint(e.Location);
            if (item >= 0)
            {
                new NoteDetailsForm(noteListView.Items[item], context).Show();
            }
        }

        private void updateProjectTemplateToolStripMenuItem_Click(object sender, EventArgs e) => context.CreateProjectDir();

        private void noteListView_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            // Get the ListBox and the item.
            ListBox lst = sender as ListBox;
            var note = ((Note)lst.Items[e.Index]);
            string titleText = note.Title;
            var tagText = "";
            if (note.NoteTags != null)
            {
                tagText = string.Join(", ", note.NoteTags.Select(nt => nt.Tag.Name));
            }
            var sizeText = "";
            var attachmentText = "";

            if (note.Attachments != null)
            {
                if (note.Attachments.Count == 1)
                {
                    attachmentText = note.Attachments[0].FileName;
                    sizeText = note.Attachments[0].Size.ReadableFileSize();
                }
                if (note.Attachments.Count > 1)
                {
                    attachmentText = "" + note.Attachments.Count + " attachments";
                    sizeText = note.Attachments.Sum(a => a.Size).ReadableFileSize();
                }
            }

            // Draw the background.
            e.DrawBackground();
            var clientBounds = new Rectangle(e.Bounds.Location, e.Bounds.Size);
            clientBounds.Inflate(-2, -2);
            using (var titleFont = new Font(this.Font, FontStyle.Bold))
            {

                // See if the item is selected.
                if ((e.State & DrawItemState.Selected) ==
                    DrawItemState.Selected)
                {
                    
                    e.Graphics.FillRectangle(SystemBrushes.Menu, clientBounds);
                }
                clientBounds.Inflate(-3, 0);
                var sf = TextFormatFlags.EndEllipsis;
                
                var titleSize = e.Graphics.MeasureString(titleText, titleFont);
                using (SolidBrush br = new SolidBrush(lst.ForeColor))
                {
                    TextRenderer.DrawText(e.Graphics, titleText, titleFont,
                        new Rectangle(clientBounds.Left, clientBounds.Top + 5, clientBounds.Width, (int)titleSize.Height*2),
                        lst.ForeColor, sf);
                }

                TextRenderer.DrawText(e.Graphics, tagText, Font, 
                    new Rectangle(clientBounds.Left, clientBounds.Top + 3 + (int)titleSize.Height*2, clientBounds.Width, (int)titleSize.Height),
                    SystemColors.GrayText, sf);
                if (!string.IsNullOrEmpty(attachmentText))
                {
                    var sizeSize = e.Graphics.MeasureString(sizeText, Font);
                    TextRenderer.DrawText(e.Graphics, sizeText, Font, 
                        new Point(clientBounds.Right - (int)sizeSize.Width, clientBounds.Bottom - 36), Color.Gray);
                    TextRenderer.DrawText(e.Graphics, attachmentText, Font,
                        new Rectangle(clientBounds.Left, clientBounds.Bottom - 35, clientBounds.Width - (int)sizeSize.Width - 2, (int)sizeSize.Height),
                        Color.Gray, sf);
                }
                e.Graphics.DrawString(note.CreateTime.ToShortDateString(), Font, Brushes.Gray, clientBounds.Left, clientBounds.Bottom - 18);
                e.Graphics.DrawLine(Pens.LightBlue, clientBounds.Left - 2, clientBounds.Bottom - 1, clientBounds.Right + 2, clientBounds.Bottom - 1);
                // Draw the focus rectangle if appropriate.
                e.DrawFocusRectangle();
            }
        }

        private void noteListView_Resize(object sender, EventArgs e)
        {
            noteListView.Invalidate();
        }

        private void tagView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Tag is string)
            {
                TextRenderer.DrawText(e.Graphics, e.Node.Text, tagView.Font, e.Bounds.Location, e.Node.ForeColor);
            }
            else e.DrawDefault = true;
        }


        private void noteListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                bool? shouldPermenantlyDelete = null;
                foreach (var idx in noteListView.SelectedIndices)
                {
                    var note = noteListView.Items[(int)idx] as Note;
                    if (note.Notebook.Name == "Deleted")
                    {
                        if (shouldPermenantlyDelete == null)
                        {
                            shouldPermenantlyDelete = MessageBox.Show(this,
                                "Items would be permenantly deleted - this is not undoable\n Are you sure?", "Permenantly Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
                            if (!shouldPermenantlyDelete ?? false)
                            {
                                continue;
                            }
                      
                        }
                    }
                    context.DeleteNote(note);
                    ((IList)noteListView.DataSource).Remove(note);
                }
                context.SaveChangesAsync();
                UpdateTagList();
                UpdateNotebookList();
            
                //UpdateNoteView(tagView.SelectedNode);

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void emptyDeletedNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                                "Items would be permenantly deleted - this is not undoable\n Are you sure?", "Permenantly Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes
            ) {
                foreach(var note in from note in context.Notes where note.Notebook == context.Deleted select note)
                {
                    context.Notes.Remove(note);
                }
            }
            context.SaveChangesAsync();
            UpdateTagList();
            UpdateNotebookList();
        }

        private void restoireAllDeletedNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var notebook = context.Notebooks.First();
            foreach (var note in from note in context.Notes where note.Notebook == context.Deleted select note)
            {
                note.Notebook = notebook;
            }
            context.SaveChangesAsync();
            UpdateNotebookList();
            UpdateTagList();
        }

        private void noteDetails1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                var node = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                if (node.Tag is Tag)
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void noteDetails1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                var node = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                var tag = node.Tag as Tag;
                if (tag != null)
                {
                    foreach (var noteTag in noteDetails1.Note.NoteTags)
                    {
                        if (noteTag.Tag == tag) return;
                    }
                    noteDetails1.Note.NoteTags.Add(new NoteTag { Note = noteDetails1.Note, Tag = tag });
                }
            }
        }
    }
}
