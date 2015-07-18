using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;


using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.SpatialAnalyst;

using Accord;
using Accord.IO;
using Accord.Math;
using Accord.Statistics.Analysis;

//using DLS.CommandLibrary;

namespace DLS
{
    public partial class frmDLSSimulation : Form
    {
        private IMap pMap = null;
        string strProjectPath = "";
        string strDlsPath = "";
        List<string> lsbNames = new List<string>();


        public frmDLSSimulation()
        {
            InitializeComponent();
        }

        public frmDLSSimulation(IMap _pMap)
        {
            InitializeComponent();
            if (_pMap != null)
                pMap = _pMap;
        }

        private void frmDLSParemeters_Load(object sender, EventArgs e)
        {
            
            for (int i = 0; i < pMap.LayerCount; i++)
            {
                if (pMap.get_Layer(i) is RasterLayer)
                {
                    lsbLayerAll.Items.Add(pMap.get_Layer(i).Name);
                    cmbBoundary.Items.Add(pMap.get_Layer(i).Name);
                    cmbRstraint.Items.Add(pMap.get_Layer(i).Name);
                }
            }
        }

        private void btnSimulation_Click(object sender, EventArgs e)
        {
            frmDLS frmDLS = new frmDLS(strDlsPath);
            frmDLS.ShowDialog();
        }

        private void btnProjectPath_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Application.StartupPath);

            string strFromPath = Application.StartupPath+ "\\DLS";
            string strToPath = "";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择工程数据存储路径";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                strToPath = fbd.SelectedPath;
                strDlsPath = strToPath;
                this.txtProjectPath.Text = strToPath;
                //dls输入数据路径
                strProjectPath = strToPath + "\\DLS\\DLS\\Input";

