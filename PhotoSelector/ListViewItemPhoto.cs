using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSelector
{
    public class ListViewItemPhoto: ListViewItem
    {
        public string FilePath { get; set; }
        public DateTime Date { get; set; }
    }
}
