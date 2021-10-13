using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceMod.Data;

namespace VoiceMod.Managers
{
    class VoicepackManager
    {
        private readonly AudioSource audioPlayer;

        public static VoicepackManager Instance
        {
            get;
            private set;
        }

        public static string folder = Path.Combine(Paths.BepInExRootPath, "Voicepacks");

        public List<Voicepack> voicepacks = new List<Voicepack>();

        public VoicepackManager()
        {
            Instance = this;

            audioPlayer = new GameObject("VoiceMod Audio Player").AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(audioPlayer.gameObject);

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            Task.Run(() => LoadAllVoicepacks());
        }

        private void LoadAllVoicepacks()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            string[] supportedFileTypes = { ".zip", ".voicepack" };
            var files = Directory.GetFiles(folder).Where(x => supportedFileTypes.Contains(Path.GetExtension(x).ToLower()));

            foreach (var subFolder in Directory.GetDirectories(folder))
            {
                files = files.Concat(Directory.GetFiles(subFolder).Where(x => supportedFileTypes.Contains(Path.GetExtension(x).ToLower())));
            }

            foreach (var file in files)
            {
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(file))
                    {
                        var jsonEntry = archive.Entries.FirstOrDefault(i => i.Name == "package.json");
                        if (jsonEntry == null)
                        {
                            ThreadingHelper.Instance.StartSyncInvoke(() =>
                            {
                                Plugin.LogWarning($"Missing 'package.json' in: {Path.GetFileName(file)}\nSkipping!");
                            });

                            continue;
                        }

                        Voicepack json = null;
                        if (jsonEntry != null)
                        {
                            var stream = new StreamReader(jsonEntry.Open(), Encoding.Default);
                            var jsonString = stream.ReadToEnd();
                            json = JsonConvert.DeserializeObject<Voicepack>(jsonString);
                            json.zipPath = file;
                        }

                        if (!(json == null) && json.voiceClips.Any()) voicepacks.Add(json);
                    }
                }
                catch (Exception e)
                {
                    ThreadingHelper.Instance.StartSyncInvoke(() =>
                    {
                        Plugin.LogError($"Error loading voicepack from {file}\n{e.Message}\n{e.StackTrace}");
                    });
                }
            }

            if (Plugin.PreloadAllClips.Value)
            {
                foreach (var voicepack in voicepacks)
                    voicepack.LoadClips();
            }

            stopwatch.Stop();
            ThreadingHelper.Instance.StartSyncInvoke(() =>
            {
                Plugin.LogInfo($"Loaded {voicepacks.Count()} voice pack{(voicepacks.Count() == 1 ? "" : "s")} in {stopwatch.ElapsedMilliseconds} ms.");
            });
        }

        public bool TryGetVoicepackWithId(string id, out Voicepack result)
        {
            result = voicepacks.FirstOrDefault(x => x.characterId == id);

            return result != null;
        }

        public void PlayClip(AudioClip clip, float volume) => audioPlayer.PlayOneShot(clip, volume);
    }
}
