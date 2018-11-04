﻿using Our.Umbraco.Look.Models;
using System;
using Umbraco.Core.Logging;

namespace Our.Umbraco.Look.Services
{
    public partial class LookService
    {
        /// <summary>
        /// Register consumer code to perform when indexing text
        /// </summary>
        /// <param name="textFunc"></param>
        public static void SetTextIndexer(Func<IndexingContext, string> textFunc)
        {
            LogHelper.Info(typeof(LookService), "Text indexing function set");

            LookService.Instance.TextIndexer = textFunc;
        }
    }
}