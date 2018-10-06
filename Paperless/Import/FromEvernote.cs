using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Paperless.Model;
using Paperless.Utils;

namespace Paperless.Import
{
    class FromEvernote
    {
        private string fileName;
        private NotesContext context;
        private string attachmentDir;
        private IProgress<ImportProgress> progress;
        private CancelEventArgs cancelToken;

        public FromEvernote(string fileName, Model.NotesContext context,string attachmentDir,
            IProgress<ImportProgress> progress, CancelEventArgs cancelToken)
        {
            this.fileName = fileName;
            this.context = context;
            this.attachmentDir = attachmentDir;
            this.progress = progress;
            this.cancelToken = cancelToken;
        }

        internal bool Import()
        {
            try
            {
                int imported = 0;
                byte[] bytes = new byte[17];
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Read(bytes, 0, 16);
                }
                string chkStr = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
                if (chkStr.Contains("SQLite format"))
                {
                    Report(new ImportProgress { Title = "Import Notes" });
                    using (SQLiteConnection conn = new SQLiteConnection(@"Data Source=" + fileName))
                    {
                        conn.Open();
                        using (SQLiteCommand tagCount = conn.CreateCommand())
                        {
                            tagCount.CommandText = "SELECT count(*) from tag_attr";
                            tagCount.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader r = tagCount.ExecuteReader();
                            r.Read();
                            Report(new ImportProgress { MaxValue = (int)(long)r[0] });
                        }
                        using (SQLiteCommand tagSelect = conn.CreateCommand())
                        {
                            tagSelect.CommandText = "SELECT uid, parent_uid, name FROM tag_attr";
                            tagSelect.CommandType = System.Data.CommandType.Text;
                            SQLiteDataReader r = tagSelect.ExecuteReader();
                            Dictionary<object, Model.Tag> tags = new Dictionary<object, Tag>();
                            Dictionary<Tag, object> parents = new Dictionary<Tag, object>();
                            while (r.Read())
                            {

                                var tag = context.Tags.FirstOrDefault(t => t.Name == (string)r["name"]);
                                if (tag == null)
                                {
                                    tag = new Tag { Name = (string)r["name"] };
                                }
                                if (r["parent_uid"] != DBNull.Value)
                                {
                                    parents.Add(tag, r["parent_uid"]);
                                }
                                tags.Add(r["uid"], tag);
                                Report(new ImportProgress { Progress = imported++, Text = "Imported " + imported + " notes" });
                            }
                            Report(new ImportProgress { Text = "Finishing up..." });
                            foreach (var tag in tags.Values)
                            {
                                AddTag(tag, tags, parents);
                            }
                        }
                    }
                }
                else
                {
                    ProcessEnex(fileName, progress);
                }
                context.SaveChanges();
                return true;
            } catch (TaskCanceledException)
            {
                progress.Report(new ImportProgress { Text = "Cancelled operation" });
                return false;
            }
        }

        private void Report(ImportProgress importProgress)
        {
            progress.Report(importProgress);
            if (cancelToken.Cancel) throw new TaskCanceledException();
        }

        

