using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core.Links;
using Sitecore.Modules.EmailCampaign.Core.Pipelines.GenerateLink;
using Sitecore.Modules.EmailCampaign.Messages;

namespace Sitecore.Support.Modules.EmailCampaign.Messages
{
    public class TextMail:Sitecore.Modules.EmailCampaign.Messages.TextMail
    {
        private readonly TextMailSource _curSource;
        protected TextMail(Item item) : base(item)
        {
            _curSource = Source as TextMailSource;
        }

        public new static TextMail FromItem(Item item)
        {
            return IsCorrectMessageItem(item) ? new TextMail(item) : null;
        }

        public override string GetMessageBody(bool preview)
        {
            var startTime = DateTime.UtcNow;
            var linksManager = new Sitecore.Support.Modules.EmailCampaign.Core.Links.LinksManager(_curSource.Body, LinkType.Href | LinkType.Text);
            var html = linksManager.Replace(link =>
            {
                var args = new GenerateLinkPipelineArgs(link, this, preview, ManagerRoot.Settings.WebsiteSiteConfigurationName);
                PipelineHelper.RunPipeline(Sitecore.Modules.EmailCampaign.Core.Constants.ModifyHyperlinkPipeline, args);
                return args.Aborted ? null : args.GeneratedUrl;
            });
            Util.TraceTimeDiff("Modify 'href' and 'text' links", startTime);
            return html;
        }
    }
}