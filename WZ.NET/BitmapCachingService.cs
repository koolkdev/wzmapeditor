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
using System.Text;
using System.Threading;
using System.Collections;
using System.Drawing;

namespace WZ
{
    public class BitmapCachingService
    {

        private static Thread thread;

        private static Hashtable cache = new Hashtable();
        private static Hashtable fcache = new Hashtable();

        public static Object Locker = new Object();

        public static void Start()
        {
            if (thread == null)
            {
                thread = new Thread(new ThreadStart(CleanTimer));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public static void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        private static void CleanTimer()
        {
            while (true)
            {
                Clean();
                Thread.Sleep(1000);
            }
        }
        public static Bitmap Cache(Objects.WZCanvas canvas, Bitmap bitmap)
        {
            Start();
            lock (Locker)
            {
                if (cache.Contains(canvas))
                    return ((CachedBitmap)cache[canvas]).bitmap;
                cache.Add(canvas, new CachedBitmap(bitmap));
            }
            return bitmap;
        }

        public static Bitmap GetCache(Objects.WZCanvas canvas)
        {
            Start();
            lock (Locker)
            {
                if (cache.Contains(canvas))
                    return ((CachedBitmap)cache[canvas]).bitmap;
            }
            return null;
        }
        public static Bitmap FCache(Objects.WZCanvas canvas, Bitmap bitmap)
        {
            Start();
            lock (Locker)
            {
                if (fcache.Contains(canvas))
                    return ((CachedBitmap)fcache[canvas]).bitmap;
                fcache.Add(canvas, new CachedBitmap(bitmap));
            }
            return bitmap;
        }

        public static Bitmap GetFCache(Objects.WZCanvas canvas)
        {
            Start();
            lock (Locker)
            {
                if (fcache.Contains(canvas))
                    return ((CachedBitmap)fcache[canvas]).bitmap;
            }
            return null;
        }

        private static void Clean()
        {
            long time = DateTime.Now.Ticks / 10000;

            List<Objects.WZCanvas> remove = new List<Objects.WZCanvas>();
            foreach (DictionaryEntry cached in cache)
            {
                CachedBitmap bCache = (CachedBitmap)cached.Value;

                long ctime;
                lock (Locker)
                    ctime = bCache.time;

                if (time - ctime > 5) remove.Add((Objects.WZCanvas)cached.Key);
            }

            foreach (Objects.WZCanvas obj in remove)
            {
                lock (Locker)
                    cache.Remove(obj);
            }

            remove.Clear();

            foreach (DictionaryEntry cached in fcache)
            {
                CachedBitmap bCache = (CachedBitmap)cached.Value;

                long ctime;
                lock (Locker)
                    ctime = bCache.time;

                if (time - ctime > 5) remove.Add((Objects.WZCanvas)cached.Key);
            }

            foreach (Objects.WZCanvas obj in remove)
            {
                lock (Locker)
                    fcache.Remove(obj);
            }
        }
    }
}
