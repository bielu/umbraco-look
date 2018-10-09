﻿using System;
using System.Web;

namespace Our.Umbraco.Look.Models
{
    public class LookMatch
    {
        /// <summary>
        /// The Umbraco (content, media or member) Id of the matched item
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Highlight text (containing search text) extracted from from the full text
        /// </summary>
        public IHtmlString Highlight { get; }

        /// <summary>
        /// The full text (only returned if specified)
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Tag collection (only returned if specified)
        /// </summary>
        public string[] Tags { get; }

        /// <summary>
        /// The custom date field
        /// </summary>
        public DateTime? Date { get; }

        /// <summary>
        /// The custom name field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The custom location (lat|lng) field
        /// </summary>
        public Location Location { get; }

        /// <summary>
        /// Temp field for calculated results
        /// </summary>
        public double? Distance { get; }

        /// <summary>
        /// The Lucene score for this match
        /// </summary>
        public float Score { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="highlight"></param>
        /// <param name="text"></param>
        /// <param name="tags"></param>
        /// <param name="date"></param>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="distance"></param>
        /// <param name="score"></param>
        internal LookMatch(
                    int id,
                    IHtmlString highlight,
                    string text,
                    string[] tags,
                    DateTime? date,
                    string name,
                    Location location,
                    double? distance,
                    float score)
        {
            this.Id = id;
            this.Highlight = highlight;
            this.Text = text;
            this.Tags = tags;
            this.Date = date;
            this.Name = name;
            this.Location = location;
            this.Distance = distance;
            this.Score = score;
        }
    }
}