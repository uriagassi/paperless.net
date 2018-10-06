namespace Paperless
{
    partial class NoteDetails
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.title = new System.Windows.Forms.TextBox();
            this.noteBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.notebook = new System.Windows.Forms.ComboBox();
            this.notebookBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.noteTagsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.noteContents = new System.Windows.Forms.WebBrowser();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.noteTagsList1 = new Paperless.Controls.NoteTagsList();
            this.noteTagsBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.noteBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.notebookBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.noteTagsBindingSource)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noteTagsBindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.title, 3);
            this.title.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.noteBindingSource, "Title", true));
            this.title.Location = new System.Drawing.Point(3, 3);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(627, 20);
            this.title.TabIndex = 0;
            // 
            // noteBindingSource
            // 
            this.noteBindingSource.DataSource = typeof(Paperless.Model.Note);
            this.noteBindingSource.CurrentChanged += new System.EventHandler(this.noteBindingSource_CurrentChanged);
            // 
            // notebook
            // 
            this.notebook.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.noteBindingSource, "Notebook", true));
            this.notebook.DataSource = this.notebookBindingSource;
            this.notebook.DisplayMember = "Name";
            this.notebook.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.notebook.FormattingEnabled = true;
            this.notebook.Location = new System.Drawing.Point(3, 30);
            this.notebook.Name = "notebook";
            this.notebook.Size = new System.Drawing.Size(144, 21);
            this.notebook.TabIndex = 1;
            // 
            // notebookBindingSource
            // 
            this.notebookBindingSource.DataSource = typeof(Paperless.Model.Notebook);
            // 
            // noteTagsBindingSource
            // 
            this.noteTagsBindingSource.DataMember = "NoteTags";
            this.noteTagsBindingSource.DataSource = this.noteBindingSource;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePicker1.CustomFormat = "dd/MM/yyyy hh:mm";
            this.dateTimePicker1.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.noteBindingSource, "CreateTime", true));
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(486, 30);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(144, 20);
            this.dateTimePicker1.TabIndex = 3;
            this.dateTimePicker1.Value = new System.DateTime(2018, 9, 29, 0, 0, 0, 0);
            // 
            // noteContents
            // 
            this.noteContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noteContents.Location = new System.Drawing.Point(0, 55);
            this.noteContents.MinimumSize = new System.Drawing.Size(20, 20);
            this.noteContents.Name = "noteContents";
            this.noteContents.Size = new System.Drawing.Size(633, 431);
            this.noteContents.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.Controls.Add(this.notebook, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.title, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dateTimePicker1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.noteTagsList1, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(633, 55);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // noteTagsList1
            // 
            this.noteTagsList1.AllowDrop = true;
            this.noteTagsList1.DataMember = "";
            this.noteTagsList1.DataSource = this.noteTagsBindingSource1;
            this.noteTagsList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noteTagsList1.Location = new System.Drawing.Point(153, 30);
            this.noteTagsList1.Name = "noteTagsList1";
            this.noteTagsList1.Size = new System.Drawing.Size(327, 22);
            this.noteTagsList1.TabIndex = 4;
            this.noteTagsList1.AddTag += new System.EventHandler<Paperless.Controls.NoteTagsList.TextEventArgs>(this.noteTagsList1_AddTag);
            // 
            // noteTagsBindingSource1
            // 
            this.noteTagsBindingSource1.DataMember = "NoteTags";
            this.noteTagsBindingSource1.DataSource = this.noteBindingSource;
            // 
            // NoteDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.noteContents);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NoteDetails";
            this.Size = new System.Drawing.Size(633, 486);
            ((System.ComponentModel.ISupportInitialize)(this.noteBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.notebookBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.noteTagsBindingSource)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noteTagsBindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox title;
        private System.Windows.Forms.ComboBox notebook;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.WebBrowser noteContents;
        private System.Windows.Forms.BindingSource noteBindingSource;
        private System.Windows.Forms.BindingSource noteTagsBindingSource;
        private System.Windows.Forms.BindingSource notebookBindingSource;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Controls.NoteTagsList noteTagsList1;
        private System.Windows.Forms.BindingSource noteTagsBindingSource1;
    }
}
