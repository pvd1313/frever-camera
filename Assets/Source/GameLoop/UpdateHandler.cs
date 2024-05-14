using System;
using UnityEngine;

namespace Frever.GameLoop
{
    public class UpdateHandler : MonoBehaviour
    {
        public event Action update;

        private void Update()
        {
            update?.Invoke();
        }
    }
}