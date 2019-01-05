﻿using Our.Umbraco.Look.BackOffice.Services;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Our.Umbraco.Look.BackOffice.Controllers
{
    [Tree("developer", "lookTree", "Look", "icon-zoom-in")]
    public class LookTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = new TreeNodeCollection();

            var treeNode = LookTreeService.MakeLookTreeNode(id);

            foreach(var childTreeNode in treeNode.Children)
            {
                tree.Add(
                    this.CreateTreeNode(
                            childTreeNode.Id, 
                            id, 
                            queryStrings, 
                            childTreeNode.Name, 
                            childTreeNode.Icon, 
                            childTreeNode.Children.Any()));
            }

            return tree;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            MenuItemCollection nodeMenu = new MenuItemCollection();

            return nodeMenu;
        }
    }
}