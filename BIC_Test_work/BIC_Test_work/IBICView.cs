using System;
using System.Windows.Forms;

namespace BIC_Test_work
{
    public interface IBICView
    {
        string DBFPath { get; }
        string SaveFilePath { get; }
        DataGridView BICTable { get; set; }
        BindingNavigator BICViewNavigator { get; set; }
        string TextSearch { get; }
        TextBox TextBoxSearch { get; set; }
        ComboBox ComboBoxSearch { get; set; }

        string ValueCellBeforeEdit { get; }

        event EventHandler OpenClick;
        event EventHandler SaveClick;
        event EventHandler BICTableChange;
        event EventHandler TextSearchChange;
        event EventHandler ComboBoxSearchClick;
        event DataGridViewRowsAddedEventHandler BICTableAddRow;
        //event DataGridViewCellCancelEventHandler CellBeginEdit;
    }
}
