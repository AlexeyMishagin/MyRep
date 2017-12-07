using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBF_Work;

namespace BIC_Test_work
{
    class BICPresenter
    {
        public readonly IBICView _view;
        public readonly IDBFManager _dbfmanager;
        public readonly IMessage _message;
        private string _currentFolder;
        private BindingSource bicBindingSource = new BindingSource();
        private BindingSource pznBindingSource = new BindingSource();
        private string tmpbictable = "tmp_bnkseek";
        private SqlDataAdapter dataAdapter = new SqlDataAdapter();

        public BICPresenter(IBICView view, IDBFManager dbfmanager, IMessage message)
        {
            _view = view;
            _dbfmanager = dbfmanager;
            _message = message;

            _view.BICTableChange += _view_BICTableChange;
            _view.OpenClick += _view_OpenClick;
            _view.SaveClick += _view_SaveClick;
            _view.TextSearchChange += _view_TextSearchChange;
            _view.BICTableAddRow += _view_BICTableAddRow;
            _view.BICTable.DataError += BICTable_DataError;
            _view.ComboBoxSearchClick += _view_ComboBoxSearchClick;
        }

        void _view_ComboBoxSearchClick(object sender, EventArgs e)
        {
            /*
            _view.ComboBoxSearch.Items.Clear();
            foreach (DataGridViewColumn col in _view.BICTable.Columns)
            {
                _view.ComboBoxSearch.Items.Add(col.Name);
            }
            _view.ComboBoxSearch.Items.Add("<ВСЕ>");
             */
        }

        void BICTable_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            _message.ShowError("Ошибка ввода!\n" + e.Exception.Message + "\nНажмите ESC для отмены изменений и выхода из режима редактирования.");
        }

        /// <summary>
        /// Обработка события сохранения таблицы
        /// </summary>
        void _view_SaveClick(object sender, EventArgs e)
        {
            try
            {
                string createsql = "CREATE TABLE " + _view.SaveFilePath + " ([REAL] Char(4), [PZN] Char(2), [UER] Char(1), [RGN] Char(2), IND Char(6), TNP Char(1), NNP Char(25), ADR Char(30), RKC Char(9), NAMEP Char(45), NEWNUM Char(9), TELEF Char(25), REGN Char(9), OKPO Char(8), DT_IZM DATE, KSNP Char(20), DATE_IN DATE, DATE_CH DATE, UNIQUE(NEWNUM))";

                _dbfmanager.Execute(createsql);

                string insertsql = "";
                foreach (DataGridViewRow row in _view.BICTable.Rows)
                {
                    insertsql = "INSERT INTO " + _view.SaveFilePath + " values ('" + row.Cells["REAL"].Value + "', '" +
                                row.Cells["BNK_PZN"].Value + "'" +
                                ",'" + row.Cells["BNK_UER"].Value + "' ,'" + row.Cells["BNK_RGN"].Value + "','" +
                                row.Cells["IND"].Value + "'" +
                                ",'" + row.Cells["BNK_TNP"].Value + "','" + row.Cells["NNP"].Value + "','" +
                                row.Cells["ADR"].Value + "'" +
                                ",'" + row.Cells["RKC"].Value + "','" + row.Cells["NAMEP"].Value + "','" +
                                row.Cells["NEWNUM"].Value + "'" +
                                ",'" + row.Cells["TELEF"].Value + "','" + row.Cells["REGN"].Value + "','" +
                                row.Cells["OKPO"].Value + "'" +
                                "," + GetDateValueForSQL(row.Cells["DT_IZM"].Value) + ",'" + row.Cells["KSNP"].Value + "'," +
                                GetDateValueForSQL(row.Cells["DATE_IN"].Value) + "," + GetDateValueForSQL(row.Cells["DATE_CH"].Value) + ")";

                    _dbfmanager.Execute(insertsql);
                }

                _message.ShowMessage("Файл успешно сохранен.");
            }
            catch (Exception ex)
            {
                _message.ShowError(ex.Message);
            }
        }

        private string GetDateValueForSQL(object col)
        {
            if (string.IsNullOrEmpty(col.ToString()))
            {
                return " NULL";
            }
            return "'" + col.ToString() + "'";
        }

