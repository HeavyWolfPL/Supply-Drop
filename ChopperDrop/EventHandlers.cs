using System;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Exiled.API.Features;
using Respawning;
using Map = Exiled.API.Features.Map;
using Object = UnityEngine.Object;
using Exiled.API.Features.Items;

namespace ChopperDrop
{
    public class EventHandlers
    {
        public Plugin<Config> pl;

        public int time;
        public int minPlayers;
        public ushort bcTime;
        public string dropText;
        public List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

        public bool roundStarted = false;
        public Dictionary<ItemType, int> allowedItems;
        public EventHandlers(Plugin<Config> plugin, Dictionary<ItemType, int> drops, int tim, string dropTex, int minPly, ushort bcTimee)
        {
            pl = plugin;
            allowedItems = drops;
            time = tim;
            dropText = dropTex;
            minPlayers = minPly;
            bcTime = bcTimee;
    }

        internal void RoundStart()
        {
            roundStarted = true;
            foreach (CoroutineHandle handle in coroutines)
                Timing.KillCoroutines(handle);
            Log.Info("Starting Chopper Thread.");
            coroutines.Add(Timing.RunCoroutine(ChopperThread(), "ChopperThread"));
        }

        internal void WaitingForPlayers()
        {
            foreach (CoroutineHandle handle in coroutines)
                Timing.KillCoroutines(handle);
        }

        public IEnumerator<float> ChopperThread()
        {
            while (roundStarted)
            {
                var playerCount = PlayerManager.players.Count;

                if (playerCount >= minPlayers)
                {
                    yield return Timing.WaitForSeconds(time); // Wait seconds (10 minutes by defualt)
                    Log.Info("Spawning chopper!");

                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.NineTailedFox);

                    Map.Broadcast(bcTime, dropText);

                    yield return Timing.WaitForSeconds(15); // Wait 15 seconds

                    Vector3 spawn = GetRandomSpawnPoint(RoleType.NtfPrivate);

                    foreach (KeyValuePair<ItemType, int> drop in allowedItems) // Drop items
                    {
                        Log.Info("Spawning " + drop.Value + " " + drop.Key.ToString() + "'s");
                        for (int i = 0; i < drop.Value; i++)
                            SpawnItem(drop.Key, spawn);
                    }
                    yield return Timing.WaitForSeconds(15); // Wait 15 seconds to let the chopper leave.
                }
            }
        }

        public static Vector3 GetRandomSpawnPoint(RoleType roleType)
        {
            GameObject randomPosition = Object.FindObjectOfType<SpawnpointManager>().GetRandomPosition(roleType);

            return randomPosition == null ? Vector3.zero : randomPosition.transform.position;
        }

        public static void SpawnItem(ItemType type, Vector3 pos)
        {
            Item item = new Item(type);
            item.Spawn(pos, default);
        }
    }
}
