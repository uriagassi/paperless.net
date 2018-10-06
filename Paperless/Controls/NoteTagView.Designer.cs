namespace Paperless.Controls
{
    partial class NoteTagView
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
            this.deletePanel = new System.Windows.Forms.Panel();
            this.tagName = new System.Windows.Forms.Label();
            this.tag = new System.Windows.Forms.BindingSource(this.components);
            this.noteTag = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.tag)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.noteTag)).BeginInit();
            this.SuspendLayout();
            // 
            // deletePanel
            // 
            this.deletePanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.deletePanel.Location = new System.Drawing.Point(54, 0);
            this.deletePanel.Name = "deletePanel";
            this.deletePanel.Size = new System.Drawing.Size(20, 20);
            this.deletePanel.TabIndex = 0;
            this.deletePanel.Click += new System.EventHandler(this.deletePanel_Click);
            this.deletePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.deletePanel_Paint);
            this.deletePanel.MouseEnter += new System.EventHandler(this.tagName_MouseEnter);
            this.deletePanel.MouseLeave += new System.EventHandler(this.tagName_MouseEnter);
            // 
            // tagName
            // 
            this.tagName.AutoSize = true;
            this.tagName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.tag, "Name", true));
            this.tagName.Dock = System.Windows.Forms.DockStyle.Right;
            this.tagName.Location = new System.Drawing.Point(0, 0);
            this.tagName.MinimumSize = new System.Drawing.Size(0, 20);
            this.tagName.Name = "tagName";
            this.tagName.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.tagName.Size = new System.Drawing.Size(54, 20);
            this.tagName.TabIndex = 1;
            this.tagName.Text = "tag name";
            this.tagName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.tagName.DragEnter += new System.Windows.Forms.DragEventHandler(this.tagName_DragEnter);
            this.tagName.MouseEnter += new System.EventHandler(this.tagName_MouseEnter);
            this.tagName.MouseLeave += new System.EventHandler(this.tagName_MouseEnter);
            // 
            // tag
            // 
            this.tag.DataMember = "Tag";
            this.tag.DataSource = this.noteTag;
            // 
            // noteTag
            // 
            this.noteTag.DataSource = typeof(Paperless.Model.NoteTag);
            // 
            // NoteTagView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.Controls.Add(this.tagName);
            this.Controls.Add(this.deletePanel);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.MaximumSize = new System.Drawing.Size(500, 20);
            this.MinimumSize = new System.Drawing.Size(0, 20);
            this.Name = "NoteTagView";
            this.Size = new System.Drawing.Size(74, 20);
            ((System.ComponentModel.ISupportInitialize)(this.tag)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.noteTag)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel deletePanel;
        private System.Windows.Forms.Label tagName;
        private System.Windows.Forms.BindingSource noteTag;
        private System.Windows.Forms.BindingSource tag;
    }
}
