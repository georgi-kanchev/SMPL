﻿using SFML.Audio;
using SFML.Graphics;
using System.Collections.Generic;
using System.Threading;

namespace SMPL
{
	public class Scene
	{
		public struct AssetQueue
		{
			public List<string> Textures { get; set; }
			public List<string> Music { get; set; }
			public List<string> Sounds { get; set; }
			public List<string> Fonts { get; set; }
		}

		private static Scene scene, loadScene, unloadScene, startScene, stopScene;
		private static Thread assetsLoading;

		public static Scene CurrentScene
		{
			get => scene;
			set
			{
				unloadScene = scene;
				scene = value;
				LoadingScene?.OnStart();
				loadScene = value;
			}
		}
		public static Scene LoadingScene { get; set; }
		public float LoadingPercent { get; private set; } = 100;
		public Color BackgroundColor { get; set; }

		protected Dictionary<string, Texture> Textures { get; } = new();
		protected Dictionary<string, Music> Music { get; } = new();
		protected Dictionary<string, Sound> Sounds { get; } = new();
		protected Dictionary<string, Font> Fonts { get; } = new();

		protected virtual AssetQueue OnRequireAssets() => default;
		protected virtual void OnStart() { }
		protected virtual void OnUpdate() { }
		protected virtual void OnStop() { }
		protected virtual void OnGameStop() { }

		internal void LoadAssets()
		{
			var assets = OnRequireAssets();
			var loadedCount = 0;

			for (int i = 0; i < assets.Textures?.Count; i++)
			{
				Textures[assets.Textures[i]] = new Texture(assets.Textures[i]);
				UpdateLoadingPercent();
			}
			for (int i = 0; i < assets.Sounds?.Count; i++)
			{
				Sounds[assets.Sounds[i]] = new Sound(new SoundBuffer(assets.Sounds[i]));
				UpdateLoadingPercent();
			}
			for (int i = 0; i < assets.Music?.Count; i++)
			{
				Music[assets.Music[i]] = new Music(assets.Music[i]);
				UpdateLoadingPercent();
			}
			for (int i = 0; i < assets.Fonts?.Count; i++)
			{
				Fonts[assets.Fonts[i]] = new Font(assets.Fonts[i]);
				UpdateLoadingPercent();
			}

			void UpdateLoadingPercent()
			{
				loadedCount++;

				var total = GetCount(assets.Fonts) + GetCount(assets.Music) + GetCount(assets.Sounds) + GetCount(assets.Textures);
				CurrentScene.LoadingPercent = loadedCount.Map(0, total, 0, 100);

				int GetCount(List<string> list) => list?.Count ?? 0;
			}
		}
		internal void UnloadAssets()
		{
			foreach (var kvp in Textures)
				kvp.Value.Dispose();

			Textures.Clear();
		}
		internal void GameStop() => OnGameStop();

		internal static void Init(Scene startingScene, Scene loadingScene)
		{
			LoadingScene = loadingScene;
			CurrentScene = startingScene;

			assetsLoading = new(ThreadLoadAssets) { IsBackground = true, Name = "AssetsLoading" };
			assetsLoading.Start();
		}
		internal static void UpdateCurrentScene()
		{
			if (startScene != null)
			{
				LoadingScene?.OnStop();
				startScene = null;
				CurrentScene?.OnStart();
			}
			if (stopScene != null)
			{
				stopScene = null;
				CurrentScene?.OnStop();
			}
			if (CurrentScene?.LoadingPercent < 100)
				LoadingScene?.OnUpdate();
			else
				CurrentScene?.OnUpdate();
		}
		internal static void ThreadLoadAssets()
		{
			while (true)
			{
				if (loadScene != null)
				{
					scene.LoadAssets();
					startScene = loadScene;
					loadScene = null;
				}
				if (unloadScene != null)
				{
					scene.UnloadAssets();
					stopScene = unloadScene;
					unloadScene = null;
				}
			}
		}
	}
}
