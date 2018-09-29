namespace Paperless
{
    partial class NoteDetailsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.noteDetails1 = new Paperless.NoteDetails();
            this.noteBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.noteBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // noteDetails1
            // 
            this.noteDetails1.Context = null;
            this.noteDetails1.DataSource = null;
            this.noteDetails1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.noteDetails1.Location = new System.Drawing.Point(0, 0);
            this.noteDetails1.Name = "noteDetails1";
            this.noteDetails1.Note = null;
            this.noteDetails1.Size = new System.Drawing.Size(800, 450);
            this.noteDetails1.TabIndex = 0;
            // 
            // noteBindingSource
            // 
            this.noteBindingSource.DataSource = typeof(Paperless.Model.Note);
            // 
            // NoteDetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.noteDetails1);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.noteBindingSource, "Title", true));
            this.Name = "NoteDetailsForm";
            this.Text = "NoteDetailsForm";
            ((System.ComponentModel.ISupportInitialize)(this.noteBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private NoteDetails noteDetails1;
        private System.Windows.Forms.BindingSource noteBindingSource;
    }
}