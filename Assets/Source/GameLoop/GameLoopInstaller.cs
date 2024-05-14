using Zenject;

namespace Frever.GameLoop
{
    public class GameLoopInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameLoopController>().AsSingle();
        }
    }
}