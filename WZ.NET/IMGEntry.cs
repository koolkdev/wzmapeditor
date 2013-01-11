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
using System.Collections;
using WZ.Objects;

namespace WZ
{
    public class IMGEntry : WZWriteable, ICloneable, IDisposable
    {
        private EntryList _childs = new EntryList();

        public EntryList childs 
        {
            get 
            {
                if (!loaded)
                    Open();
                return _childs;
            }
        }

        public WZObject value;

        public IMGEntry parent;

        public string Name;

        public bool ToSave = false;

        protected bool loaded = true;

        public IMGEntry() { }

        public IMGEntry(IMGEntry parent)
        {
            this.parent = parent;
        }

        public virtual IMGFile Open() { return null; }

        public IMGEntry(string Name)
            : this(null, Name, null)
        {
        }

        public IMGEntry(string Name, WZObject value)
            : this(null, Name, value)
        {
        }

        public IMGEntry(IMGEntry parent, string Name, WZObject value)
        {
            this.parent = parent;
            this.Name = Name;
            this.value = value;
        }

        public IMGEntry(WZProperty p)
        {
            CreateEntry(this, p);
        }

        public void Dispose()
        {
            if (value != null)
            {
                value.Dispose();
                value = null;
            }
            foreach (IMGEntry entry in childs.Values)
            {
                entry.Dispose();
            }
            childs.Clear();
        }

        public void Add(IMGEntry child){
            childs.Add(child.Name, child);
            child.parent = this;
        }

        public void Rename(string name)
        {
            if (parent != null)
            {
                if (parent.childs.Contains(name))
                {
                    return;
                }
                parent.childs.Remove(Name);
                parent.childs.Add(name, this);
            }
            Name = name;
        }

        public object Clone()
        {
            IMGEntry entry = new IMGEntry(Name, (value == null) ? null : value.Clone() as WZObject);

            foreach (IMGEntry subentry in childs.Values)
            {
                entry.Add(subentry.Clone() as IMGEntry);
            }

            return entry;            
        }

        public IMGEntry GetChild(string name)
        {
            int find = name.IndexOf("/");
            if (find != -1)
            {
                string tof = name.Substring(0, find);
                if (childs.ContainsKey(tof))
                {
                    return ((IMGEntry)childs[tof]).GetChild(name.Substring(find + 1));
                }
                else if (tof == "..")
                {
                    if (parent != null)
                    {
                        return parent.GetChild(name.Substring(3));
                    }
                }
            }
            else if (name == "..")
            {
                return parent;
            }
            else if (childs.ContainsKey(name))
            {
                return (IMGEntry)childs[name];
            }
            return null;
        }

        public short GetShort()
        {
            if (value == null) return 0;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: return (short)((WZShort)value).value;
                case WZObject.WZObjectType.WZ_INTEGER: return (short)((WZInteger)value).value;
                case WZObject.WZObjectType.WZ_FLOAT: return (short)((WZFloat)value).value;
                case WZObject.WZObjectType.WZ_DOUBLE: return (short)((WZDouble)value).value;
            }
            return 0;
        }

