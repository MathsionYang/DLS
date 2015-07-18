using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyPluginEngine;


//更新测试lizy 2015/7/17
namespace DLS
{
    public class DLSBar : MyPluginEngine.IMenuDef
    {
        #region IMenuDef 成员

        public string Caption
        {
            get { return "土地利用动态模拟"; }
        }

        public void GetItemInfo(int pos, ItemDef itemDef)
        {
            switch (pos)
            {
                case 0:
                    itemDef.ID = "DLS.DLS";
                    itemDef.Group = false;
                    break;
               
                ////case 1:
                //    //itemDef.ID = "";
                //    //itemDef.Group = true;
                //    break;
                default:
                    break;

            }
        }

        public long ItemCount
        {
            get { return 1; }
        }

        public string Name
        {
            get { return "DLSBar"; }
        }

        #endregion
    }
}
