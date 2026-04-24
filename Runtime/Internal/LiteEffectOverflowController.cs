using System;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectOverflowController : IDisposable
    {
        private readonly VisualElement element;
        private VisualElement host;
        private StyleEnum<Overflow> originalInlineOverflow;
        private bool overflowCaptured;
        private bool expanded;

        public LiteEffectOverflowController(VisualElement element)
        {
            this.element = element;
        }

        public void SetExpanded(bool shouldExpand)
        {
            var parent = element.parent;
            if (parent == null)
            {
                RestoreOverflow();
                return;
            }

            if (host != null && host != parent)
            {
                RestoreOverflow();
            }

            host = parent;
            if (!overflowCaptured)
            {
                originalInlineOverflow = host.style.overflow;
                overflowCaptured = true;
            }

            if (shouldExpand)
            {
                if (!expanded)
                {
                    expanded = true;
                    host.style.overflow = Overflow.Visible;
                }

                return;
            }

            RestoreOverflow();
        }

        public void Dispose()
        {
            RestoreOverflow();
        }

        private void RestoreOverflow()
        {
            if (!expanded)
            {
                host = element.parent;
                return;
            }

            expanded = false;
            if (host != null)
            {
                host.style.overflow = originalInlineOverflow;
            }

            host = element.parent;
        }
    }
}
