/* Copyright (C) 2014 Robin Hermanussen

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see http://www.gnu.org/licenses/. */
using System;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Web;

namespace SitecoreHtmlCrawler.ComputedFields
{
    /// <summary>
    /// Computed field that requests a page and indexes the content.
    /// Because it actually 'http requests' the page, the content of the components on the page are also indexed.
    /// </summary>
    public class HtmlCrawledField : IComputedIndexField
    {
        public string FieldName { get; set; }
        
        public string ReturnType { get; set; }

        public object ComputeFieldValue(IIndexable indexable)
        {
            Assert.ArgumentNotNull(indexable, "indexable");
            string url = null;
            try
            {
                Item item = indexable as SitecoreIndexableItem;

                // This field only works for items uder /sitecore/content that have a layout
                if (item == null
                    || item.Visualization.Layout == null
                    || ! item.Paths.FullPath.StartsWith(
                            Sitecore.Constants.ContentPath,
                            StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                // Determine the url to request
                using (new DatabaseSwitcher(item.Database))
                {
                    url = WebUtil.AddQueryString(
                        LinkManager.GetItemUrl(item, new UrlOptions()
                            {
                                AlwaysIncludeServerUrl = true
                            }),
                        "sc_database", Sitecore.Context.Database.Name);
                }

                // Http request the page
                using (var client = new WebClient())
                {
                    string pageContent = client.DownloadString(url);

                    // Parse the page's html using HtmlAgilityPack
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(pageContent);

                    // Strip out all the html tags, so we can index just the text
                    HtmlNode mainContainer = htmlDocument.DocumentNode.Descendants("body").FirstOrDefault();
                    string content = mainContainer != null ? GetAllInnerTexts(mainContainer) : null;
                    return content;
                }
            }
            catch (WebException webExc)
            {
                Log.Warn(string.Format("Failed to html index {0} ({1}): {2}", indexable.Id, url, webExc.Message), webExc, this);
            }
            catch (Exception exc)
            {
                Log.Error(string.Format("An error occurred when indexing {0}: {1}", indexable.Id, exc.Message), exc, this);
            }
            return null;
        }

        /// <summary>
        /// Find all inner texts and return a simplified string.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected virtual string GetAllInnerTexts(HtmlNode node)
        {
            return RemoveWhitespace(string.Join(" ", node.DescendantsAndSelf()
                    .Select(d => d.InnerText.Replace(Environment.NewLine, " ")))).Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Storing whitespace for a field that is only to be used for searching in is not very useful.
        /// This methods removes excessive whitespace.
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        private static string RemoveWhitespace(string inputStr)
        {
            const int n = 5;
            StringBuilder tmpbuilder = new StringBuilder(inputStr.Length);
            for (int i = 0; i < n; ++i)
            {
                string scopy = inputStr;
                bool inspaces = false;
                tmpbuilder.Length = 0;
                for (int k = 0; k < inputStr.Length; ++k)
                {
                    char c = scopy[k];
                    if (inspaces)
                    {
                        if (c != ' ')
                        {
                            inspaces = false;
                            tmpbuilder.Append(c);
                        }
                    }
                    else if (c == ' ')
                    {
                        inspaces = true;
                        tmpbuilder.Append(' ');
                    }
                    else
                    {
                        tmpbuilder.Append(c);
                    }
                }
            }
            return tmpbuilder.ToString();
        }
    }
}
