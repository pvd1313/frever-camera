using Zenject;

namespace Frever.Input
{
    public class InputInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputController>().AsSingle();
        }
    }
}