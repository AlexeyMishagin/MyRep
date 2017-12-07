using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIC_Test_work
{
    public partial class BICView : Form, IBICView
    {
        private string valuecellbeforeedit;
        private string saveFilePath;

        public BICView()
        {
            InitializeComponent();

            butOpenPath.Click += butOpenPath_Click;
            btnSave.Click += btnSave_Click;
            dtgBICTable.CellEndEdit += dtgBICTable_CellValueChanged;
            butSelectPath.Click += butSelectPath_Click;
            textSearch.TextChanged += textSearch_TextChanged;
            dtgBICTable.RowsAdded += dtgBICTable_RowsAdded;
            dtgBICTable.CellBeginEdit += dtgBICTable_CellBeginEdit;
            cmbBoxSearch.SelectionChangeCommitted += cmbBoxSearch_Click;
        }

        #region Events
        void butOpenPath_Click(object sender, EventArgs e)
        {
            if (OpenClick != null) OpenClick(this, EventArgs.Empty);
        }

        void dtgBICTable_CellValueChanged(object sender, EventArgs e)
        {
            if (BICTableChange != null) BICTableChange(this, EventArgs.Empty);
        }

        void textSearch_TextChanged(object sender, EventArgs e)
        {
            if (TextSearchChange != null) TextSearchChange(this, EventArgs.Empty);
        }

        void dtgBICTable_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dtgBICTable.CurrentCell.Value != null)
                valuecellbeforeedit = dtgBICTable.CurrentCell.Value.ToString();
                //if (CellBeginEdit != null) CellBeginEdit(this, e);
            else
                valuecellbeforeedit = "";
        }

        void dtgBICTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {    
            if (BICTableAddRow != null) BICTableAddRow(this, e);
        }

        void cmbBoxSearch_Click(object sender, EventArgs e)
        {
            if (TextSearchChange != null) TextSearchChange(this, e);
        }
        #endregion

        #region IBICView
        public event EventHandler OpenClick;
        public event EventHandler SaveClick;
        public event EventHandler BICTableChange;
        public event EventHandler TextSearchChange;
        public event EventHandler ComboBoxSearchClick;
        public event DataGridViewRowsAddedEventHandler BICTableAddRow;
        //public event DataGridViewCellCancelEventHandler CellBeginEdit;

        public string DBFPath
        {
            get
            {
                if (string.IsNullOrEmpty(fldDBPath.Text))
                {
                    fldDBPath.Text = "C:\\";
                    return "C:\\";   
                }
                return fldDBPath.Text;
            }
            //get { return fldDBPath.Text; }
        }

        public string SaveFilePath { get { return saveFilePath; } }

        public DataGridView BICTable
        {
            get { return dtgBICTable; }
            set { dtgBICTable = value;}
        }

        public BindingNavigator BICViewNavigator
        {
            get { return navBICView; }
            set { navBICView = value; }
        }

        public string TextSearch
        {
            get { return textSearch.Text; }
        }

        public TextBox TextBoxSearch
        {
            get { return textSearch; }
            set { textSearch = value; } 
        }

        public string ValueCellBeforeEdit
        {
            get { return valuecellbeforeedit; }
        }

        public ComboBox ComboBoxSearch
        {
            get { return cmbBoxSearch; }
            set { cmbBoxSearch = value; }
        }
        #endregion

        private void butSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderdlg = new FolderBrowserDialog();
            folderdlg.Description = "Выберите каталог со справочником БИК.";

            if (!string.IsNullOrEmpty(fldDBPath.Text))
                folderdlg.SelectedPath = fldDBPath.Text;

            if (folderdlg.ShowDialog() == DialogResult.OK)
            {
                fldDBPath.Text = folderdlg.SelectedPath;
                if (OpenClick != null) OpenClick(this, EventArgs.Empty);
            }
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFiledlg = new SaveFileDialog();

            saveFiledlg.FileName = "MyDB";
            saveFiledlg.DefaultExt = "dbf";
            saveFiledlg.Filter = "DBF files (*.dbf)|*.dbf";

            if (saveFiledlg.ShowDialog() == DialogResult.OK)
            {
                // В исходный каталог не сохраняем
                if (Path.GetDirectoryName(saveFiledlg.FileName) == fldDBPath.Text)
                {
                    MessageBox.Show("Выбранный каталог служебный, сохранение в него не возможно!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    saveFilePath = saveFiledlg.FileName;
                    // файл может быть открыт или занят процессом
                    try
                    {
                        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (SaveClick != null) SaveClick(this, EventArgs.Empty);   
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
