using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    public readonly struct LiteEffectHandle
    {
        private readonly VisualElement element;

        internal LiteEffectHandle(VisualElement element)
        {
            this.element = element;
        }

        public bool IsValid => element != null;

        public void Update(LiteEffectSettings settings)
        {
            if (!IsValid)
            {
                return;
            }

            LiteEffectControllerRegistry.GetOrCreate(element).Apply(settings);
        }

        public void Refresh()
        {
            if (!IsValid)
            {
                return;
            }

            LiteEffectControllerRegistry.GetOrCreate(element).Refresh();
        }

        public void Clear()
        {
            if (!IsValid)
            {
                return;
            }

            LiteEffectControllerRegistry.GetOrCreate(element).ClearExplicit();
        }
    }
}
