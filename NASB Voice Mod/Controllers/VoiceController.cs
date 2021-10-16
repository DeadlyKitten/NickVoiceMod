using Nick;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoiceMod.Data;
using VoiceMod.Managers;
using VoiceMod.Utilities;

namespace VoiceMod.Controllers
{
    class VoiceController : MonoBehaviour
    {
        private string agentId;
        private GameAgent agent;
        private GameAgentStateMachine stateMachine;
        private Voicepack voicepack;
        private readonly Dictionary<int, string> lookup = new Dictionary<int, string>();
        private Dictionary<string, int> stateIdDict;

        void Start()
        {
            agent = GetComponent<GameAgent>();
            agentId = agent?.GameUniqueIdentifier;
            stateMachine = GetComponent<GameAgentStateMachine>();
            stateIdDict = stateMachine.GetPrivateField<Dictionary<string, int>>("stateIdDict");

            if (agentId == null ||
                stateMachine == null ||
                !VoicepackManager.Instance.TryGetVoicepackWithId(agentId, out voicepack) ||
                stateIdDict == null)
            {
                DestroyImmediate(this);
                return;
            }

            voicepack.LoadClips();


            foreach (var clip in voicepack.voiceClips)
            {
                if(stateIdDict.TryGetValue(clip.id, out var id))
                {
                    if (!lookup.TryGetValue(id, out _))
                    {
                        lookup.Add(id, clip.id);
                    }
                }
            }

            foreach (var group in voicepack.audioGroups)
            {
                foreach (var move in group.moves)
                {
                    if (stateIdDict.TryGetValue(move, out var id))
                    {
                        if (!lookup.TryGetValue(id, out _))
                            lookup.Add(id, group.name);
                    }
                }
            }
        }

        private int stateLastFrame = -1;
        void Update()
        {
            if (stateMachine.CurrentStateId != stateLastFrame)
            {
                if (lookup.TryGetValue(stateMachine.CurrentStateId, out var id))
                {
                    int entryId = stateIdDict["entrance"];
                    if (stateMachine.CurrentStateId == entryId)
                        Invoke("PlayEntranceAudio", agent.playerIndex * 1.25f);
                    else voicepack.Play(id);
                }
            }
            stateLastFrame = stateMachine.CurrentStateId;
        }

        private void PlayEntranceAudio()
        {
            //All the checking and tryGets were done already. We _know_ this exists or the code wouldn't have gotten this far
            string id = lookup[stateIdDict["entrance"]];
            voicepack.Play(id);
        }
    }
}