        /// <summary>
        /// Обработка события открытия справочника БИК
        /// </summary>
        void _view_OpenClick(object sender, EventArgs e)
        {
            try
            {
                bicBindingSource.DataSource = GetBICTable(_view.DBFPath);

                _view.BICTable.DataSource = bicBindingSource.DataSource;
                
                _view.BICViewNavigator.BindingSource = bicBindingSource;

                foreach (DataGridViewColumn col in _view.BICTable.Columns)
                {
                    if (col.Name == "BNK_PZN") col.ReadOnly = true;
                    if (col.Name == "BNK_TNP") col.ReadOnly = true;
                    if (col.Name == "BNK_UER") col.ReadOnly = true;
                    if (col.Name == "BNK_RGN") col.ReadOnly = true;

                    switch (col.Name)
                    {
                        case "REAL": (col as DataGridViewTextBoxColumn).MaxInputLength = 4; break;
                        case "IND": (col as DataGridViewTextBoxColumn).MaxInputLength = 6; break;
                        case "NNP": (col as DataGridViewTextBoxColumn).MaxInputLength = 25; break;
                        case "ADR": (col as DataGridViewTextBoxColumn).MaxInputLength = 30; break;
                        case "RKC": (col as DataGridViewTextBoxColumn).MaxInputLength = 9; break;
                        case "NAMEP": (col as DataGridViewTextBoxColumn).MaxInputLength = 45; break;
                        case "NEWNUM": (col as DataGridViewTextBoxColumn).MaxInputLength = 9; break;
                        case "TELEF": (col as DataGridViewTextBoxColumn).MaxInputLength = 25; break;
                        case "REGN": (col as DataGridViewTextBoxColumn).MaxInputLength = 9; break;
                        case "OKPO": (col as DataGridViewTextBoxColumn).MaxInputLength = 8; break;
                        case "KSNP": (col as DataGridViewTextBoxColumn).MaxInputLength = 20; break;
                        case "DT_IZM": col.DefaultCellStyle.Format = "dd.MM.yyyy"; break;
                        case "DATE_IN": col.DefaultCellStyle.Format = "dd.MM.yyyy"; break;
                        case "DATE_CH": col.DefaultCellStyle.Format = "dd.MM.yyyy"; break;
                    }
                }

                string query = "";

                DataGridViewComboBoxColumn comboBOXColumn_PZN = new DataGridViewComboBoxColumn();
                
                query = "SELECT pzn.PZN as PZN_PZN, pzn.NAME as PZN_NAME " +
                               "FROM " + _view.DBFPath + "\\PZN.dbf as pzn ORDER BY 2";
                comboBOXColumn_PZN.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

                pznBindingSource.DataSource = _dbfmanager.Execute(query);
                comboBOXColumn_PZN.DataSource = pznBindingSource.DataSource;
                //comboBOXColumn_PZN.DataSource = _dbfmanager.Execute(query);
                comboBOXColumn_PZN.ValueMember = "PZN_PZN";
                comboBOXColumn_PZN.DisplayMember = "PZN_NAME";

                comboBOXColumn_PZN.DataPropertyName = "BNK_PZN";
                comboBOXColumn_PZN.HeaderText = "PZN_NAME";
                comboBOXColumn_PZN.Name = "PZN_NAME";
                comboBOXColumn_PZN.FlatStyle = FlatStyle.Flat;

                _view.BICTable.Columns.Insert(_view.BICTable.Columns["BNK_PZN"].Index + 1, comboBOXColumn_PZN);

                DataGridViewComboBoxColumn comboBOXColumn_TNP = new DataGridViewComboBoxColumn();
                query = "SELECT tnp.TNP as TNP_TNP, tnp.FULLNAME as TNP_FULLNAME " +
                    "FROM " + _view.DBFPath + "\\TNP.dbf as tnp ORDER BY 2";
                comboBOXColumn_TNP.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                comboBOXColumn_TNP.DataSource = _dbfmanager.Execute(query);
                comboBOXColumn_TNP.ValueMember = "TNP_TNP";
                comboBOXColumn_TNP.DisplayMember = "TNP_FULLNAME";

                comboBOXColumn_TNP.DataPropertyName = "BNK_TNP";
                comboBOXColumn_TNP.HeaderText = "TNP_FULLNAME";
                comboBOXColumn_TNP.Name = "TNP_FULLNAME";
                comboBOXColumn_TNP.FlatStyle = FlatStyle.Flat;
                
                _view.BICTable.Columns.Insert(_view.BICTable.Columns["BNK_TNP"].Index + 1, comboBOXColumn_TNP);

                DataGridViewComboBoxColumn comboBOXColumn_UER = new DataGridViewComboBoxColumn();
                query = "SELECT uer.UER as UER_UER, uer.UERNAME as UER_UERNAME " +
                    "FROM " + _view.DBFPath + "\\UER.dbf as uer ORDER BY 2";
                comboBOXColumn_UER.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                comboBOXColumn_UER.DataSource = _dbfmanager.Execute(query);
                comboBOXColumn_UER.ValueMember = "UER_UER";
                comboBOXColumn_UER.DisplayMember = "UER_UERNAME";

                comboBOXColumn_UER.DataPropertyName = "BNK_UER";
                comboBOXColumn_UER.HeaderText = "UER_UERNAME";
                comboBOXColumn_UER.Name = "UER_UERNAME";
                comboBOXColumn_UER.FlatStyle = FlatStyle.Flat;

                _view.BICTable.Columns.Insert(_view.BICTable.Columns["BNK_UER"].Index + 1, comboBOXColumn_UER);

                DataGridViewComboBoxColumn comboBOXColumn_RGN = new DataGridViewComboBoxColumn();
                query = "SELECT reg.RGN as REG_RGN, reg.NAME as RGN_NAME " +
                    "FROM " + _view.DBFPath + "\\REG.dbf as reg ORDER BY 2";
                comboBOXColumn_RGN.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                comboBOXColumn_RGN.DataSource = _dbfmanager.Execute(query);
                comboBOXColumn_RGN.ValueMember = "REG_RGN";
                comboBOXColumn_RGN.DisplayMember = "RGN_NAME";

                comboBOXColumn_RGN.DataPropertyName = "BNK_RGN";
                comboBOXColumn_RGN.HeaderText = "RGN_NAME";
                comboBOXColumn_RGN.Name = "RGN_NAME";
                comboBOXColumn_RGN.FlatStyle = FlatStyle.Flat;

                _view.BICTable.Columns.Insert(_view.BICTable.Columns["BNK_RGN"].Index + 1, comboBOXColumn_RGN);

                // Задаем список для комбобокса
                _view.ComboBoxSearch.Items.Clear();
                foreach (DataGridViewColumn col in _view.BICTable.Columns)
                {
                    if (col.ValueType != typeof(DateTime) && col.Name != "PZN_NAME" &&
                        col.Name != "TNP_FULLNAME" && col.Name != "UER_UERNAME" && col.Name != "RGN_NAME")
                    _view.ComboBoxSearch.Items.Add(col.Name);
                }
                _view.ComboBoxSearch.Items.Add("<ВСЕ>");
                _view.ComboBoxSearch.SelectedItem = "<ВСЕ>";

                _message.ShowMessage("Справочник БИК успешно открыт.");
            }
            catch (Exception ex)
            {
                _message.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Обработка события изменения строки поиска
        /// </summary>
        void _view_TextSearchChange(object sender, EventArgs e)
        {
            _view.TextBoxSearch.ForeColor = SystemColors.WindowText;

            string selectcol =  _view.ComboBoxSearch.SelectedItem.ToString();

            //_message.ShowMessage(selectcol);
            string strfilter = "";

            if (selectcol == "<ВСЕ>")
            {
                strfilter = "REAL like '%" + _view.TextSearch + "%'";

                foreach (DataGridViewColumn column in _view.BICTable.Columns)
                {
                    if (column.ValueType != typeof (DateTime) && column.Name != "PZN_NAME" &&
                        column.Name != "TNP_FULLNAME" && column.Name != "UER_UERNAME" && column.Name != "RGN_NAME")
                    {
                        strfilter += " OR " + column.HeaderText + " like '%" + _view.TextSearch + "%' ";
                    }
                    //_message.ShowMessage(column.ValueType.ToString());
                }
            }
            else
            {
                strfilter = selectcol + " like '%" + _view.TextSearch + "%'";
            }

            bicBindingSource.Filter = strfilter;
            if (bicBindingSource.Count == 0)
            {
                bicBindingSource.Filter = "";
                _view.TextBoxSearch.ForeColor = SystemColors.ScrollBar;
            }
        }

        /// <summary>
        /// Логика проверок для вводимых и изменяемых данных
        /// </summary>
        void _view_BICTableChange(object sender, EventArgs e)
        {
            if (_view.BICTable.CurrentCell != null)
            {
                string namecol = _view.BICTable.CurrentCell.OwningColumn.HeaderText;

                // проверяем введенное значение на пустоту, если верно, то возвращаем значение до изменения
                if (namecol == "BNK_UER" || namecol == "BNK_RGN" || namecol == "NAMEP" || namecol == "NEWNUM" ||
                    namecol == "DT_IZM" || namecol == "DATE_IN")
                {
                    string currentcellvalue = _view.BICTable.CurrentCell.Value.ToString();

                    if (string.IsNullOrEmpty(currentcellvalue))
                    {
                        _view.BICTable.CurrentCell.Value = _view.ValueCellBeforeEdit;
                        _message.ShowMessage("Поле " + namecol + " не может быть пустым " +
                                             (_view.BICTable.CurrentCell.RowIndex + 1) + ". Изменение будет отменено.");
                    }
                }

                // проверяем введенное значение на уникальность, если не верно, то возвращаем значение до изменения
                if (namecol == "NEWNUM")
                {
                    string currentcellvalue = _view.BICTable.CurrentCell.Value.ToString();

                    for (int i = 0; i < _view.BICTable.Rows.Count - 1; i++)
                    {
                        if (currentcellvalue == _view.BICTable[namecol, i].Value.ToString() &&
                            i != _view.BICTable.CurrentCell.RowIndex)
                        {
                            _view.BICTable.CurrentCell.Value = _view.ValueCellBeforeEdit;
                            _message.ShowMessage("Поле " + namecol +
                                                 " должно быть уникальным, найдено совпадение строка " + (i + 1) +
                                                 ". Изменение будет отменено.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обратботка события добавления строки, задание значений по умолчанию
        /// </summary>
        void _view_BICTableAddRow(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (e.RowIndex > 1)
            {
                DateTime date = DateTime.Now;
                _view.BICTable.Rows[e.RowIndex].Cells["BNK_UER"].Value = "0";
                _view.BICTable.Rows[e.RowIndex].Cells["BNK_RGN"].Value = "00";
                _view.BICTable.Rows[e.RowIndex].Cells["NAMEP"].Value = "Наименование участника расчетов 40 символов";
                _view.BICTable.Rows[e.RowIndex].Cells["NEWNUM"].Value =
                    (date.DayOfYear.ToString().Remove(2) + date.Hour.ToString() + date.Minute.ToString() + date.Second.ToString() +
                     date.Millisecond.ToString()).Remove(9);
                _view.BICTable.Rows[e.RowIndex].Cells["DT_IZM"].Value = "01.01.2222";
                _view.BICTable.Rows[e.RowIndex].Cells["DATE_IN"].Value = "01.01.2222";
            }
        }

        /// <summary>
        /// Получаем таблицу справочника БИК из выбранного каталога
        /// </summary>
        public DataTable GetBICTable(string folderPath)
        {
            if (IsExist(folderPath, "BNKSEEK.dbf") && IsExist(folderPath, "PZN.dbf") && IsExist(folderPath, "TNP.dbf") && IsExist(folderPath, "UER.dbf") && IsExist(folderPath, "REG.dbf"))
            {
                /*
                string query = "CREATE TABLE " + tmpbictable +
                               " (BIC_REAL VARCHAR(4), PZN_NAME VARCHAR(40), UERNAME VARCHAR(40), RGN_NAME VARCHAR(40), IND VARCHAR(6), " +
                               "FULLNAME VARCHAR(25), NNP VARCHAR(25), ADR VARCHAR(30), RKC VARCHAR(9), NAMEP VARCHAR(45), NEWNUM VARCHAR(9), " +
                               "TELEF VARCHAR(25), REGN VARCHAR(9), OKPO VARCHAR(8), DT_IZM DATETIME, KSNP VARCHAR(20), DATE_IN DATETIME, DATE_CH DATETIME)";
                _dbfmanager.Execute(query);
               
                query = "INSERT INTO " + tmpbictable + " \n" +*/
                string query = "SELECT bnk.REAL, bnk.PZN as BNK_PZN, bnk.UER as BNK_UER, bnk.RGN as BNK_RGN, bnk.IND, bnk.TNP as BNK_TNP, bnk.NNP, bnk.ADR, " +
                               "bnk.RKC, bnk.NAMEP, bnk.NEWNUM, bnk.TELEF, bnk.REGN, bnk.OKPO, bnk.DT_IZM, bnk.KSNP, bnk.DATE_IN, bnk.DATE_CH " +
                               "FROM " + folderPath + "\\BNKSEEK.dbf as bnk";

                _dbfmanager.Execute(query);

                //query = "SELECT * FROM " + tmpbictable;
                return _dbfmanager.Execute(query);
            }

            throw new Exception("В выбранном каталоге '" + folderPath + "' отсутствует одна из служебных таблиц:\nBNKSEEK.dbf, PZN.dbf, TNP.dbf, UER.dbf, REG.dbf.");
        }

        /// <summary>
        /// Проверка существования файла в каталоге, возврат bool
        /// </summary>
        public bool IsExist(string folderPath, string fileName)
        {
            bool isExist = File.Exists(folderPath + "\\" + fileName);
            return isExist;
        }
    }
}
