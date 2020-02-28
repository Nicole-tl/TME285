using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HumanLanguageLibrary.Phrases;

namespace HumanLanguageLibrary.Visualization
{
    public partial class PhrasePairListDataGridView : DataGridView
    {
        private const int SCROLLBAR_WIDTH = 25;

        private PhraseDictionary phraseDictionary = null;
        private DataGridViewCellStyle standardCellStyle;
        private DataGridViewCellStyle oneSidedCellStyle;
        private Boolean showCompact = true;

        public PhrasePairListDataGridView()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            this.BorderStyle = BorderStyle.None;
            standardCellStyle = new DataGridViewCellStyle();
            standardCellStyle.Font = new Font("Lucida Console", 9);
            standardCellStyle.ForeColor = Color.Lime;
            standardCellStyle.BackColor = Color.Black;
            oneSidedCellStyle = new DataGridViewCellStyle();
            oneSidedCellStyle.Font = new Font("Lucida Console", 9);
            oneSidedCellStyle.ForeColor = Color.LightSkyBlue;
            oneSidedCellStyle.BackColor = Color.Black;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.Columns.Count < 3) { return; }
            this.Columns[1].Width = 30;
            int remainingWidth = this.Width - this.Columns[1].Width - SCROLLBAR_WIDTH;
            this.Columns[0].Width = remainingWidth / 2;
            this.Columns[2].Width = remainingWidth / 2;
        }

        private void InitializeView()
        {
            this.Rows.Clear();
            this.Columns.Clear();
            this.Columns.Add("phrase1Column", "Phrase 1");
            this.Columns.Add("operatorColumn", "Op");
            this.Columns.Add("phrase2Column", "Phrase 2");
            this.RowHeadersVisible = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AllowUserToOrderColumns = false;
            this.AllowUserToResizeColumns = true;
            this.AllowUserToResizeRows = false;
            this.Columns[1].Width = 30;
            int remainingWidth = this.Width - this.Columns[1].Width - SCROLLBAR_WIDTH;
            this.Columns[0].Width = remainingWidth / 2;
            this.Columns[2].Width = remainingWidth / 2;
        }

        private void ShowItems()
        {
            this.Rows.Clear();
            if (showCompact)
            {
                foreach (PhraseDictionaryItem item in phraseDictionary.CompactItemList)
                {
                    string operatorString = "";
                    if (item.OneSidedInterchangeability) { operatorString = " =>"; }
                    else { operatorString = "<=>"; }
                    string[] information = new string[] { item.Phrase1, operatorString, item.Phrase2 };
                    this.Rows.Add(information);
                    if (item.OneSidedInterchangeability) { this.Rows[this.Rows.Count - 1].Cells[1].Style = oneSidedCellStyle; }
                    else { this.Rows[this.Rows.Count - 1].Cells[1].Style = standardCellStyle; }
                    this.Rows[this.Rows.Count - 1].Cells[0].Style = standardCellStyle;
                    this.Rows[this.Rows.Count - 1].Cells[2].Style = standardCellStyle;
                    this.Rows[this.Rows.Count - 1].Tag = item;
                }
            }
            else
            {
                foreach (PhraseDictionaryItem item in phraseDictionary.ItemList)
                {
                    string operatorString = "";
                    if (item.OneSidedInterchangeability) { operatorString = " =>"; }
                    else { operatorString = "<=>"; }
                    string[] information = new string[] { item.Phrase1, operatorString, item.Phrase2 };
                    this.Rows.Add(information);
                    if (item.OneSidedInterchangeability) { this.Rows[this.Rows.Count - 1].Cells[1].Style = oneSidedCellStyle; }
                    else { this.Rows[this.Rows.Count - 1].Cells[1].Style = standardCellStyle; }
                    this.Rows[this.Rows.Count - 1].Cells[0].Style = standardCellStyle;
                    this.Rows[this.Rows.Count - 1].Cells[2].Style = standardCellStyle;
                    this.Rows[this.Rows.Count - 1].Tag = item;
                }
            }
        }

        public void SetPhraseDictionary(PhraseDictionary phraseDictionary, Boolean showCompact)
        {
            this.showCompact = showCompact;
            InitializeView();
            this.phraseDictionary = phraseDictionary;
            ShowItems();
        }
    }
}
