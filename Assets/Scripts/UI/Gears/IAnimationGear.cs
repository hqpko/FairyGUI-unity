
namespace FairyGUI
{

    public interface IAnimationGear
    {

        bool playing { get; set; }


        int frame { get; set; }


        float timeScale { get; set; }


        bool ignoreEngineTimeScale { get; set; }


        /// <param name="time"></param>
        void Advance(float time);
    }
}
