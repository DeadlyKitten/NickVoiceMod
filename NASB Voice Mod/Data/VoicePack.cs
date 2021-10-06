using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VoiceMod.Utilities;

namespace VoiceMod.Data
{
    [System.Serializable]
    public class Voicepack
    {
        public string characterId;
        public Voiceclip[] voiceClips;
        public AudioGroup[] audioGroups = { };

        public string zipPath;

        private bool loaded = false;

        public string TempFolder => Path.Combine(Paths.CachePath, characterId);

        public void LoadClips()
        {
            if (loaded) return;

            SharedCoroutineStarter.StartCoroutine(LoadAllClips());
        }

        private IEnumerator LoadAllClips()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var coroutines = new List<Coroutine>();

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                Directory.CreateDirectory(TempFolder);

                for (int i = 0; i < voiceClips.Count(); i++)
                {
                    var clip = voiceClips[i];

                    var entry = archive.GetEntry(clip.path);

                    if (entry != null)
                    {
                        var tempLocation = ExtractToTempFile(clip.path, entry);

                        coroutines.Add(SharedCoroutineStarter.StartCoroutine(LoadClip(tempLocation, clip)));
                    }
                }
            }

            foreach (var coroutine in coroutines)
                yield return coroutine;

            try { Directory.Delete(TempFolder, true); }
            catch (System.Exception e)
            {
                Plugin.LogWarning($"Failed to delete temporary folder at {TempFolder}\n{e.Message}\n{e.StackTrace}");
            }

            stopwatch.Stop();
            Plugin.LogInfo($"Loaded {voiceClips.Length} audio clip{(voiceClips.Length == 1 ? "" : "s")} in {stopwatch.ElapsedMilliseconds} ms.");

            loaded = true;
        }

        private string ExtractToTempFile(string path, ZipArchiveEntry entry)
        {
            var tempLocation = Path.Combine(TempFolder, Path.GetFileName(path));

            if (File.Exists(tempLocation)) File.Delete(tempLocation);
            entry.ExtractToFile(tempLocation);
            return tempLocation;
        }

        private IEnumerator LoadClip(string path, Voiceclip voiceclip)
        {
            AudioType audioType;
                
            switch(Path.GetExtension(path).ToLower())
            {
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                case ".mp3":
                    audioType = AudioType.MPEG;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
                default:
                    yield break;
            }

            var loader = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            yield return loader.SendWebRequest();

            if (loader.error != null)
            {
                Debug.LogError($"Error loading clip from path: {path}\n{loader.error}");
                voiceClips = voiceClips.Where(x => x != voiceclip).ToArray();
                yield break;
            }

            voiceclip.clip = DownloadHandlerAudioClip.GetContent(loader);
        }

        public void Play(string id)
        {
            AudioGroup group;
            if ((group = audioGroups?.FirstOrDefault(x => x.name == id)) != null)
                id = group.GetRandomClipId();

            var clip = voiceClips.FirstOrDefault(x => x.id == id);

            clip?.Play();
        }
    }
}
