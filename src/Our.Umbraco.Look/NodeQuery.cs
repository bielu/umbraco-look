﻿using Our.Umbraco.Look.Extensions;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;

namespace Our.Umbraco.Look
{
    public class NodeQuery
    {
        /// <summary>
        /// The types of node to find, eg. Content, Media, Members
        /// </summary>
        public PublishedItemType[] Types { get; set; } = null;

        /// <summary>
        /// Flags to indicate whether detached content should be returned
        /// </summary>
        public DetachedQuery DetachedQuery { get; set; } = DetachedQuery.IncludeDetached;

        /// <summary>
        /// The cultures of the content nodes to find
        /// </summary>
        public CultureInfo[] Cultures { get; set; } = null;

        /// <summary>
        /// The document type, media type or member type aliases
        /// </summary>
        public string[] Aliases { get; set; } = null;

        /// <summary>
        /// If not null, then each result returned must have a key in this collection (useful for finding detached content)
        /// </summary>
        public string[] Keys { get; set; } = null;

        /// <summary>
        /// Any umbraco ids that should be exlcuded from the results
        /// </summary>
        public int[] NotIds { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public string[] NotKeys { get; set; } = null;

        /// <summary>
        /// Create new empty NodeQuery search criteria
        /// </summary>
        public NodeQuery()
        {
        }

        /// <summary>
        /// Create new NodeQuery search criteria for nodes of a given type, eg content, media or members
        /// (detached items are associated with their host type)
        /// </summary>
        /// <param name="type">The node type. eg. content, media or member</param>
        public NodeQuery(PublishedItemType type)
        {
            this.Types = new PublishedItemType[] { type };
        }

        /// <summary>
        /// Create new NodeQuery search critera (specifying a culture implies that the PublishedItemType will be Content)
        /// </summary>
        /// <param name="cultureInfo"></param>
        public NodeQuery(CultureInfo cultureInfo)
        {
            this.Cultures = new CultureInfo[] { cultureInfo };
        }

        /// <summary>
        /// Create new NodeQuery search criteria for all nodes of any of these aliases
        /// </summary>
        /// <param name="aliases">array of string aliases for the content, media or members</param>
        public NodeQuery(params string[] aliases)
        {
            this.Aliases = aliases;
        }

        /// <summary>
        /// Create new NodeQuery search critera 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aliases"></param>
        public NodeQuery(PublishedItemType type, params string[] aliases)
        {
            this.Types = new PublishedItemType[] { type };
            this.Aliases = aliases;
        }

        /// <summary>
        /// Create new NodeQuery search critera 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <param name="aliases"></param>
        public NodeQuery(CultureInfo cultureInfo, params string[] aliases)
        {
            this.Cultures = new CultureInfo[] { cultureInfo };
            this.Aliases = aliases;
        }

        public override bool Equals(object obj)
        {
            NodeQuery nodeQuery = obj as NodeQuery;

            return nodeQuery != null
                && nodeQuery.Types.BothNullOrElementsEqual(this.Types)
                && nodeQuery.Cultures.BothNullOrElementsEqual(this.Cultures)
                && nodeQuery.Aliases.BothNullOrElementsEqual(this.Aliases)
                && nodeQuery.NotIds.BothNullOrElementsEqual(this.NotIds);
        }

        internal NodeQuery Clone()
        {
            return (NodeQuery)this.MemberwiseClone();
        }
    }
}