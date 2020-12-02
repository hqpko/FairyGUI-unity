namespace FairyGUI
{
    public delegate void UILoadCallback();

    public interface IUISource
    {
        string fileName { get; set; }

        bool loaded { get; }

        /// <param name="callback"></param>
        void Load(UILoadCallback callback);
    }
}