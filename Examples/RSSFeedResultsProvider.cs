﻿/*
Copyright (c) 2010 Stephen Ward and Joseph E Feser

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Windows.Controls.Custom;

namespace Examples {

    public class RSSFeedResultsProvider : ISearchResultsProvider {

        private XmlDataDocument _XmlDoc;

        private XmlDataDocument XmlDoc {
            get {
                if (_XmlDoc == null) {
                    try {
                        //we are going to show how we can use an rss feed to obtain data and then filter based on the name
                        string url = "http://maps.yahoo.com/traffic.rss?csz=10101&mag=5&minsev=1";

                        WebRequest req = WebRequest.Create(url);
                        WebResponse res = req.GetResponse();

                        Stream rsstream = res.GetResponseStream();
                        System.Xml.XmlDataDocument rssdoc = new System.Xml.XmlDataDocument();

                        rssdoc.Load(rsstream);
                        _XmlDoc = rssdoc;
                    }
                    catch (Exception) {
                        //do nothing, bad stuff happened.
                    }
                }
                return _XmlDoc;
            }
        }

        private List<Result> getRSSfeed() {

            List<Result> retVal = new List<Result>();

            System.Xml.XmlNodeList rssitems = XmlDoc.SelectNodes("rss/channel/item");

            for (int i = 0; i < rssitems.Count; i++) {
                System.Xml.XmlNode rssdetail;

                rssdetail = rssitems.Item(i).SelectSingleNode("title");
                if (rssdetail != null) {
                    var result = new Result();
                    retVal.Add(result);

                    result.Title = rssdetail.InnerText;

                    rssdetail = rssitems.Item(i).SelectSingleNode("description");
                    result.Description = rssdetail.InnerText;

                    rssdetail = rssitems.Item(i).SelectSingleNode("link");


                    result.Link = (rssdetail != null) ? rssdetail.InnerText : "";

                    rssdetail = rssitems.Item(i).SelectSingleNode("category");
                    result.Category = (rssdetail != null) ? rssdetail.InnerText : "";

                    rssdetail = rssitems.Item(i).SelectSingleNode("severity");
                    result.Severity = (rssdetail != null) ? rssdetail.InnerText : "";

                }
            }
            return retVal;
        }

        #region ISearchResultsProvider Members

        public void BeginSearchAsync(string searchTerm, DateTime startTimeUtc, int maxResults, 
            object extraInfo, Action<DateTime, IEnumerable<object>> whenDone) {

            var results = getRSSfeed();

            whenDone(startTimeUtc, results
                .Where(term => term.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) > -1
                || term.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) > -1)
                .Take(maxResults).Cast<object>());
        }

        public void CancelAllSearches() {
            //throw new NotImplementedException();
        }

        #endregion

        private class Result {

            public string Title {
                get;
                set;
            }

            public string Description {
                get;
                set;
            }

            public string Link {
                get;
                set;
            }

            public string Category {
                get;
                set;
            }

            public string Severity {
                get;
                set;
            }

            public override string ToString() {
                return Title;
            }
        }
    }
}