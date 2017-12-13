﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Links;
using Sitecore.Modules.EmailCampaign.Core.Pipelines.GenerateLink;

namespace Sitecore.Support.Modules.EmailCampaign.Messages
{
    public class HtmlMail : Sitecore.Modules.EmailCampaign.Messages.HtmlMail
    {
        protected HtmlMail(Item item) : base(item)
        {
        }

        public new static HtmlMail FromItem(Item item)
        {
            return IsCorrectMessageItem(item) ? new HtmlMail(item) : null;
        }

        protected override string CorrectHtml(string html, bool preview)
        {
            var htmlHelper = new HtmlHelper(html);
            htmlHelper.CleanHtml();

            var startTime = DateTime.UtcNow;
            htmlHelper.InsertStyleSheets();
            Util.TraceTimeDiff("Insert style sheets", startTime);

            startTime = DateTime.UtcNow;
            var linksManager = new Sitecore.Support.Modules.EmailCampaign.Core.Links.LinksManager(htmlHelper.Html, LinkType.Href);
            html = linksManager.Replace(link =>
            {
                var args = new GenerateLinkPipelineArgs(link, this, preview, ManagerRoot.Settings.WebsiteSiteConfigurationName);
                PipelineHelper.RunPipeline(Sitecore.Modules.EmailCampaign.Core.Constants.ModifyHyperlinkPipeline, args);
                return args.Aborted ? null : args.GeneratedUrl;
            });
            Util.TraceTimeDiff("Modify 'href' links", startTime);

            startTime = DateTime.UtcNow;
            html = HtmlHelper.EncodeSrc(html);
            Util.TraceTimeDiff("Encode 'src' links", startTime);

            return html;
        }
    }
}