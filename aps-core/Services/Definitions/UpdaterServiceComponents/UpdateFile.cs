using System;
using System.IO;
using PT.APSCommon;
using PT.Common;

namespace PT.UpdaterServiceComponents
{
    /// <summary>
    /// Summary description for UpdateFile.
    /// </summary>
    public class UpdateFile
    {
        public UpdateFile(string a_relativePath, long a_modifiedDate)
        {
            m_relativePath = a_relativePath;
            m_modifiedDate = a_modifiedDate;
            m_compressed = null;
        }

        public UpdateFile(string aRelativePath)
        {
            m_relativePath = aRelativePath;
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), aRelativePath);

            if (!File.Exists(fullPath))
            {
                throw new PTException("2579", new object[] { fullPath });
            }

            try
            {
                FileInfo fileInfo = new FileInfo(fullPath);
                if (fileInfo.IsReadOnly)
                {
                    System.IO.File.SetAttributes(fullPath, FileAttributes.Normal);
                }
            }
            catch (Exception e)
            {
                throw new PTException("2580", e, new object[] { fullPath });
            }

            try
            {
                m_compressed = PT.Common.Compression.Optimal.Compress(System.IO.File.ReadAllBytes(fullPath));
            }
            catch (Exception e)
            {
                throw new PTException("2581", e, new object[] { fullPath });
            }

            m_modifiedDate = File.GetLastWriteTimeUtc(fullPath).Ticks;
        }

        public string m_relativePath;
        public long m_modifiedDate;
        public byte[] m_compressed;
    }
}
