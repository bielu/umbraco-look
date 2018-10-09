﻿using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Highlight;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Tier;
using Lucene.Net.Spatial.Tier.Projectors;
using Our.Umbraco.Look.Interfaces;
using Our.Umbraco.Look.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core.Logging;
using UmbracoExamine;

namespace Our.Umbraco.Look.Services
{
    public static class LookSearchService
    {
        /// <summary>
        ///  Main searching method
        /// </summary>
        /// <param name="lookQuery"></param>
        /// <returns>an IEnumerableWithTotal</returns>
        public static IEnumerableWithTotal<LookMatch> Query(LookQuery lookQuery)
        {
            IEnumerableWithTotal<LookMatch> lookMatches = null; // prepare return value

            if (lookQuery == null)
            {
                LogHelper.Warn(typeof(LookService), "Supplied search query was null");
            }
            else
            {
                var searchProvider = LookService.Searcher;

                var searchCriteria = searchProvider.CreateSearchCriteria();

                var query = searchCriteria.Field(string.Empty, string.Empty);

                // Text
                if (lookQuery.TextQuery != null)
                {
                    if (!string.IsNullOrWhiteSpace(lookQuery.TextQuery.SearchText))
                    {
                        if (lookQuery.TextQuery.Fuzzyness > 0)
                        {
                            query.And().Field(LookService.TextField, lookQuery.TextQuery.SearchText.Fuzzy(lookQuery.TextQuery.Fuzzyness));
                        }
                        else
                        {
                            query.And().Field(LookService.TextField, lookQuery.TextQuery.SearchText);
                        }
                    }
                }

                // Tags
                if (lookQuery.TagQuery != null)
                {
                    var allTags = new List<string>();
                    var anyTags = new List<string>();

                    if (lookQuery.TagQuery.AllTags != null)
                    {
                        allTags.AddRange(lookQuery.TagQuery.AllTags);
                        allTags.RemoveAll(x => string.IsNullOrWhiteSpace(x));
                    }

                    if (lookQuery.TagQuery.AnyTags != null)
                    {
                        anyTags.AddRange(lookQuery.TagQuery.AnyTags);
                        anyTags.RemoveAll(x => string.IsNullOrWhiteSpace(x));
                    }

                    if (allTags.Any())
                    {
                        query.And().GroupedAnd(allTags.Select(x => LookService.TagsField), allTags.ToArray());
                    }

                    if (anyTags.Any())
                    {
                        query.And().GroupedOr(allTags.Select(x => LookService.TagsField), anyTags.ToArray());
                    }
                }

                // TODO: Date

                // TODO: Name

                // Nodes
                if (lookQuery.NodeQuery != null)
                {
                    if (lookQuery.NodeQuery.TypeAliases != null)
                    {
                        var typeAliases = new List<string>();

                        typeAliases.AddRange(lookQuery.NodeQuery.TypeAliases);
                        typeAliases.RemoveAll(x => string.IsNullOrWhiteSpace(x));

                        if (typeAliases.Any())
                        {
                            query.And().GroupedOr(typeAliases.Select(x => UmbracoContentIndexer.NodeTypeAliasFieldName), typeAliases.ToArray());
                        }
                    }

                    if (lookQuery.NodeQuery.ExcludeIds != null)
                    {
                        foreach (var excudeId in lookQuery.NodeQuery.ExcludeIds.Distinct())
                        {
                            query.Not().Id(excudeId);
                        }
                    }
                }

                try
                {
                    searchCriteria = query.Compile();
                }
                catch (Exception exception)
                {
                    LogHelper.WarnWithException(typeof(LookService), "Could not compile the Examine query", exception);
                }

                if (searchCriteria != null && searchCriteria is LuceneSearchCriteria)
                {
                    Sort sort = null;
                    Filter filter = null;

                    Func<int, double?> getDistance = x => null;
                    Func<string, IHtmlString> getHighlight = null;

                    TopDocs topDocs = null;

                    switch (lookQuery.SortOn)
                    {
                        case SortOn.Date: // newest -> oldest
                            sort = new Sort(new SortField(LuceneIndexer.SortedFieldNamePrefix + LookService.DateField, SortField.LONG, true));
                            break;

                        case SortOn.Name: // a -> z
                            sort = new Sort(new SortField(LuceneIndexer.SortedFieldNamePrefix + LookService.NameField, SortField.STRING));
                            break;
                    }

                    if (lookQuery.LocationQuery != null && lookQuery.LocationQuery.Location != null)
                    {
                        double maxDistance = LookService.MaxDistance;

                        if (lookQuery.LocationQuery.MaxDistance != null)
                        {
                            maxDistance = Math.Min(lookQuery.LocationQuery.MaxDistance.GetMiles(), maxDistance);
                        }

                        var distanceQueryBuilder = new DistanceQueryBuilder(
                                                    lookQuery.LocationQuery.Location.Latitude,
                                                    lookQuery.LocationQuery.Location.Longitude,
                                                    maxDistance,
                                                    LookService.LocationField + "_Latitude",
                                                    LookService.LocationField + "_Longitude",
                                                    CartesianTierPlotter.DefaltFieldPrefix,
                                                    true);

                        // update filter
                        filter = distanceQueryBuilder.Filter;

                        if (lookQuery.SortOn == SortOn.Distance)
                        {
                            // update sort
                            sort = new Sort(
                                        new SortField(
                                            LookService.DistanceField,
                                            new DistanceFieldComparatorSource(distanceQueryBuilder.DistanceFilter)));
                        }

                        // raw data for the getDistance func
                        var distances = distanceQueryBuilder.DistanceFilter.Distances;

                        // update getDistance func
                        getDistance = new Func<int, double?>(x =>
                        {
                            if (distances.ContainsKey(x))
                            {
                                return distances[x];
                            }

                            return null;
                        });
                    }

                    var indexSearcher = new IndexSearcher(((LuceneIndexer)LookService.Indexer).GetLuceneDirectory(), false);

                    var luceneSearchCriteria = (LuceneSearchCriteria)searchCriteria;

                    // do the Lucene search
                    topDocs = indexSearcher.Search(
                                                luceneSearchCriteria.Query, // the query build by Examine
                                                filter ?? new QueryWrapperFilter(luceneSearchCriteria.Query),
                                                LookService.MaxLuceneResults,
                                                sort ?? new Sort(SortField.FIELD_SCORE));

                    if (topDocs.TotalHits > 0)
                    {
                        // setup the getHightlight func if required
                        if (lookQuery.TextQuery.HighlightFragments > 0 && !string.IsNullOrWhiteSpace(lookQuery.TextQuery.SearchText))
                        {
                            var version = Lucene.Net.Util.Version.LUCENE_29;

                            Analyzer analyzer = new StandardAnalyzer(version);

                            var queryParser = new QueryParser(version, LookService.TextField, analyzer);

                            var queryScorer = new QueryScorer(queryParser
                                                                .Parse(lookQuery.TextQuery.SearchText)
                                                                .Rewrite(indexSearcher.GetIndexReader()));

                            Highlighter highlighter = new Highlighter(new SimpleHTMLFormatter("<strong>", "</strong>"), queryScorer);

                            // update the getHightlight func
                            getHighlight = (x) =>
                            {
                                var tokenStream = analyzer.TokenStream(LookService.TextField, new StringReader(x));

                                var highlight = highlighter.GetBestFragments(
                                                                tokenStream,
                                                                x,
                                                                lookQuery.TextQuery.HighlightFragments, // max number of fragments
                                                                lookQuery.TextQuery.HighlightSeparator); // fragment separator

                                return new HtmlString(highlight);
                            };
                        }

                        lookMatches = new EnumerableWithTotal<LookMatch>(
                                                    LookSearchService.GetLookMatches(
                                                                        lookQuery,
                                                                        indexSearcher,
                                                                        topDocs,
                                                                        getHighlight,
                                                                        getDistance),
                                                    topDocs.TotalHits);
                    }
                }
            }

            return lookMatches ?? new EnumerableWithTotal<LookMatch>(Enumerable.Empty<LookMatch>(), 0);
        }

