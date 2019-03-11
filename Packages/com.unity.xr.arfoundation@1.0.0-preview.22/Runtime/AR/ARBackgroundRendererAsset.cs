namespace UnityEngine.XR.ARFoundation
{
    public abstract class ARBackgroundRendererAsset : ScriptableObject
    {
        public abstract ARFoundationBackgroundRenderer CreateARBackgroundRenderer();

        public abstract void CreateHelperComponents(GameObject gameObject);

        public abstract Material CreateCustomMaterial();

    }
}
