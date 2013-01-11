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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;
public static class ResourceExtractor
{
    [DllImport("kernel32")]
    public extern static int LoadLibrary(string lpLibFileName);
    [DllImport("kernel32")]
    public extern static bool FreeLibrary(int hLibModule);

    static Hashtable hs = new Hashtable();

    public static void ExtractResource(string resourceName, string filename, byte version)
    {
        if (!System.IO.File.Exists(filename))
        {
            if (!System.IO.File.Exists(Path.GetTempPath() + filename) || !CheckDllVersion(filename, version))
                using (System.IO.Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                using (System.IO.FileStream fs = new System.IO.FileStream(Path.GetTempPath() + filename, System.IO.FileMode.Create))
                {
                    byte[] b = new byte[s.Length];
                    s.Read(b, 0, b.Length);
                    fs.Write(b, 0, b.Length);
                    fs.Close();
                    using (System.IO.FileStream fs2 = new System.IO.FileStream(Path.GetTempPath() + filename + ".txt", System.IO.FileMode.Create))
                    {
                        fs2.WriteByte(version);
                        fs2.Close();
                    }
                }
            

            int h = LoadLibrary(Path.GetTempPath() + filename);

            hs[filename] = h;

            Debug.Assert(h != 0, "Unable to load library " + filename);
        }
    }

    public static string ExtractResource(string resourceName, string filename)
    {
        using (System.IO.Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        using (System.IO.FileStream fs = new System.IO.FileStream(Path.GetTempPath() + filename, System.IO.FileMode.Create))
        {
            byte[] b = new byte[s.Length];
            s.Read(b, 0, b.Length);
            fs.Write(b, 0, b.Length);
            fs.Close();
        }

        return Path.GetTempPath() + filename;
    }
    public static bool CheckDllVersion(string filename, byte version)
    {
        if (!System.IO.File.Exists(Path.GetTempPath() + filename + ".txt")) return false;
        using (System.IO.FileStream fs = new System.IO.FileStream(Path.GetTempPath() + filename + ".txt", System.IO.FileMode.Open))
        {
            bool result = fs.ReadByte() == version;
            fs.Close();
            return result;
        }

    }

    public static void DeleteResource(string filename)
    {
        Debug.Assert(FreeLibrary((int)hs[filename]), "Unable to free library");

        //System.IO.File.Delete(Path.GetTempPath() + filename);
    }
}