        private void ProcessEnex(string fileName, IProgress<ImportProgress> progress)
        {
            int imported = 0;
            long read = 0;
            long maxSize = (new FileInfo(fileName).Length * 3) / 4;
            Report(new ImportProgress { Title = "Importing Notes", MaxValue= (int)(maxSize/1000) });
            Notebook mainNotebook = context.Notebooks.First();
            CultureInfo provider = CultureInfo.InvariantCulture;
            foreach (var note in ReadNotes(fileName))
            {
                Note newNote = new Note()
                {
                    Notebook = mainNotebook,
                    Attachments = new List<Attachment>()
                };
                foreach (var element in ReadElements(note, "note"))
                {
                    switch(element.Name)
                    {
                        case "title":
                            newNote.Title = element.ReadElementContentAsString();
                             break;
                        case "created":
                            newNote.CreateTime = DateTime.ParseExact(element.ReadElementContentAsString(), "yyyyMMdd'T'HHmmss'Z'", provider);
                            if (newNote.UpdateTime == null) newNote.UpdateTime = newNote.CreateTime;
                        break;
                        case "content":
                            newNote.NoteData = element.ReadElementContentAsString();
                            break;
                        case "updated":
                            newNote.UpdateTime = DateTime.ParseExact(element.ReadElementContentAsString(), "yyyyMMdd'T'HHmmss'Z'", provider);
                            break;
                        case "tag":
                            string tagNode = element.ReadElementContentAsString();
                            context.AddTag(newNote, tagNode);
                            break;
                        case "resource":
                            Attachment att = new Attachment();
                            byte[] data = null;
                            foreach (var attElement in ReadElements(element, "resource"))
                            {
                                switch (attElement.Name)
                                {
                                    case "mime":
                                        att.Mime = attElement.ReadElementContentAsString();
                                        break;
                                    case "file-name":
                                        att.FileName = attElement.ReadElementContentAsString();
                                        break;
                                    case "source-url":
                                        if (att.FileName == null)
                                        {
                                            att.FileName = attElement.ReadElementContentAsString().Split('/').Last().Split('\\').Last();
                                        }
                                        else
                                        {
                                            attElement.Read();
                                        }
                                        break;
                                    case "data":
                                        byte[] buffer = new byte[10240];
                                        int readBytes = 0;
                                        using (MemoryStream ms = new MemoryStream())
                                        using (BinaryWriter bin = new BinaryWriter(ms))
                                        {
                                            while ((readBytes = attElement.ReadElementContentAsBase64(buffer, 0, 10240)) > 0)
                                            {
                                                bin.Write(buffer, 0, readBytes);
                                                read += readBytes;
                                                Report(new ImportProgress { Progress = (int)(read / 1000) });
                                            }
                                            bin.Flush();
                                            data = ms.ToArray();
                                        }
                                        break;
                                    default:
                                        attElement.Read();
                                        break;
                                }
                            }
                            if (att.FileName == null)
                                att.FileName = newNote.Title + ExtensionHelper.GetExtension(att.Mime, "");
                            att.SetAttachmentFile(attachmentDir, data);
                            context.Attachments.Add(att);
                            newNote.Attachments.Add(att);
                            break;
                        default:
                            element.Read();
                            break;
                    }
                  

                }
                newNote.NoteData = fixAttachments(newNote);
                newNote.Notebook.Notes.Add(newNote);
                Report(new ImportProgress { Text = "" + imported++ + " notes imported\n\n" + read.ReadableFileSize() + " / " + maxSize.ReadableFileSize() + " unpacked..." });

            }
            Report(new ImportProgress { Text = "Finishing up..." });
        }

        private static Regex attachmentRegex = new Regex("<en-media.*?hash=\"([^\"]+)\".*?/>", RegexOptions.Compiled);

        private string fixAttachments(Note note)
        {
            //if (note.Attachments.Count == 0) return note.NoteData;
            return attachmentRegex.Replace(note.NoteData, delegate (Match match)
            {
                Attachment att = note.Attachments.Find(x => x.Hash == match.Groups[1].Value.ToLower());
                if (att == null)
                {
                    context.AddTag(note, ".__Corrupt__");
                    return match.Groups[0].Value;
                }
                return att.GetHTMLTag();
            });
        }

        private IEnumerable<XmlReader> ReadElements(XmlReader reader, string tagName)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tagName) break;
                while (reader.NodeType == XmlNodeType.Element)
                {
                    yield return reader;
                }
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tagName) break;
            }
        }

        private IEnumerable<XmlReader> ReadNotes(string fileName) { 
            XmlReaderSettings readerSettings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
            };
            using (XmlReader reader = XmlReader.Create(fileName, readerSettings))
            {
                PropertyInfo propertyInfo = reader.GetType().GetProperty("DisableUndeclaredEntityCheck", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                propertyInfo.SetValue(reader, true);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        while (reader.NodeType == XmlNodeType.Element && reader.Name == "note")
                        {
                            yield return reader;
                        }
                    }
                }
            }
        }

        private void AddTag(Tag tag, Dictionary<object, Tag> tags, Dictionary<Tag, object> parents)
        {
            if (tag.TagId == 0)
            {
                if (parents.ContainsKey(tag))
                {
                  
                    tag.ParentTag = tags[parents[tag]];
                    AddTag(tag.ParentTag, tags, parents);
                }
                context.Tags.Add(tag);
            } else if (parents.ContainsKey(tag) ^ tag.ParentTag != null)
            {
                tag.ParentTag = tags[parents[tag]];
            }
        }
    }
}