        /// <summary>
        /// Supplied with the result of a Lucene query, this method will yield a constructed LookMatch for each in order
        /// </summary>
        /// <param name="indexSearcher">The searcher supplied to get the Lucene doc for each id in the Lucene results (topDocs)</param>
        /// <param name="topDocs">The results of the Lucene query (a collection of ids in an order)</param>
        /// <param name="getHighlight">Function used to get the highlight text for a given result text</param>
        /// <param name="getDistance">Function used to calculate distance (if a location was supplied in the original query)</param>
        /// <returns></returns>
        private static IEnumerable<LookMatch> GetLookMatches(
                                                    LookQuery lookQuery,
                                                    IndexSearcher indexSearcher,
                                                    TopDocs topDocs,
                                                    Func<string, IHtmlString> getHighlight,
                                                    Func<int, double?> getDistance)
        {
            bool getText = lookQuery.TextQuery != null && lookQuery.TextQuery.GetText;
            bool getTags = lookQuery.TagQuery != null && lookQuery.TagQuery.GetTags;

            var fields = new List<string>();

            fields.Add(LuceneIndexer.IndexNodeIdFieldName); // "__NodeId"
            fields.Add(LookService.DateField);
            fields.Add(LookService.NameField);
            fields.Add(LookService.LocationField);

            if (getHighlight != null || getText) // if a highlight function is supplied, then it'll need the text field to process
            {
                fields.Add(LookService.TextField);
            }

            if (getHighlight == null) // if highlight func does not exist, then create one to always return null
            {
                getHighlight = x => null;
            }

            if (getTags)
            {
                fields.Add(LookService.TagsField);
            }

            var mapFieldSelector = new MapFieldSelector(fields.ToArray());

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var docId = scoreDoc.doc;

                var doc = indexSearcher.Doc(docId, mapFieldSelector);

                DateTime? date = null;

                if (long.TryParse(doc.Get(LookService.DateField), out long ticks))
                {
                    date = new DateTime(ticks);
                }

                var lookMatch = new LookMatch(
                    Convert.ToInt32(doc.Get(LuceneIndexer.IndexNodeIdFieldName)),
                    getHighlight(doc.Get(LookService.TextField)),
                    getText ? doc.Get(LookService.TextField) : null,
                    getTags ? doc.Get(LookService.TagsField).Split(' ') : null,
                    date,
                    doc.Get(LookService.NameField),
                    doc.Get(LookService.LocationField) != null ? new Location(doc.Get(LookService.LocationField)) : null,
                    getDistance(docId),
                    scoreDoc.score
                );

                yield return lookMatch;
            }
        }
    }
}