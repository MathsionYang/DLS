using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyPluginEngine;
using System.Drawing;

using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;



/**
 * author lk 
 * 2015/3/25
 * DLS作为一个command加入到框架中
 * 有两个问题没有完全结果
 * 
 * 1.关于限制区域没看到代码中有写转化为asc11码；
 * 2.土地利用类型，情景数据的制作，不确定是不是需要代码里实现。
 * 
 **/

namespace DLS
{
    public class DLS : MyPluginEngine.ICommand
    {
        private MyPluginEngine.IApplication hk;
        private System.Drawing.Bitmap m_hBitmap;

        //private ESRI.ArcGIS.SystemUI.ICommand cmd = null;
        private IMapControlDefault _MapControl = null;
        private IMap _Map = null;
        public DLS()
        {
            string str = @"..\Data\Image\MainTools\dls.png";
            if (System.IO.File.Exists(str))
                m_hBitmap = new Bitmap(str);
            else
                m_hBitmap = null;
        }

        #region ICommand 成员

        public System.Drawing.Bitmap Bitmap
        {
            get { return m_hBitmap; }
        }

        public string Caption
        {
            get { return "土地利用动态模拟"; }
        }

        public string Category
        {
            get { return "土地利用动态模拟"; }
        }

        public bool Checked
        {
            get { return false; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public int HelpContextId
        {
            get { return 0; }
        }

        public string HelpFile
        {
            get { return ""; }
        }

        public string Message
        {
            get { return "DLS"; }
        }

        public string Name
        {
            get { return "DLS"; }
        }

        public void OnClick()
        {
            frmDLSSimulation frmDLS = new frmDLSSimulation(_Map);
            frmDLS.ShowDialog();
        }

        public void OnCreate(MyPluginEngine.IApplication hook)
        {
            if (hook != null)
            {
                this.hk = hook;
                _MapControl = this.hk.MapControl;
                _Map = _MapControl.Map;
            }
        }

        public string Tooltip
        {
            get { return "DLS"; }
        }

        #endregion
    }
}

