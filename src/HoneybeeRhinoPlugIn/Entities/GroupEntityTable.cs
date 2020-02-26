using Rhino.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneybeeRhino.Entities
{
    public class GroupEntityTable : Dictionary<Guid, GroupEntity>
    {
   
        /// <summary>
        /// Class major and minor verson numbers
        /// </summary>
        private const int MAJOR = 1;
        private const int MINOR = 0;
        public GroupEntityTable Duplicate()
        {
            var newTb = new GroupEntityTable();
            foreach (var item in this)
            {
                newTb.Add(item.Key, item.Value);
            }
            return newTb;
        }
        /// <summary>
        /// Write to binary archive
        /// </summary>
        public bool WriteDocument(BinaryArchiveWriter archive)
        { 
            var rc = false;
            if (null != archive)
            {
                try
                {
                    archive.Write3dmChunkVersion(MAJOR, MINOR);
                    //Filter out invalid groups.
                    var validGroups = this.Where(_ => _.Value.IsValid);
                    var counts = validGroups.Count();
                    archive.WriteInt(counts);
                    foreach (var item in validGroups)
                    {
                        archive.WriteGuid(item.Key);
                        item.Value.WriteToArchive(archive);
                    }
                    rc = archive.WriteErrorOccured;
                }
                catch
                {
                    // ignored
                }
            }
            return rc;
        }

        /// <summary>
        /// Read from binary archive
        /// </summary>
        public bool ReadDocument(BinaryArchiveReader archive)
        {
            var rc = false;
            if (null != archive)
            {
                try
                {
                    archive.Read3dmChunkVersion(out var major, out var minor);
                    if (major == MAJOR && minor == MINOR)
                    {
                        var count = archive.ReadInt();
                        for (var i = 0; i < count; i++)
                        {
                            var guid = archive.ReadGuid();
                            var data = new GroupEntity();
                            if (data.ReadArchive(archive))
                            {
                                this.Add(guid, data);
                            }
                        }
                        rc = archive.ReadErrorOccured;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return rc;
        }
    }
}
