using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;

namespace malmo
{
    public partial class search : System.Web.UI.Page
    {
        luceneIndex searcher = new luceneIndex();
        string index = "public";
        bool komin = false;
        string searchString = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["index"]))
            {
                index = HttpUtility.UrlDecode(Request.QueryString.GetValues("index").GetValue(0).ToString());
                if (index == "komin")
                {
                    komin = true;
                }
            }

            if (!string.IsNullOrEmpty(Request.QueryString["action"]))
            {
                string action = HttpUtility.UrlDecode(Request.QueryString.GetValues("action").GetValue(0).ToString());
                if (action == "build")
                {
                    indexMessage message = new indexMessage();
                    searcher.buildIndex(message,komin);
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ContentType = "application/json";
                    Response.Charset = "UTF-8";
                    Response.ContentEncoding = System.Text.Encoding.UTF8;
                    Response.Write(JsonConvert.SerializeObject(message));
                    Response.End();
                }
            }

            if (!string.IsNullOrEmpty(Request.QueryString["query"]))
            {
                searchString = HttpUtility.UrlDecode(Request.QueryString.GetValues("query").GetValue(0).ToString());
            }

            if (!string.IsNullOrEmpty(searchString))
            {

                Response.Clear();
                Response.ClearHeaders();
                Response.ContentType = "application/json";
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.Write(doSearch(searchString,komin));
                Response.End();
            }
            else
            {
                Response.Clear();
                Response.ClearHeaders();
                Response.ContentType = "application/json";
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.Write("[]");
                Response.End();
            }

        }

        private string doSearch(string query, bool komin)
        {
            List<indexVideo> articles = searcher.searchArticles(query, 100,komin);
            string response = JsonConvert.SerializeObject(articles);
            return response;
        }
    }
}