/* 
  WZ.NET library
 
  Copyright (c) 2009-2013 koolk

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.

     2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.

     3. This notice may not be removed or altered from any source
     distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Microsoft.DirectX.Direct3D;

namespace WZ.Objects
{
    public class WZCanvas : WZObjects
    {
        [DllImport("SomeC++Stuffs.dll", EntryPoint = "Compress")]
        private static extern int Compress(IntPtr buffer, int bufferLength, IntPtr writeBuffer, int writeBufferLength, bool strongCompress);

        internal static char t = '1';

        public int width, height, size, offset;
        public ImageFormat format;
        public BinaryReader file;

        byte[] bitmapBytes;
        byte[] flippedBitmapBytes;

        Bitmap bitmap;
        Bitmap flippedBitmap;

        Texture texture;
        Texture flippedTexture;

        public enum ImageFormat
        {
            FORMAT_4444 = 1,
            FORMAT_8888 = 2,
            FORMAT_565 = 513,
            FORMAT_BIN = 517
        }
        public WZCanvas()
        {
            type = WZObjectType.WZ_CANVAS;
        }

        public WZCanvas(BinaryReader file, int width, int height, int size, int offset, ImageFormat format)
        {
            type = WZObjectType.WZ_CANVAS;
            this.width = width;
            this.height = height;
            this.size = size;
            this.offset = offset;
            this.format = format;
            this.file = file;
        }

        internal byte[] Decompress(byte[] compressedBuffer, int decompressedSize)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(compressedBuffer, 2, compressedBuffer.Length - 2);
            byte[] buffer = new byte[decompressedSize];
            stream.Position = 0L;
            DeflateStream stream2 = new DeflateStream(stream, CompressionMode.Decompress);
            stream2.Read(buffer, 0, buffer.Length);
            stream2.Close();
            stream2.Dispose();
            stream.Close();
            stream.Dispose();
            return buffer;
        }


        private static byte[] Compress(byte[] decompressedBuffer, bool strong)
        {
            byte[] write = new byte[(int)(decompressedBuffer.Length*1.01 + 12)];
            GCHandle bufferHandle = new GCHandle();
            IntPtr bufferPtr = IntPtr.Zero;
            GCHandle writeHandle = new GCHandle();
            IntPtr writePtr = IntPtr.Zero;
            int length;
            try
            {
                File.GetPointer(decompressedBuffer, 0, ref bufferHandle, ref bufferPtr);
                File.GetPointer(write, 0, ref writeHandle, ref writePtr);
                length = Compress(bufferPtr, decompressedBuffer.Length, writePtr, (int)(decompressedBuffer.Length*1.01 + 12), strong);
            }
            finally
            {
                File.ReleasePointer(bufferHandle);
                File.ReleasePointer(writeHandle);
            }
            byte[] compressedBuffer = new byte[length];
            System.Buffer.BlockCopy(write, 0, compressedBuffer, 0, length);

            return compressedBuffer;
        }

        public static byte[] Compress(byte[] decompressedBuffer)
        {
            return Compress(decompressedBuffer, false);
        }

        public static byte[] StrongCompress(byte[] decompressedBuffer)
        {
            return Compress(decompressedBuffer, true);
        }

        private int round(int width)
        {
            int nwidth = 1;
            while (width > nwidth) nwidth *= 2;
            return nwidth;
        }
        public Texture GetTexture(Device device)
        {
            if (texture != null)
            {
                unsafe
                {
                    if ((int)texture.UnmanagedComPointer == 0x0)
                    {
                        goto createTexture;
                    }
                }
                return texture;
            }

        createTexture:

            Format dFormat = Format.A4R4G4B4;

            int line = width * 2;

            switch (format)
            {
                case ImageFormat.FORMAT_8888: dFormat = Format.A8R8G8B8; line = width * 4; break;
                case ImageFormat.FORMAT_565: dFormat = Format.R5G6B5; break;
                case ImageFormat.FORMAT_BIN: dFormat = Format.Unknown; line = width / 128 ; break;
            }

            if(device.DeviceCaps.TextureCaps.SupportsPower2)
                texture = new Texture(device, Math.Min(round(width), device.DeviceCaps.MaxTextureWidth), Math.Min(round(height), device.DeviceCaps.MaxTextureHeight), 0, Usage.Dynamic, dFormat, Pool.Default);
            else
                texture = new Texture(device, width, height, 0, Usage.Dynamic | Usage.AutoGenerateMipMap, dFormat, Pool.Default);

            if(format == ImageFormat.FORMAT_BIN) return texture; // TODO

            int pitch;
            using (Microsoft.DirectX.GraphicsStream g = texture.LockRectangle(0, LockFlags.Discard, out pitch))
            {
                byte[] buf = GetBitmapBytes();
                for (int i = 0; i < height; i++)
                {
                    int offset = pitch * i;
                    g.Position = offset;
                    g.Write(buf, line * i, line);
                }
            }
            texture.UnlockRectangle(0);             

            //texture = new Texture(device, GetBitmap(), Usage.Dynamic, Pool.Default);

            return texture;
        }


        public Texture GetFlippedTexture(Device device)
        {
            if (flippedTexture != null)
            {
                unsafe
                {
                    if ((int)flippedTexture.UnmanagedComPointer == 0x0)
                    {
                        goto createTexture;
                        //flippedTexture = new Texture(device, GetFlippedBitmap(), Usage.Dynamic, Pool.Default);
                    }
                }
                return flippedTexture;
            }

        createTexture:

            Format dFormat = Format.A4R4G4B4;

            int line = width * 2;

            switch (format)
            {
                case ImageFormat.FORMAT_8888: dFormat = Format.A8R8G8B8; line = width * 4; break;
                case ImageFormat.FORMAT_565: dFormat = Format.R5G6B5; break;
                case ImageFormat.FORMAT_BIN: dFormat = Format.Unknown; line = width / 128; break;
            }

            if (device.DeviceCaps.TextureCaps.SupportsPower2)
                flippedTexture = new Texture(device, Math.Max(round(width), device.DeviceCaps.MaxTextureWidth), Math.Max(round(height), device.DeviceCaps.MaxTextureHeight), 0, Usage.Dynamic, dFormat, Pool.Default);
            else
                flippedTexture = new Texture(device, width, height, 0, Usage.Dynamic | Usage.AutoGenerateMipMap, dFormat, Pool.Default);

            if (format == ImageFormat.FORMAT_BIN) return flippedTexture; // TODO

            int pitch;
            using (Microsoft.DirectX.GraphicsStream g = flippedTexture.LockRectangle(0, LockFlags.Discard, out pitch))
            {
                byte[] buf = GetFlippedBitmapBytes();
                for (int i = 0; i < height; i++)
                {
                    int offset = pitch * i;
                    g.Position = offset;
                    g.Write(buf, line * i, line);
                }
            }
            flippedTexture.UnlockRectangle(0);             

            //flippedTexture = new Texture(device, GetFlippedBitmap(), Usage.Dynamic, Pool.Default);

            return flippedTexture;
        }

        public byte[] GetBitmapBytes()
        {
            if (bitmapBytes != null)
            {
                return bitmapBytes;
            }

            int dSize = size - 1;

            file.BaseStream.Seek(offset + 1, SeekOrigin.Begin);

            int decompressedSize = 0;
            switch (format)
            {
                case ImageFormat.FORMAT_4444:
                case ImageFormat.FORMAT_565: decompressedSize = width * height * 2; break;
                case ImageFormat.FORMAT_8888: decompressedSize = width * height * 4; break;
                case ImageFormat.FORMAT_BIN: decompressedSize = width * height / 128; break;
            }

            byte[] compressedBytes = null;

            ushort zlib_header = file.ReadUInt16();

            file.BaseStream.Seek(-2, SeekOrigin.Current);

            if (zlib_header % 0x100 == 0x78)
            {
                compressedBytes = file.ReadBytes(dSize);
            }
            else
            {
                int end = offset + size;
                while (file.BaseStream.Position < end)
                {
                    int blockSize = file.ReadInt32();
                    byte[] bytes = file.ReadBytes(blockSize);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        bytes[i] ^= File.Key[i];
                    }
                    if (compressedBytes == null)
                    {
                        compressedBytes = bytes;
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream(new byte[compressedBytes.Length + bytes.Length], 0, compressedBytes.Length + bytes.Length, true, true);
                        ms.Write(compressedBytes, 0, compressedBytes.Length);
                        ms.Write(bytes, 0, bytes.Length);
                        compressedBytes = ms.GetBuffer();
                    }
                }
            }

            bitmapBytes = Decompress(compressedBytes, decompressedSize);

            return bitmapBytes;
        }

        public byte[] GetFlippedBitmapBytes()
        {
            if (flippedBitmapBytes != null)
            {
                return flippedBitmapBytes;
            }

            byte[] bytes = GetBitmapBytes();

            int pixel = 2;

            switch (format)
            {
                case ImageFormat.FORMAT_8888: pixel = 4; break;
                case ImageFormat.FORMAT_BIN: return bytes; // TODO
            }

            flippedBitmapBytes = new byte[bytes.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < pixel; i++)
                    {
                        flippedBitmapBytes[(y + 1) * pixel * width - (x + 1) * pixel + i] = bytes[y * pixel * width + x * pixel + i];
                    }
                }
            }

            return flippedBitmapBytes;
        }

        public Bitmap GetBitmap()
        {
            if (this.bitmap != null)
            {
                return this.bitmap;
            }

            Bitmap cache = BitmapCachingService.GetCache(this);
            if (cache != null)
            {
                return cache;
            }
            
            byte[] pixelsData = bitmapBytes;

            if (pixelsData == null)
            {
                int dSize = size - 1;

                file.BaseStream.Seek(offset + 1, SeekOrigin.Begin);

                int decompressedSize = 0;
                switch (format)
                {
                    case ImageFormat.FORMAT_4444:
                    case ImageFormat.FORMAT_565: decompressedSize = width * height * 2; break;
                    case ImageFormat.FORMAT_8888: decompressedSize = width * height * 4; break;
                    case ImageFormat.FORMAT_BIN: decompressedSize = width * height / 128; break;
                }

                byte[] compressedBytes = null;

                ushort zlib_header = file.ReadUInt16();

                file.BaseStream.Seek(-2, SeekOrigin.Current);

                if (zlib_header % 0x100 == 0x78)
                {
                    compressedBytes = file.ReadBytes(dSize);
                }
                else
                {
                    int end = offset + size;
                    while (file.BaseStream.Position < end)
                    {
                        int blockSize = file.ReadInt32();
                        byte[] bytes = file.ReadBytes(blockSize);
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            bytes[i] ^= File.Key[i];
                        }
                        if (compressedBytes == null)
                        {
                            compressedBytes = bytes;
                        }
                        else
                        {
                            MemoryStream ms = new MemoryStream(new byte[compressedBytes.Length + bytes.Length], 0, compressedBytes.Length + bytes.Length, true, true);
                            ms.Write(compressedBytes, 0, compressedBytes.Length);
                            ms.Write(bytes, 0, bytes.Length);
                            compressedBytes = ms.GetBuffer();
                        }
                    }
                }

                pixelsData = Decompress(compressedBytes, decompressedSize);
            }
            
            if(format == ImageFormat.FORMAT_4444){
                // TO FORMAT_4444
                byte[] newPixelsData = new byte[pixelsData.Length * 2];
                for(int i = 0; i < pixelsData.Length; i++)
		        {
			        byte low	= (byte)(pixelsData[i] & 0x0F);
			        byte high	=(byte)( pixelsData[i] & 0xF0);

			        newPixelsData[(i << 1)] = (byte)((low << 4) | low);
			        newPixelsData[(i << 1) + 1] = (byte)(high | (high >> 4));
	        	}
                pixelsData = newPixelsData;
            }

            PixelFormat pFormat = PixelFormat.Format32bppArgb;
            
            switch(format){
		        case ImageFormat.FORMAT_565: pFormat = PixelFormat.Format16bppRgb565; break;
		        case ImageFormat.FORMAT_BIN: pFormat = PixelFormat.Format1bppIndexed; break;
            }

            Bitmap bitmap; // do not store

            unsafe
            {
                bitmap = new Bitmap(width, height, pFormat);

                BitmapData bmp_data;

                bmp_data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pFormat);

                byte* bmpPtr = (byte*)(bmp_data.Scan0);

                for (int i = 0; i < pixelsData.Length; i++)
                {
                    bmpPtr[i] = pixelsData[i];
                }

                bitmap.UnlockBits(bmp_data);
            }
            
            return BitmapCachingService.Cache(this, bitmap);
        }

        public Bitmap GetFlippedBitmap()
        {
            if (flippedBitmap != null)
            {
                return flippedBitmap;
            }

            flippedBitmap = (Bitmap)GetBitmap().Clone();
            flippedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return flippedBitmap;
        }

        public void SetBitmap(Bitmap image)
        {
            if (bitmap != null)
                bitmap.Dispose();
            if (flippedBitmap != null)
            {
                flippedBitmap.Dispose();
                flippedBitmap = null;
            }
            if(texture != null)
            {
                texture.Dispose();
                texture = null;
            }
            if(flippedTexture != null)
            {
                flippedTexture.Dispose();
                flippedTexture = null;
            }
            bitmap = image;
            width = image.Width;
            height = image.Height;
        }

        public override void Write(BinaryWriter file)
        {
            WriteString(file, "Canvas", 0x73, 0x1B);
            file.Write((byte)0);
            file.Write((objects.Count > 0));

            if (objects.Count > 0)
            {
                file.Write((short)0);
                WriteObjects(file);
            }

            GetBitmap();

            WritePackedInt(file, width);
            WritePackedInt(file, height);

            ImageFormat format = this.format;

            // TODO: FORMAT_565 and FORMAT_BIN

            switch(format)
            {
                case ImageFormat.FORMAT_565: format = ImageFormat.FORMAT_8888; break;
                case ImageFormat.FORMAT_BIN: format = ImageFormat.FORMAT_4444; break;                   
            }

            switch (format)
            {
                case ImageFormat.FORMAT_4444: WritePackedInt(file, 1); break;
                case ImageFormat.FORMAT_8888: WritePackedInt(file, 2); break;
            }

            WritePackedInt(file, 0);

            file.Write((int)0);

            byte[] pixels = new byte[height * width * 4];

            for(int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Color pixel = bitmap.GetPixel(j, i);
                    pixels[i * width * 4 + j * 4] = pixel.B;
                    pixels[i * width * 4 + j * 4 + 1] = pixel.G;
                    pixels[i * width * 4 + j * 4 + 2] = pixel.R;
                    pixels[i * width * 4 + j * 4 + 3] = pixel.A;
                }
            }

            // compress to 4444
            if (format == ImageFormat.FORMAT_4444)
            {
                byte[] nPixels = new byte[height * width * 2];
                for (int i = 0; i < height * width * 4; i++)
                {
                    byte pPix = pixels[i];
                    byte nPix1 = (byte)(((pPix >> 4) << 4) + (pPix >> 4));
                    byte nPix2 = (byte)((((pPix >> 4) + 1) << 4) + ((pPix >> 4) + 1));
                    if (Math.Abs(nPix1 - pPix) < Math.Abs(nPix2 - pPix)) pPix = nPix1;
                    else pPix = nPix2;
                    pPix >>= 4;
                    if (i % 2 == 1) nPixels[i >> 1] += (byte)(pPix << 4);
                    else nPixels[i >> 1] = pPix;
                }
                pixels = nPixels;
            }
            byte[] compressed = Compress(pixels);

            file.Write((int)(compressed.Length + 1));

            // write compressed data
            file.Write((byte)0);

            file.Write(compressed);

            bitmap.Dispose();
            bitmap = null;
        }

        public override object Clone()
        {
            WZCanvas c = new WZCanvas(file, width, height, size, offset, format);

            foreach (WZObject obj in objects)
            {
                c.objects.Add(obj.Clone() as WZObject);
            }

            return c;
        }

        public override void Dispose()
        {
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }
            if (flippedBitmap != null)
            {
                flippedBitmap.Dispose();
                flippedBitmap = null;
            }
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }
            if (flippedTexture != null)
            {
                flippedTexture.Dispose();
                flippedTexture = null;
            }
            if (bitmapBytes != null)
            {
                bitmapBytes = null;
            }
        }
    }
}
