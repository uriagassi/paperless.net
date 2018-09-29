using System;
using System.Collections.Generic;
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

        public FromEvernote(string fileName, Model.NotesContext context,string attachmentDir)
        {
            this.fileName = fileName;
            this.context = context;
            this.attachmentDir = attachmentDir;
        }

        internal void Import()
        {
            byte[] bytes = new byte[17];
            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Read(bytes, 0, 16);
            }
            string chkStr = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
            if (chkStr.Contains("SQLite format"))
            {
                using (SQLiteConnection conn = new SQLiteConnection(@"Data Source=" + fileName))
                {
                    conn.Open();
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
                        }
                        foreach (var tag in tags.Values)
                        {
                            AddTag(tag, tags, parents);
                        }
                    }
                }
            } else
            {
                ProcessEnex(fileName);
            }
            context.SaveChanges();
        }

        private void ProcessEnex(string fileName)
        {
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
                            AddTag(newNote, tagNode);
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
                                        } else
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
                            att.FileName = Regex.Replace(att.FileName, @"[\/:"" *?<>|&=;]+", "_");
                            if (String.IsNullOrEmpty(Path.GetExtension(att.FileName))) {
                                att.FileName += ExtensionHelper.GetExtension(att.Mime, "");
                            }
                            if (att.FileName.Length > 55)
                            {
                                att.FileName = Path.GetFileNameWithoutExtension(att.FileName).Substring(0, 50) + new string(Path.GetExtension(att.FileName).Take(5).ToArray());
                            }
                            var fileInfo = new FileInfo(attachmentDir + att.FileName);


                            att.UniqueFileName = fileInfo.Exists ?
                                Path.GetFileNameWithoutExtension(att.FileName) + Environment.TickCount + "." + fileInfo.Extension : att.FileName;
                            Directory.CreateDirectory(attachmentDir);
                            File.WriteAllBytes(attachmentDir + att.UniqueFileName, data);
                            MD5 md5 = MD5.Create();
                            byte[] hash = md5.ComputeHash(data);
                            att.Hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
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

            }
        }

        private void AddTag(Note note, string tagName)
        {
            Tag tag = context.Tags.Local.FirstOrDefault(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                context.Tags.Add(tag);
            }
            context.NoteTags.Add(new NoteTag { Note = note, Tag = tag });
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
                    AddTag(note, "__Corrupt__");
                    return match.Groups[0].Value;
                }
                if (att.Mime.StartsWith("image"))
                {
                    return "<img class='paperless-attachment' src='attachments/" + att.UniqueFileName + "' hash='" + att.Hash + "'/>";
                }
                else if (att.Mime.EndsWith("pdf"))
                {
                    return "<embed class='paperless-attachment' src='attachments/" + att.UniqueFileName + "' type='" + att.Mime + "' hash='" + att.Hash + "'/>";
                } else
                {
                    return "<div class='paperless-attachment-file' data-ext='"+ ExtensionHelper.GetExtension(att.Mime, att.FileName) +"'" +
                    " data-src='attachments/" + att.UniqueFileName + "'><span>&nbsp;</span><span>" + att.FileName + "</span>\n" +
"<span>13.0 KB </span></div>";
                }
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
