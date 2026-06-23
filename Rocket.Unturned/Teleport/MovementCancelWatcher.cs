using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rocket.Unturned.Teleport
{
    internal sealed class MovementCancelWatcher : MonoBehaviour
    {
        private sealed class TrackedPlayer
        {
            public SDG.Unturned.Player Player = null!;
            public System.Action MoveCallback = null!;
            public Vector3 Position;
            public float MoveMaxDistance;
        }

        public static MovementCancelWatcher Instance = null!;
        private readonly List<TrackedPlayer> players = new List<TrackedPlayer>();
        private float defaultMoveMaxDistance = 0.5f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InvokeRepeating(nameof(CheckMovement), 0.1f, 0.1f);
        }

        public void SetMoveMaxDistance(float distance)
        {
            defaultMoveMaxDistance = distance;
        }

        public void AddPlayer(SDG.Unturned.Player player, System.Action callback, float? moveMaxDistance = null)
        {
            players.Add(new TrackedPlayer
            {
                Player = player,
                MoveCallback = callback,
                Position = player.transform.position,
                MoveMaxDistance = moveMaxDistance ?? defaultMoveMaxDistance
            });
        }

        public void RemovePlayer(SDG.Unturned.Player player)
        {
            players.RemoveAll(x => x.Player == player);
        }

        private void CheckMovement()
        {
            foreach (TrackedPlayer tracked in players.ToList())
            {
                if (tracked.Player == null)
                {
                    players.Remove(tracked);
                    continue;
                }

                if (Vector3.Distance(tracked.Position, tracked.Player.transform.position) > tracked.MoveMaxDistance)
                {
                    tracked.MoveCallback.Invoke();
                    players.Remove(tracked);
                }
            }
        }
    }
}
