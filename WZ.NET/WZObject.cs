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
using System.IO;
using System.Collections;
using WZ.Objects;

namespace WZ
{
    public abstract class WZObject : WZWriteable, ICloneable, IDisposable
    {
        public WZObjectType type;
        public enum WZObjectType
        {
            WZ_EMPTY = 0x1, // 0x00
            WZ_SHORT = 0x2, // 0x02
            WZ_INTEGER = 0x4, // 0x03
            WZ_FLOAT = 0x8, // 0x04
            WZ_DOUBLE = 0x10, // 0x05
            WZ_STRING = 0x20, // 0x08
            WZ_COMPLEX = 0x40, // 0x09
            WZ_PROPERTY = 0x80, // Property
            WZ_CONVEX = 0x100, // Shape2D#Convex2D
            WZ_VECTOR = 0x200, // Shape2D#Vector2D
            WZ_CANVAS = 0x400, // Canvas
            WZ_UOL = 0x800, // UOL
            WZ_SOUND = 0x1000, // Sound_DX8
            WZ_ENTRY = 0x2000, // for Entries
            //
            WZ_SIMPLE_OBJECT = (WZ_EMPTY | WZ_SHORT | WZ_INTEGER | WZ_FLOAT | WZ_DOUBLE | WZ_STRING),
            WZ_OBJECTS = (WZ_PROPERTY | WZ_CONVEX | WZ_CANVAS),
            WZ_NUMBERIC_VALUE = (WZ_SHORT | WZ_INTEGER | WZ_FLOAT | WZ_DOUBLE)
        }

        public static WZProperty FromEntry(IMGEntry entry)
        {
            return PropertyFromEntry(entry);
        }

        private static WZObject ObjectFromEntry(IMGEntry entry)
        {
            if (entry.value == null)
            {
                return new WZComplex(entry.Name, ComplexFromEntry(entry));
            }
            else
            {
                switch (entry.value.type)
                {
                    case WZObjectType.WZ_EMPTY: return EmptyFromEntry(entry);
                    case WZObjectType.WZ_SHORT: return ShortFromEntry(entry);
                    case WZObjectType.WZ_INTEGER: return IntegerFromEntry(entry);
                    case WZObjectType.WZ_FLOAT: return FloatFromEntry(entry);
                    case WZObjectType.WZ_DOUBLE: return DoubleFromEntry(entry);
                    case WZObjectType.WZ_STRING: return StringFromEntry(entry);
                    case WZObjectType.WZ_VECTOR:
                    case WZObjectType.WZ_CANVAS: 
                    case WZObjectType.WZ_SOUND:
                    case WZObjectType.WZ_UOL: return new WZComplex(entry.Name, ComplexFromEntry(entry));
                }
            }
            return null;
        }


        private static WZObject ComplexFromEntry(IMGEntry entry)
        {
            if (entry.value == null)
            {
                if (entry.childs.Count == 0)
                {
                    return PropertyFromEntry(entry);
                }
                else
                {
                    if (entry.childs.Contains("Convex0"))
                    {
                        return ConvexFromEntry(entry);
                    }
                    else
                    {
                    return PropertyFromEntry(entry);
                    }
                }
            }
            else
            {
                switch (entry.value.type)
                {
                    case WZObjectType.WZ_VECTOR: return VectorFromEntry(entry);
                    case WZObjectType.WZ_CANVAS: return CanvasFromEntry(entry);
                    case WZObjectType.WZ_SOUND: return SoundFromEntry(entry);
                    case WZObjectType.WZ_UOL: return UOLFromEntry(entry);
                }
            }
            return null;
        }


        private static WZEmpty EmptyFromEntry(IMGEntry entry)
        {
            return new WZEmpty(entry.Name);
        }

        private static WZShort ShortFromEntry(IMGEntry entry)
        {
            return new WZShort(entry.Name, ((WZShort)entry.value).value);
        }

        private static WZInteger IntegerFromEntry(IMGEntry entry)
        {
            return new WZInteger(entry.Name, ((WZInteger)entry.value).value);
        }

        private static WZFloat FloatFromEntry(IMGEntry entry)
        {
            return new WZFloat(entry.Name, ((WZFloat)entry.value).value);
        }

        private static WZDouble DoubleFromEntry(IMGEntry entry)
        {
            return new WZDouble(entry.Name, ((WZDouble)entry.value).value);
        }

        private static WZString StringFromEntry(IMGEntry entry)
        {
            return new WZString(entry.Name, ((WZString)entry.value).value);
        }

        private static WZProperty PropertyFromEntry(IMGEntry entry)
        {
            WZProperty obj = new WZProperty();
            for (int i = 0; i < entry.childs.Count; i++)
            {
                obj.objects.Add(ObjectFromEntry(entry.childs[i]));
            }
            return obj;
        }

        private static WZConvex ConvexFromEntry(IMGEntry entry)
        {
            WZConvex obj = new WZConvex();
            for (int i = 0; i < entry.childs.Count; i++)
            {
                obj.objects.Add(ComplexFromEntry(entry.childs[i]));
            }
            return obj;
        }

        private static WZVector VectorFromEntry(IMGEntry entry)
        {
            WZVector v = (WZVector)entry.value;
            return new WZVector(v.x, v.y);
        }

        private static WZCanvas CanvasFromEntry(IMGEntry entry)
        {
            WZCanvas c = (WZCanvas)entry.value;
            WZCanvas obj = new WZCanvas(c.file, c.width, c.height, c.size, c.offset, c.format);
            obj.SetBitmap(c.GetBitmap());
            for (int i = 0; i < entry.childs.Count; i++)
            {
                obj.objects.Add(ObjectFromEntry(entry.childs[i]));
            }
            return obj;
        }

        private static WZSound SoundFromEntry(IMGEntry entry)
        {
            return new WZSound();
        }

        private static WZUOL UOLFromEntry(IMGEntry entry)
        {
            WZUOL u = (WZUOL)entry.value;
            return new WZUOL(u.path);
        }

        public abstract object Clone();

        public virtual void Dispose()
        {
        }
    }
}
