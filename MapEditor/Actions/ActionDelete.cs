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

namespace WZMapEditor.Actions
{
    class ActionDelete : IAction
    {
        int layer = -1;
        private List<MapItem> items;

        public ActionDelete(List<MapItem> items)
        {
            this.items = items;
        }

        public ActionDelete(List<MapItem> items, int layer)
        {
            this.items = items;
            this.layer = layer;
        }

        public ActionDelete(MapItem item)
        {
            items = new List<MapItem>();
            items.Add(item);
        }

        public ActionDelete(MapItem item, int layer)
        {
            items = new List<MapItem>();
            items.Add(item);
            this.layer = layer;
        }

        public void Undo()
        {
            if (layer == -1)
            {
                foreach (MapItem item in items)
                {
                    Map.Instance.Add(item);
                }
            }
            else
            {
                foreach (MapItem item in items)
                {
                    Map.Instance.layers[layer].Add(item);
                }
            }
        }

        public IAction Redo()
        {
            return new ActionAdd(items, layer);
        }

    }
}
