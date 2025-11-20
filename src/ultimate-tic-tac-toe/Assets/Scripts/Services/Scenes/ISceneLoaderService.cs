using System;

namespace Services.Scenes
{
    public interface ISceneLoaderService
    {
        void LoadSceneAsync(string sceneName, Action onLoaded = null);
        void LoadScene(string sceneName, Action onLoaded = null);
    }
}


