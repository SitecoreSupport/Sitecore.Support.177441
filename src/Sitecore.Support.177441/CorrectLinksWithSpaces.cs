using System;
using Sitecore.Modules.EmailCampaign.Core.Pipelines;
using Sitecore.Support.Modules.EmailCampaign.Core.Links;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Pipelines.GenerateLink;
using Sitecore.Modules.EmailCampaign.Core.Links;
using Sitecore.Modules.EmailCampaign.Factories;
using System.Reflection;

namespace Sitecore.Support.EmailCampaign.Cm.Pipelines.SendEmail
{
    class CorrectLinksWithSpaces
    {
        public void Process(SendMessageArgs args)
        {
            args.EcmMessage.Body = CorrectHtml(args.EcmMessage.Body, false, args.EcmMessage as MessageItem);
        }

        public static string CorrectHtml(string html, bool preview, MessageItem messageItem)
        {
            HtmlHelper htmlHelper = new HtmlHelper(html);
            MailMessageItem messItem = messageItem as MailMessageItem;
            Type blType = typeof(BusinessLogicFactory);
            PropertyInfo pipInfo = blType.GetProperty("PipelineHelper", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance);
            PipelineHelper pipHelper = pipInfo.GetValue(EcmFactory.GetDefaultFactory().Bl) as PipelineHelper;
            Sitecore.Support.Modules.EmailCampaign.Core.Links.LinksManager linkManager = new Sitecore.Support.Modules.EmailCampaign.Core.Links.LinksManager(htmlHelper.Html, LinkType.Href);
            html = linkManager.Replace(delegate (string link)
            {
                GenerateLinkPipelineArgs generateLinkPipelineArgs = new GenerateLinkPipelineArgs(link, messItem, preview, messageItem.ManagerRoot.Settings.WebsiteSiteConfigurationName);
                pipHelper.RunPipeline("modifyHyperlink", generateLinkPipelineArgs);
                if (!generateLinkPipelineArgs.Aborted)
                {
                    return generateLinkPipelineArgs.GeneratedUrl;
                }
                return null;
            });
            return html;
        }
    }
}
