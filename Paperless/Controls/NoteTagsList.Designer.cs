namespace Paperless.Controls
{
    partial class NoteTagsList
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.noteTagView1 = new Paperless.Controls.NoteTagView();
            this.noteTags = new System.Windows.Forms.BindingSource(this.components);
            this.showMore = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noteTags)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.noteTagView1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(480, 20);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.ClientSizeChanged += new System.EventHandler(this.flowLayoutPanel1_ClientSizeChanged);
            // 
            // noteTagView1
            // 
            this.noteTagView1.AutoSize = true;
            this.noteTagView1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.noteTagView1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.noteTagView1.DataSource = typeof(Paperless.Model.NoteTag);
            this.noteTagView1.Location = new System.Drawing.Point(3, 0);
            this.noteTagView1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.noteTagView1.MaximumSize = new System.Drawing.Size(500, 20);
            this.noteTagView1.MinimumSize = new System.Drawing.Size(0, 20);
            this.noteTagView1.Name = "noteTagView1";
            this.noteTagView1.NotesContext = null;
            this.noteTagView1.Size = new System.Drawing.Size(74, 20);
            this.noteTagView1.TabIndex = 0;
            // 
            // noteTags
            // 
            this.noteTags.DataSource = typeof(Paperless.Model.NoteTag);
            this.noteTags.DataSourceChanged += new System.EventHandler(this.noteTags_DataSourceChanged);
            this.noteTags.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.noteTags_ListChanged);
            // 
            // showMore
            // 
            this.showMore.Dock = System.Windows.Forms.DockStyle.Right;
            this.showMore.Location = new System.Drawing.Point(480, 0);
            this.showMore.Name = "showMore";
            this.showMore.Size = new System.Drawing.Size(20, 20);
            this.showMore.TabIndex = 1;
            this.showMore.UseVisualStyleBackColor = true;
            this.showMore.Visible = false;
            this.showMore.Paint += new System.Windows.Forms.PaintEventHandler(this.showMore_Paint);
            // 
            // NoteTagsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.showMore);
            this.Name = "NoteTagsList";
            this.Size = new System.Drawing.Size(500, 20);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noteTags)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.BindingSource noteTags;
        private NoteTagView noteTagView1;
        private System.Windows.Forms.Button showMore;
    }
}
