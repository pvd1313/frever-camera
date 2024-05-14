using UnityEngine;
using Zenject;

namespace Frever.Cinematics
{
    public class CinematicsInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private CameraConfig _cameraConfig;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_cameraConfig);
            
            Container.BindInterfacesAndSelfTo<CameraController>().AsSingle();
        }
    }
}