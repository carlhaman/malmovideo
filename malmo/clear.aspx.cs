using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace malmo
{
    public partial class clear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string html = string.Empty;
            html += "\n<h2>Cache:</h2>";
            if (Cache["Archive"] != null) { html += "\n<p>Video archive is cached</p>"; }

            if (Cache["kominArchive"] != null) { html += "\n<p>KOMIN Video archive is cached</p>"; }

            html += "\n<hr/>";

            html += "\n<h2>Video caches cleared:</h>";

            if (Cache["Archive"] != null)
            {
                Cache.Remove("Archive");
                html += "\n<p>Video archive cache is removed!</p>";
            }
            if (Cache["kominArchive"] != null)
            {
                Cache.Remove("kominArchive");
                html += "\n<p>KOMIN Video archive cache is removed!</p>";
            }

            pageContent.InnerHtml = html;
        }
    }
}