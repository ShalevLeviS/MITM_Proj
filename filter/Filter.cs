using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSNA
{
    class Filter
    {
        private Form1 f1;
        private string url;

        public Filter(string url,Form1 f1)
        {
            this.f1 = f1;
            this.url = url;
            CheckUrls();
        }

        public void CheckUrls()
        {

            string text = System.IO.File.ReadAllText(@"G:\\avoda\\CSNA COPY\\CSNA\\Properties\\Url Blacklist.txt");

            List<string> sites = text.Split('$').ToList<string>();
            
            foreach (string urlx in sites)
            {
                if (url.Contains(urlx))
                {
                    Monitoring(true);
                    break;
                }
            }
            Monitoring(false);
        }

        public void Monitoring(bool check)
        {
            
            if (check == true)
            {
                string[] row = { url, "Yes"};
                var listViewItem = new ListViewItem(row);
                f1.MonitorLog.Items.Add(listViewItem);
            }
            else
            {
                string[] row = { url, "No" };
                var listViewItem = new ListViewItem(row);
                f1.MonitorLog.Items.Add(listViewItem);
            }
        }
    }
}
