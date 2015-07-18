using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DLS
{
    public partial class DataViewTest : Form
    {
        DataTable _table = null;
        public DataViewTest(DataTable dataTable)
        {
            _table = dataTable;
            InitializeComponent();
        }
       
        private void DataViewTest_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = _table;
        }
    }
}