        public int GetInt()
        {
            if (value == null) return 0;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: return (int)((WZShort)value).value;
                case WZObject.WZObjectType.WZ_INTEGER: return (int)((WZInteger)value).value;
                case WZObject.WZObjectType.WZ_FLOAT: return (int)((WZFloat)value).value;
                case WZObject.WZObjectType.WZ_DOUBLE: return (int)((WZDouble)value).value;
            }
            return 0;
        }

        public double GetDouble()
        {
            if (value == null) return 0;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: return (double)((WZShort)value).value;
                case WZObject.WZObjectType.WZ_INTEGER: return (double)((WZInteger)value).value;
                case WZObject.WZObjectType.WZ_FLOAT: return (double)((WZFloat)value).value;
                case WZObject.WZObjectType.WZ_DOUBLE: return (double)((WZDouble)value).value;
            }
            return 0;
        }

        public float GetFloat()
        {
            if (value == null) return 0;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: return (float)((WZShort)value).value;
                case WZObject.WZObjectType.WZ_INTEGER: return (float)((WZInteger)value).value;
                case WZObject.WZObjectType.WZ_FLOAT: return (float)((WZFloat)value).value;
                case WZObject.WZObjectType.WZ_DOUBLE: return (float)((WZDouble)value).value;
            }
            return 0;
        }

        public string GetString()
        {
            if (value == null) return "";
            if (value.type == WZObject.WZObjectType.WZ_STRING) return ((WZString)value).value;
            return "";
        }

        public WZVector GetVector()
        {
            if (value == null) return null;
            if (value.type == WZObject.WZObjectType.WZ_VECTOR) return (WZVector)value;
            return null;
        }

        public WZCanvas GetCanvas()
        {
            if (value == null) return null;
            if (value.type == WZObject.WZObjectType.WZ_CANVAS) return (WZCanvas)value;
            return null;
        }

        public bool GetBool()
        {
            if (value == null) return false;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: return ((WZShort)value).value != 0;
                case WZObject.WZObjectType.WZ_INTEGER: return ((WZInteger)value).value != 0;
            }
            return false;
        }

        // TODO Sound

        public short GetShort(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return 0;
            return e.GetShort();
        }

        public int GetInt(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return 0;
            return e.GetInt();
        }

        public double GetDouble(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return 0;
            return e.GetDouble();
        }

        public float GetFloat(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return 0;
            return e.GetFloat();
        }

        public string GetString(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return "";
            return e.GetString();
        }

        public WZVector GetVector(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return null;
            return e.GetVector();
        }

        public WZCanvas GetCanvas(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return null;
            return e.GetCanvas();
        }

        public bool GetBool(string name)
        {
            IMGEntry e = GetChild(name);
            if (e == null) return false;
            return e.GetBool();
        }

        public void SetShort(short set)
        {
            if (value == null) return;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: ((WZShort)value).value = (short)set; break;
                case WZObject.WZObjectType.WZ_INTEGER: ((WZInteger)value).value = (int)set; break;
                case WZObject.WZObjectType.WZ_FLOAT: ((WZFloat)value).value = (float)set; break;
                case WZObject.WZObjectType.WZ_DOUBLE: ((WZDouble)value).value = (double)set; break;
            }
        }

        public void SetInt(int set)
        {
            if (value == null) return;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: ((WZShort)value).value = (short)set; break;
                case WZObject.WZObjectType.WZ_INTEGER: ((WZInteger)value).value = (int)set; break;
                case WZObject.WZObjectType.WZ_FLOAT: ((WZFloat)value).value = (float)set; break;
                case WZObject.WZObjectType.WZ_DOUBLE: ((WZDouble)value).value = (double)set; break;
            }
        }

        public void SetDouble(double set)
        {
            if (value == null) return;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: ((WZShort)value).value = (short)set; break;
                case WZObject.WZObjectType.WZ_INTEGER: ((WZInteger)value).value = (int)set; break;
                case WZObject.WZObjectType.WZ_FLOAT: ((WZFloat)value).value = (float)set; break;
                case WZObject.WZObjectType.WZ_DOUBLE: ((WZDouble)value).value = (double)set; break;
            }
        }

        public void SetFloat(float set)
        {
            if (value == null) return;
            switch (value.type)
            {
                case WZObject.WZObjectType.WZ_SHORT: ((WZShort)value).value = (short)set; break;
                case WZObject.WZObjectType.WZ_INTEGER: ((WZInteger)value).value = (int)set; break;
                case WZObject.WZObjectType.WZ_FLOAT: ((WZFloat)value).value = (float)set; break;
                case WZObject.WZObjectType.WZ_DOUBLE: ((WZDouble)value).value = (double)set; break;
            }
        }

        public void SetString(string set)
        {
            if (value == null) return;
            if (value.type == WZObject.WZObjectType.WZ_STRING) ((WZString)value).value = set;
        }

        public void SetVector(WZVector set)
        {
            if (value == null)
            {
                value = set;
            }
            if (value.type == WZObject.WZObjectType.WZ_VECTOR)
            {
                value = set;
            }
        }

        public void SetCanvas(WZCanvas set)
        {
            if (value == null)
            {
                value = set;
            }
            if (value.type == WZObject.WZObjectType.WZ_CANVAS)
            {
                value = set;
            }
        }


        public void SetShort(string name, short set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {

                e = new IMGEntry(this);
                e.Name = name;
                e.value = new WZShort(set);
                Add(e);
            }
            else if (e != null)
            {
                e.SetShort(set);
            }
        }

        public void SetInt(string name, int set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {

                e = new IMGEntry(this);
                e.Name = name;
                e.value = new WZInteger(set);
                Add(e);
            }
            else if (e != null)
            {
                e.SetInt(set);
            }
        }

        public void SetDouble(string name, double set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {

                e = new IMGEntry(this);
                e.Name = name;
                e.value = new WZDouble(set);
                Add(e);
            }
            else if (e != null)
            {
                e.SetDouble(set);
            }
        }

        public void SetFloat(string name, float set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {

                e = new IMGEntry(this);
                e.Name = name;
                e.value = new WZFloat(set);
                Add(e);
            }
            else if (e != null)
            {
                e.SetFloat(set);
            }
        }

        public void SetString(string name, string set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {

                e = new IMGEntry(this);
                e.Name = name;
                e.value = new WZString(set);
                Add(e);
            }
            else if (e != null)
            {
                e.SetString(set);
            }
        }

        public void SetVector(string name, WZVector set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {
                e = new IMGEntry(this);
                e.Name = name;
                e.value = set;
                Add(e);
            }
            else if (e != null)
            {
                e.SetVector(set);
            }
        }

        public void SetCanvas(string name, WZCanvas set)
        {
            IMGEntry e = GetChild(name);
            if (e == null && name.IndexOf("/") == -1)
            {
                e = new IMGEntry(this);
                e.Name = name;
                e.value = set;
                Add(e);
            }
            else if (e != null)
            {
                e.SetCanvas(set);
            }
        }


        public static void CreateEntry(IMGEntry parent, WZObject obj)
        {
            switch (obj.type)
            {
                case WZObject.WZObjectType.WZ_EMPTY: CreateEntry(parent, (WZEmpty)obj); break;
                case WZObject.WZObjectType.WZ_SHORT: CreateEntry(parent, (WZShort)obj); break;
                case WZObject.WZObjectType.WZ_INTEGER: CreateEntry(parent, (WZInteger)obj); break;
                case WZObject.WZObjectType.WZ_FLOAT: CreateEntry(parent, (WZFloat)obj); break;
                case WZObject.WZObjectType.WZ_DOUBLE: CreateEntry(parent, (WZDouble)obj); break;
                case WZObject.WZObjectType.WZ_STRING: CreateEntry(parent, (WZString)obj); break;
                case WZObject.WZObjectType.WZ_COMPLEX: CreateEntry(parent, (WZComplex)obj); break;
                case WZObject.WZObjectType.WZ_PROPERTY: CreateEntry(parent, (WZProperty)obj); break;
                case WZObject.WZObjectType.WZ_CONVEX: CreateEntry(parent, (WZConvex)obj); break;
                case WZObject.WZObjectType.WZ_VECTOR: CreateEntry(parent, (WZVector)obj); break;
                case WZObject.WZObjectType.WZ_CANVAS: CreateEntry(parent, (WZCanvas)obj); break;
                case WZObject.WZObjectType.WZ_UOL: CreateEntry(parent, (WZUOL)obj); break;
                case WZObject.WZObjectType.WZ_SOUND: CreateEntry(parent, (WZSound)obj); break;

            }
        }

        private static void CreateEntryChilds(IMGEntry parent, WZObjects obj)
        {
            foreach (WZObject o in obj.objects)
            {
                IMGEntry e = new IMGEntry(parent);
                CreateEntry(e, o);
                parent.Add(e);
            }
        }

        private static void CreateEntry(IMGEntry parent, WZEmpty obj)
        {
            parent.Name = obj.name;
            parent.value = new WZEmpty();
        }

        private static void CreateEntry(IMGEntry parent, WZShort obj)
        {
            parent.Name = obj.name;
            parent.value = new WZShort(obj.value);
        }

        private static void CreateEntry(IMGEntry parent, WZInteger obj)
        {
            parent.Name = obj.name;
            parent.value = new WZInteger(obj.value);
        }

        private static void CreateEntry(IMGEntry parent, WZFloat obj)
        {
            parent.Name = obj.name;
            parent.value = new WZFloat(obj.value);
        }

        private static void CreateEntry(IMGEntry parent, WZDouble obj)
        {
            parent.Name = obj.name;
            parent.value = new WZDouble(obj.value);
        }

        private static void CreateEntry(IMGEntry parent, WZString obj)
        {
            parent.Name = obj.name;
            parent.value = new WZString(obj.value);
        }

        private static void CreateEntry(IMGEntry parent, WZComplex obj)
        {
            parent.Name = obj.name;
            CreateEntry(parent, obj.value);
        }

        private static void CreateEntry(IMGEntry parent, WZProperty obj)
        {
            CreateEntryChilds(parent, obj);
        }

        private static void CreateEntry(IMGEntry parent, WZConvex obj)
        {
            int i = 0;
            foreach(WZObject o in obj.objects)
            {
                IMGEntry e = new IMGEntry(parent);
                CreateEntry(e, o);
                e.Name = "Convex" + (i++).ToString();
                parent.Add(e);
            }
        }

        private static void CreateEntry(IMGEntry parent, WZVector obj)
        {
            parent.value = new WZVector(obj.x, obj.y);
        }

        private static void CreateEntry(IMGEntry parent, WZCanvas obj)
        {
            parent.value = new WZCanvas(obj.file, obj.width, obj.height, obj.size, obj.offset, obj.format);
            CreateEntryChilds(parent, obj);
        }

        private static void CreateEntry(IMGEntry parent, WZUOL obj)
        {
            parent.value = new WZUOL(obj.path);
        }

        private static void CreateEntry(IMGEntry parent, WZSound obj)
        {
            parent.value = new WZSound(obj.file, obj.offset, obj.size, obj.bytesSize, obj.bytesSize);
        }

        public override void Write(System.IO.BinaryWriter file)
        {
            throw new NotImplementedException();
        }
    }
}
