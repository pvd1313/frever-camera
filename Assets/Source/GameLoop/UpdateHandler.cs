using System;
using UnityEngine;

namespace Frever.GameLoop
{
    public class UpdateHandler : MonoBehaviour
    {
        public event Action update;
        public event Action drawGizmos;

        private void Update()
        {
            update?.Invoke();
        }

        private void OnDrawGizmos()
        {
            drawGizmos?.Invoke();
        }
    }
}