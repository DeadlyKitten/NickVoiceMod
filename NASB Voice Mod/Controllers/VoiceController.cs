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
        private GameAgentStateMachine stateMachine;
        private Voicepack voicepack;
        private readonly Dictionary<int, string> lookup = new Dictionary<int, string>();

        void Start()
        {
            agentId = GetComponent<GameAgent>()?.GameUniqueIdentifier;
            stateMachine = GetComponent<GameAgentStateMachine>();
            var stateIdDict = stateMachine.GetPrivateField<Dictionary<string, int>>("stateIdDict");

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
                    voicepack.Play(id);
                }
            }
            stateLastFrame = stateMachine.CurrentStateId;
        }
    }
}
