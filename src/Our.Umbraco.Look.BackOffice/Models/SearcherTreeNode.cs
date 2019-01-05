﻿using Examine.Providers;
using Our.Umbraco.Look.BackOffice.Interfaces;
using System.Linq;
using Umbraco.Core;

namespace Our.Umbraco.Look.BackOffice.Models
{
    internal class SearcherTreeNode : BaseTreeNode
    {
        public override string Name { get; }

        public override string Icon { get; } // could be "icon-file-cabinet, icon-files, icon-categories

        private BaseSearchProvider BaseSearchProvider { get; }

        /// <summary>
        /// Flag to indicate whether Look is 'hooked' in with this Examine provider or if this is a Look provider
        /// </summary>
        private bool Active { get; } = false;

        internal SearcherTreeNode(BaseSearchProvider baseSearchProvider) : base("searcher-" + baseSearchProvider.Name)
        {
            this.BaseSearchProvider = baseSearchProvider;

            this.Name = baseSearchProvider.Name;

            if (baseSearchProvider is LookSearcher)
            {
                this.Active = true;
                this.Icon = "icon-files";
            }
            else // must be an examine one
            {
                var name = baseSearchProvider.Name.TrimEnd("Searcher");

                if (LookConfiguration.ExamineIndexers.Select(x => x.TrimEnd("Indexer")).Any(x => x == name))
                {
                    this.Active = true;
                    this.Icon = "icon-categories";
                }
                else
                {
                    this.Icon = "icon-file-cabinet";
                }
            }
        }

        public override ILookTreeNode[] Children
        {
            get
            {
                if (this.Active)
                {
                    return new ILookTreeNode[] { new TagsTreeNode(this.BaseSearchProvider.Name) };
                }

                return base.Children; // empty
            }
        }
    }
}