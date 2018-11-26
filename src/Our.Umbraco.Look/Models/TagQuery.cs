﻿using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.Look.Models
{
    public class TagQuery
    {
        public LookTag[] All { get; set; }

        public LookTag[] Any { get; set; }

        public LookTag[] Not { get; set; }

        /// <summary>
        /// when null, facets are not calculated, but when string[], each string value represents the tag group field to facet on, the empty string or whitespace = empty group
        /// The count value for a returned tag indicates how may results would be expected should that tag be added into the AllTags collection of this query
        /// </summary>
        public string[] GetFacets { get; set; }

        /// <summary>
        /// Create a new TagQuery
        /// </summary>
        /// <param name="all">All of these tags</param>
        /// <param name="any">Any of these tags</param>
        /// <param name="not">None of these tags</param>
        /// <param name="getFacets">string array of tag groups to return facet counts for</param>
        public TagQuery(LookTag[] all = null, LookTag[] any = null, LookTag[] not = null, string[] getFacets = null)
        {
            this.All = all;
            this.Any = any;
            this.Not = not;
            this.GetFacets = getFacets;
        }

        /// <summary>
        /// Helper to simplify the construction of LookTag array, by being able to supply a raw collection of tag strings
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static LookTag[] MakeTags(params string[] tags)
        {
            List<LookTag> lookTags = new List<LookTag>();

            if (tags != null)
            {
                foreach(var tag in tags)
                {
                    lookTags.Add(new LookTag(tag));
                }
            }

            return lookTags.ToArray();
        }

        public static LookTag[] MakeTags(IEnumerable<string> tags)
        {
            return TagQuery.MakeTags(tags.ToArray());
        }

        public override bool Equals(object obj)
        {
            var tagQuery = obj as TagQuery;

            return tagQuery != null
                && ((tagQuery.All == null && this.All == null) || (tagQuery.All != null && this.All != null && tagQuery.All.SequenceEqual(this.All)))
                && ((tagQuery.Any == null && this.Any == null) || (tagQuery.Any != null && this.Any != null && tagQuery.Any.SequenceEqual(this.Any)))
                && ((tagQuery.Not == null && this.Not == null) || (tagQuery.Not != null && this.Not != null && tagQuery.Not.SequenceEqual(this.Not)))
                && tagQuery.GetFacets == this.GetFacets;
        }

        internal TagQuery Clone()
        {
            return (TagQuery)this.MemberwiseClone();
        }
    }
}
