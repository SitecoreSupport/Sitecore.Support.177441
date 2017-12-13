﻿namespace Sitecore.Support.Modules.EmailCampaign.Core.Links
{
    using System;
    using System.Text.RegularExpressions;
    using Sitecore.Modules.EmailCampaign.Core.Links;

    public class LinksManager
    {
        #region ChangedCode

        private static readonly Lazy<Regex> HrefRegex = new Lazy<Regex>(() =>
            new Regex(@"(href\s*=\s*)([""'])(.*?)\2", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        #endregion
        
        private static readonly Lazy<Regex> SrcRegex = new Lazy<Regex>(() =>
            new Regex(@"(src\s*=\s*)((""[^\s]+"")|('[^\s]+'))()", RegexOptions.Compiled | RegexOptions.IgnoreCase));
        private static readonly Lazy<Regex> TextRegex = new Lazy<Regex>(() =>
            new Regex(@"(?<!href\s*=\s*[""']|src\s*=\s*[""'])(https?:\/\/[^""'\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase));
        private static readonly Lazy<Regex> CssRegex = new Lazy<Regex>(() =>
            new Regex(@"(background\s*(-\s*image\s*)?\:\s*([^\s]+\s*)?url\s*\(\s*)((""[^\s]+"")|('[^\s]+')|([^""'\s]+))(\s*\))", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        private readonly LinkType _linkType;

        public const string UrlQueryKey = "ec_url";

        public LinksManager(string html, LinkType linkType)
        {
            Html = html;
            _linkType = linkType;
        }

        public static bool IsAbsoluteLink(string link)
        {
            if (link == "//")
            {
                return true;
            }

            Uri url;
            return Uri.TryCreate(link, UriKind.Absolute, out url);
        }

        public string Html { get; private set; }

        public string Replace(Func<string, string> func)
        {
            if (string.IsNullOrWhiteSpace(Html))
            {
                return Html;
            }

            if ((_linkType & LinkType.Href) == LinkType.Href)
            {
                Html = HrefRegex.Value.Replace(Html, match => ChangeHrefLink(match, func));
            }
            if ((_linkType & LinkType.Src) == LinkType.Src)
            {
                Html = SrcRegex.Value.Replace(Html, match => ChangeLink(match, 2, func, 1));
            }
            if ((_linkType & LinkType.Text) == LinkType.Text)
            {
                Html = TextRegex.Value.Replace(Html, match => ChangeLink(match, 1, func));
            }
            if ((_linkType & LinkType.Css) == LinkType.Css)
            {
                Html = CssRegex.Value.Replace(Html, match => ChangeLink(match, 4, func, 1, 8));
            }

            return Html;
        }

        #region AddedCode

        private static string ChangeHrefLink(Match match, Func<string, string> func)
        {
            var hrefAttribute = match.Groups[1].Value;
            var quote = match.Groups[2].Value;
            var linkValue = match.Groups[3].Value;
            var newLinkValue = func(linkValue);
            return newLinkValue != null ? $"{hrefAttribute}{quote}{newLinkValue}{quote}" : match.Value;
        }

        #endregion
        
        private static string ChangeLink(Match match, int linkGroupNumber, Func<string, string> func,
            int linkBeginGroupNumber = 0, int linkEndGroupNumber = 0)
        {
            var linkValue = match.Groups[linkGroupNumber].Value;
            var linkBeginValue = linkBeginGroupNumber > 0 ? match.Groups[linkBeginGroupNumber].Value : string.Empty;
            var linkEndValue = linkEndGroupNumber > 0 ? match.Groups[linkEndGroupNumber].Value : string.Empty;

            // In case if link wrapped in quotas.
            if (linkValue[0] == '\"' || linkValue[0] == '\'')
            {
                linkBeginValue += linkValue[0];
                linkEndValue = linkValue[linkValue.Length - 1] + linkEndValue;
                linkValue = linkValue.Substring(1, linkValue.Length - 2);
            }

            var newLinkValue = func(linkValue);
            return newLinkValue != null ? string.Format("{0}{1}{2}", linkBeginValue, newLinkValue, linkEndValue) : linkValue;
        }
    }
}
