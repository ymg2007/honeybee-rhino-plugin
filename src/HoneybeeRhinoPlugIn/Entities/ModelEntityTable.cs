using Rhino.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneybeeRhino.Entities
{
    public class ModelEntityTable : Dictionary<string, ModelEntity>
    {
        /// <summary>
        /// Class major and minor verson numbers
        /// </summary>
        private const int MAJOR = 1;
        private const int MINOR = 0;

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
                    archive.WriteInt(Count);
                    for (var i = 0; i < Count; i++)
                    {
                        var item = this.ElementAt(i);
                        archive.WriteString(item.Key);
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
                            var guid = archive.ReadString();
                            var data = new ModelEntity();
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
