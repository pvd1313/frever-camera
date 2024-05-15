using UnityEngine;
using Zenject;

namespace Frever.UI.CameraRecording
{
    public class CameraRecordingUIInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private CameraRecordingUIConfig _config;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_config);

            Container.BindInterfacesAndSelfTo<CameraRecordingUIController>().AsSingle();
        }
    }
}