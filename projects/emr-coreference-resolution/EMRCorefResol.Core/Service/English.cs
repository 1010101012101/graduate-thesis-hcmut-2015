﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace HCMUT.EMRCorefResol.Service
{
    using System.IO;
    using System.Net;
    using System.Xml;
    using Utilities;
    public static class English
    {
        private const string API_URL = "http://localhost:8181/api/";
        private static readonly HttpUtil _http = new HttpUtil();
        private static readonly WikiUltil _wiki = new WikiUltil();
        private static readonly UMLSUtil _umls = new UMLSUtil();

        private static ICache<string, string> _temporalCache;
        private static ICache<EMR, int> _mostGenderCache;
        private static ICache<string, Definition[]> _wordnetCache;
        private static ICache<string, WikiData> _wikiCache;
        private static ICache<string, UMLSData> _umlsCache;

        static English()
        {
            _temporalCache = new UnlimitedCache<string, string>();
            _mostGenderCache = new UnlimitedCache<EMR, int>();
            _wordnetCache = new UnlimitedCache<string, Definition[]>();
            _wikiCache = new UnlimitedCache<string, WikiData>();
            _umlsCache = new UnlimitedCache<string, UMLSData>();

        }

        public static void ClearCache()
        {
            _temporalCache.Clear();
            _mostGenderCache.Clear();
            _wordnetCache.Clear();
            _wikiCache.Clear();
            _umlsCache.Clear();
        }

        public static string[] POSTag(string term)
        {
            var url = API_URL + "nlp/pos?term=" + HttpUtility.UrlEncode(term);
            var res = _http.Request(url);

            if (!res.IsSuccess)
            {
                return null;
            }

            return ((System.Collections.IEnumerable)res.Data)
              .Cast<object>()
              .Select(x => x.ToString())
              .ToArray();
        }

        public static string[] Tokenize(string term)
        {
            var url = API_URL + "nlp/token?term=" + HttpUtility.UrlEncode(term);
            var res = _http.Request(url);

            if (!res.IsSuccess)
            {
                return null;
            }

            return ((System.Collections.IEnumerable)res.Data)
              .Cast<object>()
              .Select(x => x.ToString())
              .ToArray();
        }

        public static Definition[] GetSyncSets(string term)
        {
            return _wordnetCache.GetValue(term, (string search_term) =>
            {
                var url = API_URL + "dictionary/synsets?term=" + HttpUtility.UrlEncode(search_term);
                var res = _http.Request(url);

                if (!res.IsSuccess)
                {
                    return null;
                }

                var json = res.Data.ToString();
                return JsonConvert.DeserializeObject<Definition[]>(json);
            });
        }

        public static string[] GetChunks(string term)
        {
            var url = API_URL + "nlp/chunk?term=" + HttpUtility.UrlEncode(term);
            var res = _http.Request(url);

            if (!res.IsSuccess)
            {
                return null;
            }

            return ((System.Collections.IEnumerable)res.Data)
              .Cast<object>()
              .Select(x => x.ToString())
              .ToArray();
        }

        public static string GetHeadNoun(string term)
        {
            var url = API_URL + "nlp/headnoun?term=" + HttpUtility.UrlEncode(term);
            var res = _http.Request(url);

            if (!res.IsSuccess)
            {
                return null;
            }

            return (string)res.Data;
        }

        public static string GetTemporalValue(string emrPath, string line, string section)
        {
            return _temporalCache.GetValue(line, (string input_line) =>
            {
                var url = API_URL + "extractor/temporal";

                var data = new Dictionary<string, string>();
                data.Add("Path", emrPath);
                data.Add("Line", line);
                data.Add("Section", section);

                var res = _http.PostData(url, data);

                if (!res.IsSuccess)
                {
                    return null;
                }

                return (string)res.Data;
            });
        }

        public static int GetMostGender(EMR emr)
        {
            return _mostGenderCache.GetValue(emr, (EMR e) =>
            {
                var he_searcher = new AhoCorasickKeywordDictionary(new string[] { "he", "him", "his", "himself" });
                var she_searcher = new AhoCorasickKeywordDictionary(new string[] { "she", "her", "hers", "herself" });

                int isHe = 0;
                foreach (Concept c in emr.Concepts)
                {
                    if (he_searcher.Match(c.Lexicon, KWSearchOptions.IgnoreCase | KWSearchOptions.WholeWord))
                    {
                        isHe++;
                    }

                    if (she_searcher.Match(c.Lexicon, KWSearchOptions.IgnoreCase | KWSearchOptions.WholeWord))
                    {
                        isHe--;
                    }
                }
                return isHe >= 0 ? 0 : 1;
            });
        }

        public static WikiData GetAllWikiInformation(string term)
        {
            return _wikiCache.GetValue(term, (string search_term) =>
            {
                var result = _wiki.GetPageInfoByTitle(search_term);
                if(result == null)
                {
                    result = _wiki.GetPageInfoByTitle(search_term.ToUpper());
                }

                if(result == null)
                {
                    result = _wiki.QueryPageInfo(search_term);
                }

                if(result == null)
                {
                    result = _wiki.QueryPageInfo(search_term.ToUpper());
                }

                return result;
            });
        }

        public static UMLSData GetUMLSInformation(string term)
        {
            return _umlsCache.GetValue(term, (string search_term) =>
            {
                return _umls.GetUMLSInfo(search_term);
            });
        }

        public static UMLSData GetUMLSInformation(string term, int options)
        {
            return _umls.GetUMLSInfo(term, options);
        }
    }
}
