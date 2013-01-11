using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace WZ
{
    /// <summary>
    /// 
    /// </summary>
    public class Crc32
    {
        protected static Hashtable cachedCRC32Tables;
        protected static bool autoCache;

        protected uint[] crc32Table;
        private uint m_crc;

        /// <summary>
        /// Returns the default polynomial (used in WinZip, Ethernet, etc)
        /// </summary>
        public static uint DefaultPolynomial
        {
            get { return 0x04C11DB7; }
        }

        /// <summary>
        /// Gets or sets the auto-cache setting of this class.
        /// </summary>
        public static bool AutoCache
        {
            get { return autoCache; }
            set { autoCache = value; }
        }

        /// <summary>
        /// Initialize the cache
        /// </summary>
        static Crc32()
        {
            cachedCRC32Tables = Hashtable.Synchronized(new Hashtable());
            autoCache = true;
        }

        public static void ClearCache()
        {
            cachedCRC32Tables.Clear();
        }


        /// <summary>
        /// Builds a crc32 table given a polynomial
        /// </summary>
        /// <param name="ulPolynomial"></param>
        /// <returns></returns>
        protected static uint[] BuildCRC32Table(uint ulPolynomial)
        {
            uint[] table = new uint[256];

            // 256 values representing ASCII character codes. 
            for (uint dividend = 0; dividend < 256; dividend++)
            {
                uint remain = dividend << 24;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((remain & 0x80000000) != 0)
                    {
                        remain = (remain << 1) ^ ulPolynomial;
                    }
                    else
                    {
                        remain <<= 1;
                    }
                }
                table[dividend] = remain;
            }

            return table;
        }


        /// <summary>
        /// Creates a CRC32 object using the DefaultPolynomial
        /// </summary>
        public Crc32()
            : this(DefaultPolynomial)
        {
        }

        /// <summary>
        /// Creates a CRC32 object using the specified Creates a CRC32 object 
        /// </summary>
        public Crc32(uint aPolynomial)
            : this(aPolynomial, Crc32.AutoCache)
        {
        }

        /// <summary>
        /// Construct the 
        /// </summary>
        public Crc32(uint aPolynomial, bool cacheTable)
        {
            crc32Table = (uint[])cachedCRC32Tables[aPolynomial];
            if (crc32Table == null)
            {
                crc32Table = Crc32.BuildCRC32Table(aPolynomial);
                if (cacheTable)
                    cachedCRC32Tables.Add(aPolynomial, crc32Table);
            }
        }

        /// <summary>
        /// Initializes an implementation of HashAlgorithm.
        /// </summary>
        public void Initialize()
        {
            m_crc = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        protected void HashCore(byte[] buffer, int offset, int count)
        {
            // Save the text in the buffer. 
            for (int i = offset; i < count; i++)
            {
                ulong tabPtr = (m_crc >> 0x18) ^ buffer[i];
                m_crc = (m_crc << 0x08) ^ crc32Table[tabPtr];
            }
        }
        protected uint HashFinal()
        {
            return m_crc;
        }

        public uint ComputeHash(Stream inputStream)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, 4096)) > 0)
            {
                HashCore(buffer, 0, bytesRead);
            }
            return HashFinal();
        }

        public uint Compute(Stream stream)
        {
            Initialize();
            return ComputeHash(stream);
        }



        /// <summary>
        /// Overloaded. Computes the hash value for the input data.
        /// </summary>
        public uint Compute(byte[] buffer)
        {
            return ComputeHash(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Overloaded. Computes the hash value for the input data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public uint ComputeHash(byte[] buffer, int offset, int count)
        {
            Initialize();
            HashCore(buffer, offset, count);
            return HashFinal();
        }

    }

}
