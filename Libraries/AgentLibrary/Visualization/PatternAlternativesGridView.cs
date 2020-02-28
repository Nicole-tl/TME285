using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgentLibrary.IO;
using AgentLibrary.Patterns;

namespace AgentLibrary.Visualization
{
    public partial class PatternAlternativesGridView : DataGridView
    {
        public PatternAlternativesGridView()
        {
            InitializeComponent();
        }

        public void SetPattern(Pattern pattern)
        {
            pattern.Initialize();
            List<string> allAlternatives = pattern.GetAllPatternAlternativesAsString();
            List<List<string>> allAlternativesSplitList = pattern.GetAllPatternAlternativesAsStringList();
            int numberOfColumns = 0;
            for (int ii = 0; ii < allAlternativesSplitList.Count; ii++)
            {
                List<string> alternativeSplit = allAlternativesSplitList[ii];
                if (alternativeSplit.Count > numberOfColumns) { numberOfColumns = alternativeSplit.Count; }
            }
            this.Rows.Clear();
            this.Columns.Clear();
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            for (int ii = 0; ii < numberOfColumns; ii++)
            {
                this.Columns.Add("Column" + ii.ToString(), ii.ToString());
            }
            for (int ii = 0; ii < allAlternatives.Count; ii++)
            {
                this.Rows.Add();
            }
            for (int ii = 0; ii < allAlternativesSplitList.Count; ii++)
            {
                List<string> alternativeSplit = allAlternativesSplitList[ii];
                for (int kk = 0; kk < alternativeSplit.Count; kk++)
                {
                    this.Rows[ii].Cells[kk].Value = alternativeSplit[kk];
                }
            }
        }
    }
}
