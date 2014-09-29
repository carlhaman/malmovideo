using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Caching;

namespace malmo
{
    public class luceneIndex
    {
        DirectoryInfo publicIndexDirectory = Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("/index/public"));
        DirectoryInfo kominIndexDirectory = Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("/index/komin"));

        Lucene.Net.Store.Directory publicIndex = Lucene.Net.Store.FSDirectory.Open(System.Web.HttpContext.Current.Server.MapPath("/index/public"));
        Lucene.Net.Store.Directory kominIndex = Lucene.Net.Store.FSDirectory.Open(System.Web.HttpContext.Current.Server.MapPath("/index/komin"));

        Lucene.Net.Analysis.Analyzer analyser = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

        Cache _cache = System.Web.HttpContext.Current.Cache;

        private videoArchive publicArchive;
        private videoArchive kominArchive;

        public indexMessage buildIndex(indexMessage message, bool komin)
        {
            if (komin)
            {
                if (kominArchive == null) { kominArchive = getVideoArchive(true); }
                createIndex(kominArchive, message, kominIndex);
            }
            else
            {
                if (publicArchive == null) { publicArchive = getVideoArchive(false); }
                createIndex(publicArchive, message, publicIndex);
            }
            return message;
        }

        private videoArchive getVideoArchive(bool komin)
        {
            buildVideoArchive builder = new malmo.buildVideoArchive();

            videoArchive archive = new videoArchive();
            if (komin)
            {
                archive = (videoArchive)_cache["kominArchive"];
                if (archive == null)
                {
                    archive = builder.render(true);
                    _cache.Insert("kominArchive", archive, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
                }
            }
            else
            {
                archive = (videoArchive)_cache["Archive"];
                if (archive == null)
                {
                    archive = builder.render(false);
                    _cache.Insert("Archive", archive, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
                }
            }

            return archive;
        }

        public luceneIndex()
        {

        }

        private void createIndex(videoArchive archive, indexMessage message, Lucene.Net.Store.Directory index)
        {

            Lucene.Net.Index.IndexWriter writer;

            try
            {
                writer = new Lucene.Net.Index.IndexWriter(index, analyser, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);
                foreach (videoCategory cat in archive.categories)
                {
                    foreach (videoItem video in cat.videos)
                    {
                        string title = "";
                        if (!string.IsNullOrEmpty(video.name)) { title = video.name; }
                        string bctid = "";
                        if (!string.IsNullOrEmpty(video.id)) { bctid = video.id; }
                        string shortDescription = "";
                        if (!string.IsNullOrEmpty(video.shortDescription)) { shortDescription = video.shortDescription; }
                        string imageURL = "";
                        if (!string.IsNullOrEmpty(video.thumbnailURL)) { imageURL = video.thumbnailURL; }
                        if (!string.IsNullOrEmpty(video.videoStillURL)) { imageURL = video.videoStillURL; }
                        addVideoToIndex(bctid, title, shortDescription, imageURL, writer, message, index);
                    }
                }
                message.success = true;
                writer.Optimize();
                message.message = "Index built and optimized!";
                writer.Dispose();
            }

            catch (Exception ex)
            {
                message.success = false;
                message.message = ex.Message;
            }

        }

        private bool videoExistsInIndex(string id, Lucene.Net.Store.Directory index)
        {
            bool exist = false;
            Lucene.Net.Search.TermQuery termQuery = new Lucene.Net.Search.TermQuery(new Lucene.Net.Index.Term("bctid", id));
            Lucene.Net.Search.Searcher termSearcher = new Lucene.Net.Search.IndexSearcher(index, true);

            Lucene.Net.Search.TopScoreDocCollector termCollector = Lucene.Net.Search.TopScoreDocCollector.Create(1, true);
            termSearcher.Search(termQuery, termCollector);
            int termResults = termCollector.TopDocs().TotalHits;

            if (termResults > 0) { exist = true; }
            return exist;
        }

        private void addVideoToIndex(string bctid, string title, string shortDescription, string imageURL, Lucene.Net.Index.IndexWriter writer, indexMessage message, Lucene.Net.Store.Directory index)
        {
            Lucene.Net.Documents.Document video = new Lucene.Net.Documents.Document();

            video.Add(new Lucene.Net.Documents.Field("title", title, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.YES));
            video.Add(new Lucene.Net.Documents.Field("shortDescription", shortDescription, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.YES));
            video.Add(new Lucene.Net.Documents.Field("bctid", bctid, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));
            video.Add(new Lucene.Net.Documents.Field("imageURL", imageURL, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED));

            if (videoExistsInIndex(bctid, index))
            {
                writer.UpdateDocument(new Lucene.Net.Index.Term("bctid", video.Get("bctid")), video);
                message.updatedVideo++;
            }
            else
            {
                writer.AddDocument(video);
                message.newVideo++;
            }

        }
        public List<indexVideo> searchArticles(string queryString, int numberOfResults, bool komin)
        {
            List<indexVideo> resultsList = new List<indexVideo>();
            Lucene.Net.Store.Directory index = publicIndex;
            if (komin) { index = kominIndex; }

            if (!string.IsNullOrEmpty(queryString))
            {

                Lucene.Net.QueryParsers.QueryParser parser = new Lucene.Net.QueryParsers.MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, new[] { "title", "shortDescription" }, analyser);
                try
                {
                    Lucene.Net.Search.Query query = parser.Parse(queryString + "~");

                    Lucene.Net.Search.Searcher searcher = new Lucene.Net.Search.IndexSearcher(index, true);

                    Lucene.Net.Search.TopScoreDocCollector collector = Lucene.Net.Search.TopScoreDocCollector.Create(numberOfResults, true);

                    searcher.Search(query, collector);

                    Lucene.Net.Search.ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

                    if (hits.Length >= 1)
                    {
                        for (int i = 0; i < hits.Length; i++)
                        {
                            indexVideo video = new indexVideo();

                            int docId = hits[i].Doc;
                            float score = hits[i].Score;

                            Lucene.Net.Documents.Document doc = searcher.Doc(docId);

                            video.bctid = doc.Get("bctid");
                            video.title = doc.Get("title");
                            video.score = score;
                            video.shortDescription = doc.Get("shortDescription");
                            video.imageURL = doc.Get("imageURL");

                            resultsList.Add(video);
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
            return resultsList;
        }
    }

    public class indexVideo
    {
        public string bctid { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }
        public string imageURL { get; set; }
        public float score { get; set; }
    }
    public class indexMessage
    {
        public indexMessage()
        {
            newVideo = 0;
            updatedVideo = 0;
            success = false;
            message = string.Empty;
        }
        public int newVideo { get; set; }
        public int updatedVideo { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}