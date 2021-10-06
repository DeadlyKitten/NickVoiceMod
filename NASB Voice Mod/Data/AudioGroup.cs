using VoiceMod.Extensions;

namespace VoiceMod.Data
{
    [System.Serializable]
    public class AudioGroup
    {
        public string name;
        public AudioGroupItem[] clips;
        public string[] moves;

        public string GetRandomClipId() => clips.GetRandomItem(x => x.weight).id;

        [System.Serializable]
        public class AudioGroupItem
        {
            public string id;
            public float weight = 1;
        }
    }
}
