using System;

namespace PlinePager.Tools
{
    public static class PlineHelper
    {
        public static string ActiveLinkAjax(string href, string text, bool downloadLink, bool openNewTab)
        {
            return
                $"<a {(openNewTab ? "target=\"_blank\"" : "")} href=\"{href}\" {(downloadLink ? "download" : "")}>{text}</a>";
        }

        public static string ActiveLinkAjax(string href, string text, string css, string title = "")
        {
            return $"<a href=\"{href}\" class=\"{css}\" title=\"{title}\">{text}</a>";
        }

        public static string ActionLink(string text, string action, string controller, object id, string css,
            string title = "")
        {
            if (id == null)
                return $"<a href=\"{controller}/{action}\" class=\"{css}\" title=\"{title}\">{text}</a>";
            return $"<a href=\"{controller}/{action}/{id}\" class=\"{css}\" title=\"{title}\">{text}</a>";
        }
    }
}