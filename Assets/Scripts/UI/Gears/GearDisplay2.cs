using System;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearDisplay2 : GearBase
    {
        /// <summary>
        /// Pages involed in this gear.
        /// </summary>
        public string[] pages { get; set; }

        public int condition;

        private int _visible;

        public GearDisplay2(GObject owner)
            : base(owner)
        {
        }

        protected override void AddStatus(string pageId, ByteBuffer buffer)
        {
        }

        protected override void Init()
        {
            pages = null;
        }

        public override void Apply()
        {
            if (pages == null || pages.Length == 0
                              || Array.IndexOf(pages, _controller.selectedPageId) != -1)
                _visible = 1;
            else
                _visible = 0;
        }

        public override void UpdateState()
        {
        }

        public bool Evaluate(bool connected)
        {
            var v = _controller == null || _visible > 0;
            if (condition == 0)
                v = v && connected;
            else
                v = v || connected;
            return v;
        }
    }
}