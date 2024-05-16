using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frever.GameLoop
{
    public class GameLoopController : Zenject.IInitializable
    {
        private readonly IReadOnlyList<IInitializable> _initializables;
        private readonly IReadOnlyList<IUpdatable> _updatables;
        private readonly List<IGizmosDrawer> _gizmosDrawers;

        private UpdateHandler _updateHandler;
        private HashSet<IUpdatable> _brokenUpdatables;

        public GameLoopController(List<IInitializable> initializables, 
                                  List<IUpdatable> updatables,
                                  List<IGizmosDrawer> gizmosDrawers)
        {
            _initializables = initializables;
            _updatables = updatables;
            _gizmosDrawers = gizmosDrawers;
        }

        public void Initialize()
        {
            _brokenUpdatables = new HashSet<IUpdatable>();
            
            _updateHandler = new GameObject("GameLoop").AddComponent<UpdateHandler>();
            _updateHandler.update += OnUpdate;
            _updateHandler.drawGizmos += OnDrawGizmos;

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

        private void OnDrawGizmos()
        {
            for (int i = 0; i < _gizmosDrawers.Count; i++)
            {
                try
                {
                    _gizmosDrawers[i].DrawGizmos();
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