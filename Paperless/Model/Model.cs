using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace Paperless.Model
{
    class NotesContext : DbContext
    {
        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteTag> NoteTags { get; set; }
        public DbSet<Attachment>  Attachments { get; set; }

        private bool newDb = false;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            System.IO.DirectoryInfo rootDirectory = new System.IO.DirectoryInfo(Properties.Settings.Default.ProjectLocation);
                if (!rootDirectory.Exists)
                {
                    rootDirectory.Create();
                }
                System.IO.FileInfo sqlSource = new System.IO.FileInfo(rootDirectory.FullName + @"\paperless.sqlite");
                if (!sqlSource.Exists)
                {
                    newDb = true;
                    SQLiteConnection.CreateFile(sqlSource.FullName);
                    //var conn = new SQLiteConnection("Data Source=" + sqlSource.FullName);

                }
                optionsBuilder.UseSqlite("Data Source=" + sqlSource.FullName);

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Tag>().HasIndex(n => n.Name).IsUnique();
            modelBuilder.Entity<NoteTag>()
    .HasKey(t => new { t.NoteId, t.TagId });

            modelBuilder.Entity<NoteTag>()
                .HasOne(pt => pt.Note)
                .WithMany(p => p.NoteTags)
                .HasForeignKey(pt => pt.NoteId);

            modelBuilder.Entity<NoteTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(pt => pt.TagId);
        }

        public void CreateIfNeeded() { 
            
            if (newDb)
            {
                Database.EnsureCreated();
                //Database.ExecuteSqlCommand(Database.GenerateCreateScript());
                var rootNode = Tags.Add(new Tag { Name = "Root" });
                Tags.Add(new Tag { Name = "Child", ParentTag = rootNode.Entity });
                SaveChanges();
                newDb = false;
            }
            if (Notebooks.Count() == 0)
            {
                Notebooks.Add(new Notebook { Name = "Archive" });
                SaveChanges();
            }
        }

    }

    public class Notebook
    {
      
        public int NotebookId { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<Note> Notes { get; set; }
    }

    public class Tag
    {
        public int TagId { get; set; }
        public Tag ParentTag { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<NoteTag> NoteTags { get; set; }
        public bool IsExpanded { get; set; }
    }

    public class NoteTag
    {
        public int TagId { get; set; }
        public Tag Tag { get; set; }

        public int NoteId { get; set; }
        public Note Note { get; set; }
    }

    public class Attachment
    {
        [Key]
        public int AttachmentId { get; set; }
        public String FileName { get; set; }
        public String UniqueFileName { get; set; }
        public String Mime { get; set; }
        public String Hash { get; set; }
    }

    public class Note
    {
        [Key]
        public int NodeId { get; set; }
        [Required]
        public Notebook Notebook { get; set; }
        public List<NoteTag> NoteTags { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Title { get; set; }
        public string NoteData { get; set; }
        public List<Attachment> Attachments { get; set;}
    }
}
