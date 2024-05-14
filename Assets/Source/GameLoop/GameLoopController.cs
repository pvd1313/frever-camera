using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frever.GameLoop
{
    public class GameLoopController : Zenject.IInitializable
    {
        private readonly IReadOnlyList<IInitializable> _initializables;
        private readonly IReadOnlyList<IUpdatable> _updatables;

        private UpdateHandler _updateHandler;
        private HashSet<IUpdatable> _brokenUpdatables;

        public GameLoopController(List<IInitializable> initializables, List<IUpdatable> updatables)
        {
            _initializables = initializables;
            _updatables = updatables;
        }

        public void Initialize()
        {
            _brokenUpdatables = new HashSet<IUpdatable>();
            
            _updateHandler = new GameObject("GameLoop").AddComponent<UpdateHandler>();
            _updateHandler.update += OnUpdate;

            for (int i = 0; i < _initializables.Count; i++)
            {
                try
                {
                    _initializables[i].Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnUpdate()
        {
            for (int i = 0; i < _updatables.Count; i++)
            {
                try
                {
                    _updatables[i].Update();
                }
                catch (Exception e)
                {
                    if (_brokenUpdatables.Add(_updatables[i]))
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}