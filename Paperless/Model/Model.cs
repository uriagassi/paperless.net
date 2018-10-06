using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Paperless.Utils;

namespace Paperless.Model
{
    public class ObservableListSource<T> : ObservableCollection<T>, IListSource
    where T : class
    {
        private IBindingList _bindingList;

        bool IListSource.ContainsListCollection { get { return false; } }

        IList IListSource.GetList()
        {
            return _bindingList ?? (_bindingList = this.ToBindingList());
        }
    }

    public class NotesContext : DbContext
    {
        public NotesContext(string projectLocation)
        {
            //super();
            this.projectLocation = projectLocation;
        }
        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteTag> NoteTags { get; set; }
        public DbSet<Attachment>  Attachments { get; set; }
        public Notebook Deleted {
            get
            {
                if (_deleted == null)
                {
                    _deleted = (from n in Notebooks
                                where n.Name == "Deleted"
                                select n).FirstOrDefault();
                    if (_deleted == null)
                    {
                        _deleted = new Notebook { Name = "Deleted" };
                        Notebooks.Add(_deleted);
                        SaveChangesAsync();
                    }
                }
                return _deleted;
            }
        }

        private bool newDb = false;
        private string projectLocation;
        private Notebook _deleted;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            System.IO.DirectoryInfo rootDirectory = new System.IO.DirectoryInfo(projectLocation);
            if (!rootDirectory.Exists)
            {
                rootDirectory.Create();
            }
            System.IO.FileInfo sqlSource = new System.IO.FileInfo(rootDirectory.FullName + @"\paperless.sqlite");
            if (!sqlSource.Exists)
            {
                CreateProjectDir();
                newDb = true;
                SQLiteConnection.CreateFile(sqlSource.FullName);
                //var conn = new SQLiteConnection("Data Source=" + sqlSource.FullName);

            }
            optionsBuilder.UseSqlite("Data Source=" + sqlSource.FullName);

        }

        public void CreateProjectDir()
        {
            DirectoryInfo imagesDirectory = new DirectoryInfo(Path.Combine(projectLocation, "images"));
            imagesDirectory.Create();
            new DirectoryInfo(Path.Combine(projectLocation, "js")).Create();
            new DirectoryInfo(Path.Combine(projectLocation, "css")).Create();

            using (ZipArchive zip = new ZipArchive(new MemoryStream(PaperlessResources.images)))
            {

                foreach (var entry in zip.Entries)
                {
                    using (var output = File.Create(Path.Combine(imagesDirectory.FullName, entry.FullName)))
                    {
                        entry.Open().CopyTo(output);
                    }
                }
            }
            using (var js = File.CreateText(Path.Combine(projectLocation, @"js\paperless.js")))
            {
                js.Write(PaperlessResources.paperless_js);
            }
            using (var css = File.CreateText(Path.Combine(projectLocation, @"css\paperless.css")))
            {
                css.Write(PaperlessResources.paperless_css);
            }
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
                
                newDb = false;
            }
            if (Notebooks.Count() == 0)
            {
                Notebooks.Add(new Notebook { Name = "Archive" });
                Notebooks.Add(new Notebook { Name = "Deleted" });
                SaveChanges();
            }
        }
        public override int SaveChanges()
        {
            var updatedNotes = ChangeTracker.Entries()
                .Where(e => e.Entity is Note && (e.State == EntityState.Modified || e.State == EntityState.Added)).ToList();
            var currentTime = DateTime.UtcNow;
            foreach (var entry in updatedNotes)
            {
                var entityBase = entry.Entity as Note;
                if (entityBase == null) continue;
                if (entry.State == EntityState.Added)
                {
                    entityBase.CreateTime = currentTime;
                }
                entityBase.UpdateTime = currentTime;
            }

            var deletedAttachments = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();
            string attDir = Path.Combine(projectLocation, "attachments");
            foreach (var del in deletedAttachments)
            {
                if (del is Attachment)
                {
                    (del as Attachment).Delete(attDir);
                } else if (del is Note)
                {
                    foreach (var att in ((Note)del).Attachments)
                    {
                        att.Delete(attDir);
                    }
                }
            }
            var changed = ChangeTracker.Entries().Where(i => i.State != EntityState.Unchanged).ToList();

            return base.SaveChanges();
        }

        public void DeleteNote(Note note)
        {
            if (note.Notebook == Deleted)
            {
                Notes.Remove(note);
            } else
            {
                note.Notebook = Deleted;
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
        public long Size { get; set; }

        internal string GetHTMLTag()
        {
            if (Mime.StartsWith("image"))
            {
                return "<img class='paperless-attachment' src='attachments/" + UniqueFileName + "' hash='" + Hash + "'/>";
            }
            else if (Mime.EndsWith("pdf"))
            {
                return "<embed class='paperless-attachment' src='attachments/" + UniqueFileName + "' type='" + Mime + "' hash='" + Hash + "'/>";
            }
            else
            {
                return "<div class='paperless-attachment-file' data-ext='" + ExtensionHelper.GetExtension(Mime, FileName) + "'" +
                " data-src='attachments/" + UniqueFileName + "'><span>&nbsp;</span><span>" + FileName + "</span>\n" +
"<span>" + Size.ReadableFileSize() + " </span></div>";
            }
        }

        public void SetAttachmentFile(string attachmentDir, byte[] data)
        {
            FileName = Regex.Replace(FileName, @"[\/:"" *?<>|&=;]+", "_");
            if (string.IsNullOrEmpty(Path.GetExtension(FileName)))
            {
                FileName += ExtensionHelper.GetExtension(Mime, "");
            }
            if (FileName.Length > 55)
            {
                FileName = Path.GetFileNameWithoutExtension(FileName).Substring(0, 50) + new string(Path.GetExtension(FileName).Take(5).ToArray());
            }
            var fileInfo = new FileInfo(attachmentDir + FileName);
            UniqueFileName = fileInfo.Exists ?
                Path.GetFileNameWithoutExtension(FileName) + Environment.TickCount + "." + fileInfo.Extension : FileName;
            Directory.CreateDirectory(attachmentDir);
            Size = data.Length;
            File.WriteAllBytes(attachmentDir + UniqueFileName, data);
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);
            Hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public void Delete(string attachmentDir)
        {
            File.Delete(Path.Combine(attachmentDir, UniqueFileName));
        }
    }

    public class Note
    {
        private readonly ObservableListSource<NoteTag> _noteTags =
                    new ObservableListSource<NoteTag>();
        [Key]
        public int NodeId { get; set; }
        [Required]
        public Notebook Notebook { get; set; }
        public virtual ObservableListSource<NoteTag> NoteTags { get { return _noteTags;  } }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Title { get; set; }
        public string NoteData { get; set; }
        public List<Attachment> Attachments { get; set;}
    }
}