                CopyFolder(strFromPath,strToPath);
                MessageBox.Show(this,"工程创建成功\""+strToPath+"\"","提示");
            }
        }

        /// <summary>
        /// 拷贝文件夹(包括子文件夹)到指定文件夹下
        /// 文件和文件夹分开复制，当是文件夹则递归复制
        /// </summary>
        /// <param name="strFromPath">待复制地址</param>
        /// <param name="strToPath">目标地址</param>
        public static void CopyFolder(string strFromPath, string strToPath)
        {
            //如果源文件夹不存在，则创建  
            if (!Directory.Exists(strFromPath))
            {
                Directory.CreateDirectory(strFromPath);
            }
            //取得要拷贝的文件夹名  
            string strFolderName = strFromPath.Substring(
                strFromPath.LastIndexOf("\\") + 1,
                strFromPath.Length -
                strFromPath.LastIndexOf("\\") - 1);
            //如果目标文件夹中没有源文件夹
            //则在目标文件夹中创建源文件夹 
            if (!Directory.Exists(
                strToPath + "\\" + strFolderName))
            {
                Directory.CreateDirectory(
                    strToPath + "\\" + strFolderName);
            }
            //创建数组保存源文件夹下的文件名  
            string[] strFiles =
                Directory.GetFiles(strFromPath);
            //循环拷贝文件 
            for (int i = 0; i < strFiles.Length; i++)
            {
                //取得拷贝的文件名，只取文件名，地址截掉。
                string strFileName = strFiles[i].Substring(
                    strFiles[i].LastIndexOf("\\") + 1,
                    strFiles[i].Length -
                    strFiles[i].LastIndexOf("\\") - 1);
                //开始拷贝文件,true表示覆盖同名文件  
                File.Copy(
                    strFiles[i],
                    strToPath + "\\" + strFolderName +
                    "\\" + strFileName,
                    true);
            }
            //创建DirectoryInfo实例  
            DirectoryInfo dirInfo =
                new DirectoryInfo(strFromPath);
            //取得源文件夹下的所有子文件夹名称 
            DirectoryInfo[] ZiPath =
                dirInfo.GetDirectories();
            for (int j = 0; j < ZiPath.Length; j++)
            {
                //获取所有子文件夹名  
                string strZiPath = strFromPath + "\\" +
                    ZiPath[j].ToString();
                //把得到的子文件夹当成新的
                //源文件夹，从头开始新一轮的拷贝
                CopyFolder(
                    strZiPath,
                    strToPath + "\\" + strFolderName);
            }
        }

        private void btnAddY_Click(object sender, EventArgs e)
        {
            if (lsbLayerAll.Items.Count == 0)
                return;
            //一次只能选择一个数据
            if (lsbLayerAll.SelectedItems.Count > 1)
            {
                MessageBox.Show(this, "土地利用数据只能选择一个图层数据" , "提示");
                return;
            }
            if (lsbLayerAll.SelectedItem == null)
                return;
            //如果lsbLayerLandUse不为空，先移动原有的到lsbLayerAll，
            //再将lsbLayerAll选择的移动到lsbLayerLandUse
            if (lsbLayerLandUse.Items.Count == 1)
            {
                this.lsbLayerAll.Items.Add(this.lsbLayerLandUse.Items[0]);
                this.lsbLayerLandUse.Items.RemoveAt(0);
            }

            this.lsbLayerLandUse.Items.Add(this.lsbLayerAll.SelectedItem);
            this.lsbLayerAll.Items.Remove(lsbLayerAll.SelectedItem);
        }

        private void btnAddX_Click(object sender, EventArgs e)
        {
            if (lsbLayerAll.Items.Count == 0)
                return;
            if (lsbLayerAll.SelectedItem == null)
                return;
            //lsbLayerAll与lsbLayerDriverFactor 可多选SelectMode=MultiSimple）
            //lsbLayerAll与lsbLayerDriverFactor 排序
            for (int i = 0; i < lsbLayerAll.SelectedItems.Count; i++)
            {
                //将lsbLayerAll选择的X移动到lsbLayerLandUse
                this.lsbLayerDriverFactor.Items.Add(this.lsbLayerAll.SelectedItems[i]);
                this.lsbLayerAll.Items.Remove(lsbLayerAll.SelectedItems[i]);
            }
        }

        private void btnRemoveX_Click(object sender, EventArgs e)
        {
            if (lsbLayerDriverFactor.Items.Count == 0)
                return;
            if (lsbLayerDriverFactor.SelectedItem == null)
                return;
            for (int i = 0; i < lsbLayerDriverFactor.SelectedItems.Count; i++)
            {
                //将lsbLayerAll选择的X移动到lsbLayerLandUse
                this.lsbLayerAll.Items.Add(this.lsbLayerDriverFactor.SelectedItems[i]);
                this.lsbLayerDriverFactor.Items.Remove(lsbLayerDriverFactor.SelectedItems[i]);
            }
        }

        private void btnBoundary_Click(object sender, EventArgs e)
        {
            IWorkspaceFactory workSpaceF = new RasterWorkspaceFactory();
            IWorkspace workSpace;
            string strToPath = "";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择栅格数据存储路径";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                strToPath = fbd.SelectedPath;
                this.txtProjectPath.Text = strToPath;
                workSpace = workSpaceF.OpenFromFile(strToPath,0);
                if(workSpace==null)
                {
                    MessageBox.Show("Could not open workspce");
                    return;
                }

            }
        }

        private void Rater2Ascii(IGeoDataset mask,int cellsize,IGeoDataset inData,string ascFileName)
        {
            if (mask == null)
                return;
            if (inData == null)
                return;
            if (ascFileName.Trim() == "")
                return;
            IRasterExportOp rexp=new RasterConversionOpClass();
            IRasterAnalysisEnvironment pEnv = (IRasterAnalysisEnvironment)rexp;
            object dCellSize = cellsize;  //栅格大小
            pEnv.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref dCellSize);
            pEnv.Mask=mask;
            //cov1_0.0  con1_(land use type)_(simulation year)
            //string ascFile=this.txtProjectPath.Text="\\DLS\\DLS\\Input\\con1_0.0";
            //文件存在就删除
            FileInfo file = new FileInfo(ascFileName);
            if (file.Exists)
            {
                file.Delete();
            } 
            rexp.ExportToASCII(inData, ascFileName);
            //IRasterextrac rexp = new RasterConversionOpClass();
        }
        private void Rater2ASCII_GP()
        {
            IAoInitialize m_AoInitialize = new AoInitializeClass();
            esriLicenseStatus licenseStatus = esriLicenseStatus.esriLicenseUnavailable;
            licenseStatus = m_AoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
            if (licenseStatus == esriLicenseStatus.esriLicenseCheckedOut)
            {
                Geoprocessor tGp = new Geoprocessor();
                tGp.OverwriteOutput = true;
                //执行raster to ASC2 
                RasterToASCII rastertoASCIITool = new RasterToASCII();
                string ascFile = "";
                IGeoDataset gdsIn = null;
                rastertoASCIITool.in_raster = gdsIn;
                rastertoASCIITool.out_ascii_file = ascFile;

                IGeoProcessorResult tGeoResult = (IGeoProcessorResult)tGp.Execute(rastertoASCIITool, null);
            }

        }

        private void btnGenerateInputData_Click(object sender, EventArgs e)
        {
            //if (this.strProjectPath.Trim() == "")
            //    return;
            //if (this.cmbBoundary.Text.Trim() == "")
            //    return;
            //if (this.cmbRstraint.Text.Trim() == "")
            //    return;
            //if (this.txtParameter.Text.Trim() == "")
            //    return;
            if (lsbLayerLandUse.Items.Count == 0)
                return;
            //if (lsbLayerDriverFactor.Items.Count == 0)
            //    return;
            int iLanduseType = 5;

            //ILayer pLayer = null;
            IRaster pRaster = new RasterClass();

            IGeoDataset pGdsMask = null;
            IGeoDataset pGdsRstraint = null;
            IGeoDataset pGdsLanduse = null;
            IGeoDataset pGdsDriverFactor = null;
            IRasterBandCollection pRasterBandColection=(new RasterClass()) as IRasterBandCollection;
            
            //try
            //{
                string sLyrMask = this.cmbBoundary.Text;

                //boundaty-->mask
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    ILayer pLyr = pMap.get_Layer(i);
                    if (pLyr is IRasterLayer)
                    {
                        if (pLyr.Name == sLyrMask)
                        {
                            //pRaster = (pLyr as IRasterLayer).Raster;
                            pGdsMask = (pLyr as IRasterLayer).Raster as IGeoDataset;
                        }
                    }
                }
                //data
                //限制区
                string sLyrRstraint = this.cmbBoundary.Text;
                //土地利用数据
                string sLyrLanduse = this.lsbLayerLandUse.Items[0].ToString();

                // 土地利用数据与驱动因子 数据名称(去除格式名)
                // 顺序不能改变  后面 ITable2DTable 使用这个列表进行了名称替换
                lsbNames.Add(sLyrLanduse.Remove(sLyrLanduse.LastIndexOf(".")));
                foreach (string name in lsbLayerDriverFactor.Items)
                {
                    lsbNames.Add(name.Remove(name.LastIndexOf("."))); //去除文件格式 
                }
               
                //驱动因子
                //string[] arr = new string[this.lsbLayerDriverFactor.Items.Count];
                
                //for (int i = 0; i < this.lsbLayerDriverFactor.Items.Count; i++) 
                //{
                //    arr[i]=this.lsbLayerDriverFactor.Items[i].ToString();
                //}
                
                for (int i = 0; i < pMap.LayerCount; i++)
                {
                    ILayer pLyr = pMap.get_Layer(i);
                    if (pLyr is IRasterLayer)
                    {
                        //IRaster curRaster = new RasterClass();
                        if (pLyr.Name == sLyrRstraint)
                        {
                            //curRaster = (pLyr as IRasterLayer).Raster;
                            pGdsRstraint = (pLyr as IRasterLayer).Raster as IGeoDataset;
                        }
                        
                        //土地利用数据
                        if (pLyr.Name == sLyrLanduse)
                        {
                            this.rtxtState.AppendText("读取土地利用参数数据...\n");
                            this.rtxtState.ScrollToCaret();
                            
                            //curRaster = (pLyr as IRasterLayer).Raster;
                            pGdsLanduse = (pLyr as IRasterLayer).Raster as IGeoDataset;
                            //land use 添加到 IRasterBandCollection
                            IRasterBandCollection rasterbands = (IRasterBandCollection)(pLyr as IRasterLayer).Raster;
                            IRasterBand rasterband = rasterbands.Item(0);
                            pRasterBandColection.AppendBand(rasterband);

                            //pRasterBandColection = curRaster as IRasterBandCollection;
                            //pRasterBandColection.AppendBand(pRaster as IRasterBand);

                            string ascFileNameLanduse = strProjectPath + "\\cov1_all.0";
                            //cov1_0.0;cov1_1.0;
                            Rater2Ascii(pGdsMask, 100, pGdsLanduse, ascFileNameLanduse);
                            
                            //将土地利用数据拆分

                            StreamReader sr = new StreamReader(ascFileNameLanduse, System.Text.Encoding.Default);
                            //try
                            //{

                            //使用StreamReader类来读取文件
                            sr.BaseStream.Seek(0, SeekOrigin.Begin);
                            // 从数据流中读取每一行，直到文件的最后一行，并在richTextBox1中显示出内容
                            //读取头文件
                            string[] header = new string[6];

                            for (int j = 0; j < 6; j++)
                            {
                                header[j] = sr.ReadLine();
                            }
                            //行列数
                            string[] ncols = header[0].Split(' ');
                            string[] nrows = header[1].Split(' ');
                            int icol = int.Parse(ncols[ncols.Length - 1]);
                            int irow = int.Parse(nrows[nrows.Length - 1]);

                            int[,] iLanduse = new int[irow, icol];
                            //
                            string strLine = sr.ReadLine();
                            string[] strData;
                            int ir = 0;
                            while (ir < irow)
                            //while (strLine != null)
                            {
                                strData = strLine.Split(' ');
                                for (int ic = 0; ic < icol; ic++)
                                {
                                    iLanduse[ir, ic] = int.Parse(strData[ic]);
                                }
                                strLine = sr.ReadLine();
                                ir++;
                            }
                            //关闭此StreamReader对象
                            sr.Close();
                            //输出相应的土地利用数据
                            DataTable2Txt(header, iLanduseType, iLanduse, strProjectPath);

                            //}
                            //catch (Exception ex)
                            //{
                            //MessageBox.Show(ex.Message);
                            sr.Close();
                            //}
                        }
                    }
                }
                this.rtxtState.AppendText("输出土地利用参数数据成功。\n");
                this.rtxtState.AppendText("读取驱动因子数据...\n");
                this.rtxtState.ScrollToCaret();
                for (int ifac = 0; ifac < lsbLayerDriverFactor.Items.Count; ifac++)
                {
                    for (int i = 0; i < pMap.LayerCount; i++)
                    {
                        ILayer pLyr = pMap.get_Layer(i);
                        if (pLyr is IRasterLayer)
                        {
                        //输出驱动因子数据  sc1gr0.grid
                       
                            string sFacName = lsbLayerDriverFactor.Items[ifac].ToString();
                            if (pLyr.Name == sFacName)
                            {
                                //IRaster curRaster = new RasterClass();  
                                //curRaster = (pLyr as IRasterLayer).Raster;
                                pGdsDriverFactor = (pLyr as IRasterLayer).Raster as IGeoDataset;

                                string ascFileNameFac = strProjectPath + "\\sc1gr"+ifac.ToString()+".grid";
                                //cov1_0.0;cov1_1.0;
                                this.rtxtState.AppendText("输出驱动因子数据【" + sFacName + "】\n");
                                //IGeoDataset curMask = null;
                                //curMask = pGdsMask;
                                Rater2Ascii(pGdsMask, 100, pGdsDriverFactor, ascFileNameFac);
                                this.rtxtState.AppendText("输出驱动因子数据【" + sFacName + "】成功。\n");
                                this.rtxtState.ScrollToCaret();
                                //mask 添加到 IRasterBandCollection
                                IRasterBandCollection rasterbands = (IRasterBandCollection)(pLyr as IRasterLayer).Raster;
                                IRasterBand rasterband = rasterbands.Item(0);
                                pRasterBandColection.AppendBand(rasterband);
                                //pRasterBandColection.Add(pRaster as IRasterBand,ifac+1);

                            }
                        }
                    }
                }
                this.rtxtState.AppendText("开始制备驱动因子参数...\n");
                this.rtxtState.ScrollToCaret();

                //IGeoDataset curtestGeo = null;
                ////boundaty-->mask
                //for (int i = 0; i < pMap.LayerCount; i++)
                //{
                //    ILayer pLyr = pMap.get_Layer(i);
                //    if (pLyr is IRasterLayer)
                //    {
                //        if (pLyr.Name == sLyrMask)
                //        {
                //            curtestGeo = ((pLyr as IRasterLayer).Raster) as IGeoDataset;
                //        }
                //    }
                //}


                //sample data
                ITable itFactors = ExportSample(pGdsMask, pRasterBandColection);
                
                //logistic 回归分析


                //lsbNames.AddRange(names);
                DataTable dtFactors = ITable2DTable(itFactors);


                itFactors = null;
                //MessageBox.Show(dtFactors.Columns[0].ColumnName + ";" + dtFactors.Columns[1].ColumnName + ";" + dtFactors.Columns[2].ColumnName);
            
                //制备驱动力参数文件
                this.rtxtState.AppendText("读取驱动因子数据表格数据...\n");
                this.rtxtState.ScrollToCaret();
                LogisticRegressionAnalysis lra;
                // Gets the columns of the independent variables
                List<string> names = new List<string>();
                foreach (string name in lsbLayerDriverFactor.Items)
                {
                    names.Add(name.Remove(name.LastIndexOf("."))); //去除文件格式  
                } 
                

                String[] independentNames = names.ToArray();
                DataTable independent = dtFactors.DefaultView.ToTable(false, independentNames);
                // Creates the input and output matrices from the source data table
                double[][] input = independent.ToArray();
                double[,] sourceMatrix = dtFactors.ToMatrix(independentNames);

                StreamWriter sw = new StreamWriter(strProjectPath + "\\alloc1.reg", false);
                for(int ild=0; ild< iLanduseType; ild++)
                {
                    String landuseName = (string)this.lsbLayerLandUse.Items[0].ToString();
                    this.rtxtState.AppendText("开始制备土地利用类型【" + ild.ToString() + "】驱动因子参数...\n");
                    this.rtxtState.ScrollToCaret();
                    DataColumn taxColumn =new DataColumn(); 
                    taxColumn.DataType = System.Type.GetType("System.Int32"); 
                    taxColumn.ColumnName ="sysland"+ild.ToString();//列名 
                    taxColumn.Expression = "iif("+lsbNames[0]+" = "+ild.ToString()+",1,0)";//设置该列得表达式，用于计算列中的值或创建聚合列
                    dtFactors.Columns.Add(taxColumn);
                    string dependentName = "sysland" + ild.ToString();

                    DataTable dependent = dtFactors.DefaultView.ToTable(false, dependentName);
                    double[] output = dependent.Columns[dependentName].ToArray();
                   

                    // Creates the Simple Descriptive Analysis of the given source
                    DescriptiveAnalysis sda = new DescriptiveAnalysis(sourceMatrix, independentNames);
                    sda.Compute();

                    // Populates statistics overview tab with analysis data
                    //dgvDistributionMeasures.DataSource = sda.Measures;

                    // Creates the Logistic Regression Analysis of the given source
                    lra = new LogisticRegressionAnalysis(input, output, independentNames, dependentName);


                    // Compute the Logistic Regression Analysis
                    lra.Compute();

                    // Populates coefficient overview with analysis data
                    //lra.Coefficients;
              
                    MessageBox.Show(lra.Coefficients.Count.ToString());
                    MessageBox.Show(lra.CoefficientValues[0].ToString());


                    //string str_check = listBox_deVar.Items[var_count].ToString().ToLower();
                    string st = ild.ToString();
                    int Relength = lra.CoefficientValues.Length;
                   
                    int number = 0;
                    for (int i = 1; i < Relength; i++)
                    {
                        //if (< 0.05)
                        //{
                            number++;
                        //}
                    }
                    RegressionResult.Items.Add(st);
                    RegressionResult.Items.Add("\t" + Math.Round(lra.CoefficientValues[0], 6));
                    RegressionResult.Items.Add(number);
                    int var_number = 0;
                    for (int i = 1; i < Relength; i++)
                    {
                        //if ( < 0.05)
                        //{
                        RegressionResult.Items.Add("\t" + Math.Round(lra.CoefficientValues[i], 6) + "\t" + var_number);
                        //}
                        var_number = var_number + 1;
                    }

                    // 保存alloc1.reg 文件
                    sw.WriteLine(st);
                    sw.WriteLine("\t" + Math.Round(lra.CoefficientValues[0], 6));
                    sw.WriteLine(number);
                    int var_number2 = 0;
                    for (int i = 1; i < Relength; i++)
                    {
                        //if ( < 0.05)
                        //{
                        sw.WriteLine("\t" + Math.Round(lra.CoefficientValues[i]) + "\t" + var_number2);
                        //}
                        var_number2 = var_number2 + 1;
                    }
                    //progressBar1.Value = var_count + 1;
                }
                sw.Close();
            
                this.rtxtState.AppendText("制备驱动因子参数成功。\n");
                this.rtxtState.ScrollToCaret();
                MessageBox.Show(dtFactors.Rows.Count.ToString());
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void btnParameterOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdParameter = new OpenFileDialog();
            ofdParameter.InitialDirectory = strProjectPath;
            ofdParameter.Filter = "(*.1)|*.1|" +"(*.txt)|*.txt|" + "(*.*)|*.*";
            ofdParameter.FilterIndex = 0;
            if (ofdParameter.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(ofdParameter.FileName, System.Text.Encoding.Default);
                try
                {
                    //使用StreamReader类来读取文件
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    // 从数据流中读取每一行，直到文件的最后一行，并在richTextBox1中显示出内容
                    this.txtParameter.Text = "";
                    string strLine = sr.ReadLine();
                    while (strLine != null)
                    {
                        this.txtParameter.AppendText(strLine);
                        strLine = sr.ReadLine();
                    }
                    //关闭此StreamReader对象
                    sr.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    sr.Close();
                }
            }         
        }

        private void btnParameterSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfdParameterSave = new SaveFileDialog();
                sfdParameterSave.InitialDirectory = strProjectPath;
                sfdParameterSave.Filter = "(*.1)|*.1|" + "(*.txt)|*.txt|" + "(*.*)|*.*";
                sfdParameterSave.FileName = "main";

                if (sfdParameterSave.ShowDialog() == DialogResult.OK)
                {
                    this.txtParameter.SaveFile(sfdParameterSave.FileName, RichTextBoxStreamType.PlainText);//重点在此句
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmbRstraint_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            //toolTip1.AutoPopDelay = 1000;//一下4个都是属性
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            String txtHelp = "栅格数据 Value=1，为限制区，其他值则为非限制区！";
            toolTip1.SetToolTip(this.txtParameter, txtHelp);//参数1是button名,参数2是要显示的内容
        }

        private void txtParameter_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            //toolTip1.AutoPopDelay = 1000;//一下4个都是属性
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            String txtHelp = "第1行：用地类型数\n";
            txtHelp = txtHelp + "第2行：用地类型编码，从0开始\n";
            txtHelp = txtHelp + "第3行：与用地类型对应的转换规则\n";
            txtHelp = txtHelp + "第4行：收敛条件：土地需求变化和实际分配土地之间收敛时的允许误差\n";
            txtHelp = txtHelp + "第5行：模拟的起始和结束年份\n";
            txtHelp = txtHelp + "第6行：用地类型分布驱动因子随时间变化而变化的数量\n";
            txtHelp = txtHelp + "第7行：头文件记录格式标记：1表示ArcView的标题文件将在输出文件中输出，0表示不输出。";
            toolTip1.SetToolTip(this.txtParameter, txtHelp);//参数1是button名,参数2是要显示的内容
        }
        /// <summary>
        /// 存在ascii格式的文件
        /// </summary>
        /// <param name="_header">arcgis ascii 文件头</param>
        /// <param name="_dt">ascii文件数据</param>
        /// <param name="_saveFileName">保存路径</param>
        private void DataTable2Txt(string[] _header,int _landusetype, int[,] _dt,string _FilePath)
        {
            for (int ild = 0; ild < _landusetype; ild++)
            {
                string _saveFileName = _FilePath + "\\cov1_" + ild.ToString() + ".0";
                StreamWriter sr;
                if (File.Exists(_saveFileName))
                {
                    //如果文件存在,则删除对象 
                    File.Delete(_saveFileName);
                }
                //创建File.CreateText对象   
                sr = File.CreateText(_saveFileName);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _header.Length; i++)
                {
                    sr.WriteLine(_header[i]);
                }
                string dtLine = "";
                for (int i = 0; i < _dt.GetLength(0); i++)
                {
                    dtLine = "";
                    for (int j = 0; j < _dt.GetLength(1); j++)
                    {
                        if (_dt[i, j] == ild)
                        {
                            dtLine = dtLine+"1 ";
                        }
                        else if (_dt[i, j] == -9999)
                        {
                            dtLine = dtLine+"-9999 ";
                        }
                        else
                        {
                           dtLine = dtLine+"0 ";
                        }
                    }
                    dtLine = dtLine.TrimEnd();
                    sr.WriteLine(dtLine);
                }
                sr.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_pGdsLocation"></param>
        /// <param name="_BandCol"></param>
        /// <returns></returns>
        private ITable ExportSample(IGeoDataset _pGdsLocation, IRasterBandCollection _BandCol)
        {
            //IRasterLayer pRastetLayer = pLayer as IRasterLayer;
            //IRasterBandCollection pRasterBandColection = pRastetLayer.Raster as IRasterBandCollection;
            //if (pRasterBandColection.Count == 1)
            //{
                //Create the RasterExtractionOp object
                IExtractionOp pExtractionOp = new RasterExtractionOpClass();
                ////Declare the input location raster object
                //IGeoDataset pGdsLocation=_pGdsLocation;  
                ////Create a raster of multiple bands
                //IRasterBandCollection pBandCol=_BandCol;
                ////Declare the output table object
                ITable pOutputTable;
                //Calls the method     
                pOutputTable = pExtractionOp.Sample(_pGdsLocation, _BandCol as IGeoDataset, esriGeoAnalysisResampleEnum.esriGeoAnalysisResampleNearest);     
                return pOutputTable;
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_pRTable"></param>
        /// <returns></returns>
 
        private DataTable ITable2DTable(ITable _pRTable)
        {
            DataTable pTable = new DataTable();
            //此方法不能使用，对一些字段是不能删除的
            // cannot be removed include:
            //OBJECTID field 
            //SHAPE and shape dependent fields such as SHAPE_Length 
            //Enabled, AncillaryRole and Weight field for network feature classes 
            // try  
            //{ 
                // 删除字段 X Y
                //IFields pfields;
                //IField pfield;
                //pfields = _pRTable.Fields;
                //int fieldIndex = pfields.FindField("x");
                //pfield = pfields.get_Field(fieldIndex);
                //_pRTable.DeleteField(pfield);
                

                //fieldIndex = pfields.FindField("y");
                //pfield = pfields.get_Field(fieldIndex);
                //_pRTable.DeleteField(pfield);
            //}


            // catch (Exception ex)
            // {
                 
             //}

            //将ITable 转化到 DataTable 
            for (int i = 0; i < _pRTable.Fields.FieldCount; i++)
            {
                pTable.Columns.Add(_pRTable.Fields.get_Field(i).Name);
            }
            ICursor pCursor = _pRTable.Search(null,false);
            IRow pRrow = pCursor.NextRow();
            bool flag = true;
            while (pRrow != null)
            {
                flag = true;
                DataRow pRow = pTable.NewRow();
                for (int i = 0; i < pRrow.Fields.FieldCount; i++)
                {
                    pRow[i] = pRrow.get_Value(i);
                    //有缺失值的行排除掉
                    if (pRow[i] == System.DBNull.Value)
                    {
                        flag = false;
                        break;
                    }
                }
                if(flag)
                {
                    pTable.Rows.Add(pRow);
                }
                pRrow = pCursor.NextRow();
            }


            // 删除x,y以及之前的列，并对因子列更改列名
            int Yindex = pTable.Columns.IndexOf("y");
            for (int i = 0; i <= Yindex; i++) 
            {
                //MessageBox.Show(pTable.Columns[i].ColumnName);
                //注意这里不是 i 是 0 ;每次删除后需要删除的列在第一个
                pTable.Columns.RemoveAt(0);   
            };
            int fieldCount = pTable.Columns.Count;
            for (int i = 0; i < fieldCount; i++)
            {
                pTable.Columns[i].ColumnName = lsbNames[i];
            }
            //DataViewTest dt = new DataViewTest(pTable);
            //dt.ShowDialog();
            return pTable;
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void btnRstraint_Click(object sender, EventArgs e)
        {

        }
    }
}
