/* 
  koolk's Map Editor
 
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
using WZ;

namespace WZMapEditor
{
    class GarbageCollector
    {
        static Thread thread;
        static Thread MainThread;
        public static List<IMGFile> imgs = new List<IMGFile>();

        public static void StartGC()
        {
            if (thread != null)
            {
#pragma warning disable
                thread.Resume();
#pragma warning restore
            }
            else
            {
                thread = new Thread(new ThreadStart(CleanTimer));
                thread.IsBackground = true;
                MainThread = Thread.CurrentThread;
                thread.Start();
            }
        }

        public static void StopGC()
        {
#pragma warning disable
            thread.Suspend();
#pragma warning restore 
        }


        private static void CleanTimer()
        {
            while (true)
            {
                Clean();
                Thread.Sleep(1000);
            }

        }

        private static void CreateUsedIMGSList()
        {
            imgs.Clear();
            imgs.Add(MapEditor.file.Directory.GetIMG("MapHelper.img"));
            if (MapEditor.Instance.MapID != null)
            {
                if(MapEditor.Instance.MapID == "MapLogin.img")
                    imgs.Add(MapEditor.ui.Directory.GetIMG("MapLogin.img"));
                else
                    imgs.Add(MapEditor.file.Directory.GetIMG("Map/Map" + MapEditor.Instance.MapID[0] + "/" + MapEditor.Instance.MapID + ".img"));
            }
            if(Map.Instance != null)
            {
                if(Map.Instance.clock != null)
                {
                    imgs.Add(MapEditor.file.Directory.GetIMG("Obj/etc.img"));
                }
                foreach (MapBack back in Map.Instance.backs)
                {
                    IMGFile img = back.Image.parent.parent as IMGFile;
                    if (!imgs.Contains(img))
                    {
                        imgs.Add(img);
                    }
                }
                foreach (MapLife life in Map.Instance.lifes)
                {
                    IMGFile img = life.Image.parent.parent as IMGFile;
                    if (!imgs.Contains(img))
                    {
                        imgs.Add(img);
                    }
                }
                foreach (MapReactor reactor in Map.Instance.reactors)
                {
                    IMGFile img = reactor.Image.parent.parent as IMGFile;
                    if (!imgs.Contains(img))
                    {
                        imgs.Add(img);
                    }
                }
                foreach (MapToolTip tt in Map.Instance.tooltips)
                {
                    IMGFile img = tt.Image.parent.parent.parent as IMGFile;
                    if (!imgs.Contains(img))
                    {
                        imgs.Add(img);
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    foreach (MapObject obj in Map.Instance.layers[i].objects)
                    {
                        IMGFile img = obj.Image.parent.parent.parent.parent as IMGFile;
                        if (!imgs.Contains(img))
                        {
                            imgs.Add(img);
                        }
                    }
                    string tileStyle = Map.Instance.layers[i].info.GetString("tS");
                    if (tileStyle != "")
                    {
                        IMGFile img = MapEditor.file.Directory.GetIMG("Tile/" + tileStyle + ".img");
                        if (!imgs.Contains(img))
                        {
                            imgs.Add(img);
                        }

                    }
                }
            }
            if(MapEditor.Instance.EditMode.Checked && MapEditor.Instance.EditObj.Checked)
            {
                if(MapEditor.Instance.SelectedIMG != null)
                    imgs.Add(MapEditor.Instance.SelectedIMG);
            }
            if (MapBackground.Object != null)
            {
                imgs.Add(MapBackground.Object.parent as IMGFile);
            }
        }
        private static void Clean()
        {
            if (MapEditor.file != null)
            {
                lock(MapEditor.MapLock) lock(MapEditor.NpcLock) lock(MapEditor.MobLock) lock(MapEditor.StringLock) CreateUsedIMGSList();
                Clean(MapEditor.file.Directory, MapEditor.MapLock);
                Clean(MapEditor.npc.Directory, MapEditor.NpcLock);
                Clean(MapEditor.mob.Directory, MapEditor.MobLock);
                //lock (MapEditor.StringLock) Clean(MapEditor.stringf.Directory);
                Clean(MapEditor.sound.Directory, MapEditor.SoundLock);
                Clean(MapEditor.ui.Directory, MapEditor.UILock);
                Clean(MapEditor.reactor.Directory, MapEditor.ReactorLock);
            }
        }

        private static void Clean(WZDirectory directory, object locker)
        {
            foreach(IMGFile img in directory.IMGs.Values)
            {
                lock (img)
                {
                    if (img.IsLoaded())
                    {
                        if (!img.ToSave && !imgs.Contains(img))
                        {
                            lock(locker)
                                img.Close();
                        }
                    }
                }
            }
            foreach (WZDirectory dir in directory.Directories.Values)
            {
                Clean(dir, locker);
            }
        }
    }
}
