using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Sitecore.Modules.EmailCampaign.Core.Links;

namespace Sitecore.Support.Modules.EmailCampaign.Core.Links
{
    class LinksManager
    {
        private static readonly Lazy<Regex> HrefRegex = new Lazy<Regex>(() => new Regex("(href\\s*=\\s*)((\".*?\")|('.*?'))", RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private readonly LinkType _linkType;

        public string Html
        {
            get;
            private set;
        }

        public LinksManager(string html, LinkType linkType)
        {
            this.Html = html;
            this._linkType = linkType;
        }

        public string Replace(Func<string, string> func)
        {
            if (string.IsNullOrWhiteSpace(this.Html))
            {
                return this.Html;
            }

            this.Html = LinksManager.HrefRegex.Value.Replace(this.Html, (Match match) => LinksManager.ChangeLink(match, 2, func, 1, 0));
            return this.Html;
        }

        private static string ChangeLink(Match match, int linkGroupNumber, Func<string, string> func, int linkBeginGroupNumber = 0, int linkEndGroupNumber = 0)
        {
            if (match.Value.Contains("sitecore/RedirectUrlPage.aspx")) return match.Value;

            string text = match.Groups[linkGroupNumber].Value;
            string arg = (linkBeginGroupNumber > 0) ? match.Groups[linkBeginGroupNumber].Value : string.Empty;
            string text2 = (linkEndGroupNumber > 0) ? match.Groups[linkEndGroupNumber].Value : string.Empty;
            if (text[0] == '"' || text[0] == '\'')
            {
                arg += text[0];
                text2 = text[text.Length - 1] + text2;
                text = text.Substring(1, text.Length - 2);
            }
            string text3 = func(text);
            if (text3 == null)
            {
                return text;
            }
            return string.Format("{0}{1}{2}", arg, text3, text2);
        }
    }
}
