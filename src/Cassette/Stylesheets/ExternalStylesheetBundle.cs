﻿using System;
using System.Linq;
using System.Text;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Stylesheets.Manifests;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundle : StylesheetBundle, IExternalBundle, IBundleHtmlRenderer<StylesheetBundle>
    {
        readonly string url;
        bool isDebuggingEnabled;
        IBundleHtmlRenderer<StylesheetBundle> fallbackRenderer;

        public ExternalStylesheetBundle(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetBundle(string url, string applicationRelativePath) 
            : base(applicationRelativePath)
        {
            this.url = url;
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
            base.ProcessCore(settings);
            fallbackRenderer = Renderer;
            isDebuggingEnabled = settings.IsDebuggingEnabled;
            Renderer = this;
        }

        internal override bool ContainsPath(string pathToFind)
        {
            return base.ContainsPath(pathToFind) || url.Equals(pathToFind, StringComparison.OrdinalIgnoreCase);
        }

        internal override BundleManifest CreateBundleManifest(bool includeProcessedBundleContent)
        {
            var builder = new ExternalStylesheetBundleManifestBuilder { IncludeContent = includeProcessedBundleContent };
            return builder.BuildManifest(this);
        }

        public string ExternalUrl
        {
            get { return url; }
        }

        public string Render(StylesheetBundle unusedParameter)
        {
            if (isDebuggingEnabled && Assets.Any())
            {
                return fallbackRenderer.Render(this);
            }

            var conditionalRenderer = new ConditionalRenderer();

            return conditionalRenderer.Render(Condition, html =>
            {
                if (string.IsNullOrEmpty(Media))
                {
                    RenderLink(html);
                }
                else
                {
                    RenderLinkWithMedia(html);
                }
            });
        }

        void RenderLink(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.LinkHtml,
                url,
                HtmlAttributes.CombinedAttributes
                );
        }

        void RenderLinkWithMedia(StringBuilder html)
        {
            html.AppendFormat(
                HtmlConstants.LinkWithMediaHtml,
                url,
                Media,
                HtmlAttributes.CombinedAttributes
                );
        }
    }
}