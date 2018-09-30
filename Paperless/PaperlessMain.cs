using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Specialized;
using Paperless.Model;
using Microsoft.EntityFrameworkCore;

namespace Paperless
{
    public partial class PaperlessMain : Form
    {
        public PaperlessMain()
        {
            WebBrowserHelper.FixBrowserVersion();
            InitializeComponent();
        }

        private void tags_DataSourceChanged(object sender, EventArgs e)
        {
            UpdateTagList();

        }

        private void UpdateTagList()
        {
            context.CreateIfNeeded();
            tagView.SuspendLayout();
            try
            {
                UpdateTag(tagView.Nodes["Tags"].Nodes, null);
            } finally
            {
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
                                         where cNode.Tag == children[i]
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

        private void PaperlessMain_Load(object sender, EventArgs e)
        {
            LoadProject();
        }

        private void LoadProject()
        {
            try { 
            context = new Model.NotesContext();
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

        private Paperless.Model.NotesContext context;

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
                context.SaveChanges();
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
            DoDragDrop(e.Item, DragDropEffects.Move);
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
                context.SaveChanges();
                UpdateTag(e.Node.Nodes, (Model.Tag)e.Node.Tag);
            }
        }

        private void tagView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Model.Tag)
            {
                ((Model.Tag)e.Node.Tag).IsExpanded = false;
                context.SaveChanges();
            }
        }

        private void tagView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag is Model.Tag)
            {
                try
                {
                    ((Model.Tag)e.Node.Tag).Name = e.Label;
                    context.SaveChanges();
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
            if (e.Node == null) return;
            noteBindingSource.SuspendBinding();

            if (e.Node.Tag is Model.Tag)
            {
                noteBindingSource.DataSource = (from noteTag1 in context.NoteTags
                                               where noteTag1.Tag == e.Node.Tag
                                               select noteTag1.Note).Include(n=>n.NoteTags).ToList();
            } else if (e.Node.Tag is Notebook)
            {
                noteBindingSource.DataSource = context.Notes.Where(n =>
                                                n.Notebook == e.Node.Tag).Include(n => n.NoteTags).ToList();
                                              
            }
            noteBindingSource.ResetBindings(false);

        }

        private void noteListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*webBrowser1.DocumentText = "0";
            webBrowser1.Document.OpenNew(true);
            if (noteBindingSource.Current != null)
            {
                var text = (noteBindingSource.Current as Note).NoteData;
                 text = "<html><head>" +
                    
                    "<base href='" + new Uri(Properties.Settings.Default.ProjectLocation + "\\").AbsoluteUri + "'/>" +
                    "<link rel='stylesheet' type='text/css' href='css/paperless.css'/>" +
                    "<meta http-equiv='X-UA-Compatible' content='IE=11'>" +
                    "<script src='js/paperless.js'></script>" +
                    "</head><body>"
                     + text + "</body></html>";
                webBrowser1.Document.Write(text);
 
            }
            webBrowser1.Refresh();*/
            noteDetails1.DataSource = noteBindingSource.Current;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            /*HtmlElement head = webBrowser1.Document.GetElementsByTagName("head")[0];
            if (head.GetElementsByTagName("base").Count == 0)
            {
                HtmlElement baseElement = webBrowser1.Document.CreateElement("base");
                baseElement.SetAttribute("href", new Uri(Properties.Settings.Default.ProjectLocation).AbsoluteUri);
                head.AppendChild(baseElement);
                //webBrowser1.Refresh();
            }*/
           /* IHTMLDocument2 doc = (webBrowser1.Document.DomDocument) as IHTMLDocument2;
            // The first parameter is the url, the second is the index of the added style sheet.
            IHTMLStyleSheet ss = doc.createStyleSheet("", 0);

            // Now that you have the style sheet you have a few options:
            // 1. You can just set the content as text.
            //ss.cssText = @"h1 { color: blue; }";
            // 2. You can add/remove style rules.
            int index = ss.addRule(".attachment", "max-width: 100%; height: auto;");
            //ss.removeRule(index);*/
        }

        private void noteListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = noteListView.IndexFromPoint(e.Location);
            if (item >= 0)
            {
                new NoteDetailsForm(noteListView.Items[item], context).Show();
            }
        }

        private void updateProjectTemplateToolStripMenuItem_Click(object sender, EventArgs e) => NotesContext.CreateProjectDir();
    }
}
